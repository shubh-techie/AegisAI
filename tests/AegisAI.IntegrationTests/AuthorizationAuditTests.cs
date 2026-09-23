using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AegisAI.Api.Observability;
using AegisAI.Application.Authorization;
using AegisAI.Application.Observability;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AegisAI.IntegrationTests;

public sealed partial class AuthorizationEndpointTests
{
    [Theory]
    [InlineData("A", "0", "alice", "ALLOW")]
    [InlineData("A", "0", "bob", "DENY")]
    [InlineData("B", "0", "alice", "ALLOW")]
    [InlineData("B", "0", "bob", "DENY")]
    [InlineData("C", "0", "alice", "ALLOW")]
    [InlineData("C", "0.25", "alice", "STEP_UP")]
    [InlineData("C", "0.5", "alice", "LIMIT")]
    [InlineData("C", "0.75", "alice", "DENY")]
    [InlineData("C", "0", "bob", "DENY")]
    public async Task Exactly_one_safe_event_matches_the_selected_model(string model, string risk, string subject, string outcome)
    {
        var sink = new RecordingAuditSink();
        using var configured = CreateFactory(ModelCConfiguration(risk));
        using var factory = configured.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddSingleton<IAuthorizationAuditSink>(sink)));
        using var client = factory.CreateClient();
        using var authenticated = Client(subject);
        client.DefaultRequestHeaders.Authorization = authenticated.DefaultRequestHeaders.Authorization;
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "credential-canary-do-not-log");
        client.DefaultRequestHeaders.Add("Cookie", "secret=credential-canary-do-not-log");
        var before = DateTimeOffset.UtcNow;
        var path = model == "A" ? "/authorization/evaluate" : $"/authorization/evaluate/model-{model.ToLowerInvariant()}";
        using var response = await client.PostAsJsonAsync(path, new { resource = "reports", action = "read" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(outcome, (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("outcome").GetString());
        var audit = Assert.Single(sink.Events);
        Assert.Equal(outcome, audit.Decision);
        Assert.Equal(model, audit.Model);
        Assert.Equal(AuthorizationObservation.Pseudonym(Issuer, subject), audit.SubjectId);
        Assert.Equal(AuthorizationObservation.Pseudonym("reports"), audit.ResourceId);
        Assert.Equal(AuthorizationObservation.Pseudonym("read"), audit.ActionId);
        Assert.Equal(response.Headers.GetValues("X-Correlation-ID").Single(), audit.CorrelationId);
        Assert.True(Guid.TryParseExact(audit.CorrelationId, "N", out _));
        Assert.InRange(audit.Timestamp, before, DateTimeOffset.UtcNow);
        Assert.True(audit.ProcessingDurationMs >= 0);
        Assert.NotEmpty(audit.Reasons);
        if (model == "C" && subject == "alice")
            Assert.Equal(decimal.Parse(risk, System.Globalization.CultureInfo.InvariantCulture), audit.RiskScore);
        else Assert.Null(audit.RiskScore);
        var json = JsonSerializer.Serialize(audit);
        Assert.DoesNotContain("credential-canary", json);
        Assert.DoesNotContain(authenticated.DefaultRequestHeaders.Authorization!.Parameter!, json);
        Assert.DoesNotContain(Issuer, json);
    }

    [Fact]
    public async Task Failed_sink_does_not_change_response_and_invalid_requests_do_not_emit_decisions()
    {
        var sink = new RecordingAuditSink { Throw = true };
        using var configured = CreateFactory(ModelCConfiguration("0.5"));
        using var factory = configured.WithWebHostBuilder(builder => builder.ConfigureServices(services => services.AddSingleton<IAuthorizationAuditSink>(sink)));
        using var client = factory.CreateClient();
        using var auth = Client();
        client.DefaultRequestHeaders.Authorization = auth.DefaultRequestHeaders.Authorization;
        using var response = await client.PostAsJsonAsync("/authorization/evaluate/model-c", new { resource = "reports", action = "read" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("LIMIT", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("outcome").GetString());
        Assert.Single(sink.Events);
        using var invalid = await client.PostAsJsonAsync("/authorization/evaluate", new { resource = "", action = "read" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        client.DefaultRequestHeaders.Authorization = null;
        using var unauthorized = await client.PostAsJsonAsync("/authorization/evaluate", new { resource = "reports", action = "read" });
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        Assert.Single(sink.Events);
    }

    [Fact]
    public void Traces_metrics_and_audit_are_correlated_without_sensitive_tags()
    {
        var spans = new ConcurrentQueue<Activity>();
        var measurements = new ConcurrentQueue<(string Name, string[] Tags)>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == AuthorizationObservation.InstrumentationName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = spans.Enqueue
        };
        ActivitySource.AddActivityListener(listener);
        using var meter = new MeterListener();
        meter.InstrumentPublished = (instrument, l) =>
        {
            if (instrument.Meter.Name == AuthorizationObservation.InstrumentationName) l.EnableMeasurementEvents(instrument);
        };
        meter.SetMeasurementEventCallback<long>((instrument, _, tags, _) => measurements.Enqueue((instrument.Name, tags.ToArray().Select(t => t.Key).ToArray())));
        meter.SetMeasurementEventCallback<double>((instrument, _, tags, _) => measurements.Enqueue((instrument.Name, tags.ToArray().Select(t => t.Key).ToArray())));
        meter.Start();
        var sink = new RecordingAuditSink();
        var observer = new AuthorizationObservation(sink, new HttpContextAccessor());
        var request = new AuthorizationRequest(new("secret-subject", "secret-issuer"), new("secret-resource"), new("secret-action"));
        var expected = new AuthorizationDecision(AuthorizationOutcome.DENY, AuthorizationReason.NoMatchingPermission, "secret-policy");
        Assert.Same(expected, observer.Evaluate(request, () => expected));
        var audit = Assert.Single(sink.Events);
        var span = Assert.Single(spans.Where(span => span.GetTagItem("aegisai.correlation_id")?.ToString() == audit.CorrelationId));
        Assert.Equal(span.TraceId.ToHexString(), audit.TraceId);
        Assert.Equal(span.SpanId.ToHexString(), audit.SpanId);
        Assert.Equal("DENY", span.GetTagItem("aegisai.authorization.decision"));
        Assert.DoesNotContain("secret-", JsonSerializer.Serialize(audit));
        Assert.DoesNotContain(span.TagObjects, tag => tag.Value?.ToString()?.Contains("secret-") == true);
        Assert.Contains(measurements, m => m.Name == "aegisai.authorization.decisions");
        Assert.Contains(measurements, m => m.Name == "aegisai.authorization.duration");
        Assert.All(measurements, m => Assert.All(m.Tags, tag => Assert.Contains(tag, new[] { "model", "decision" })));
        var logger = new RecordingLogger();
        new LoggingAuthorizationAuditSink(logger).Write(audit);
        Assert.Contains(logger.Fields, p => p.Key == "CorrelationId" && Equals(p.Value, audit.CorrelationId));
        Assert.DoesNotContain("secret-", JsonSerializer.Serialize(logger.Fields));
    }

    [Fact]
    public void Evaluation_failure_is_preserved_without_fabricated_decision_or_exception_logging()
    {
        var sink = new RecordingAuditSink();
        var observer = new AuthorizationObservation(sink, new HttpContextAccessor());
        var failure = new InvalidOperationException("secret-credential-canary");
        var caught = Assert.Throws<InvalidOperationException>(() => observer.Evaluate(
            new(new("alice", "issuer"), new("reports"), new("read")), () => throw failure));
        Assert.Same(failure, caught);
        Assert.Empty(sink.Events);
    }

    private sealed class RecordingAuditSink : IAuthorizationAuditSink
    {
        public ConcurrentQueue<AuthorizationAuditEvent> Events { get; } = new();
        public bool Throw { get; init; }
        public void Write(AuthorizationAuditEvent e)
        {
            Events.Enqueue(e);
            if (Throw) throw new InvalidOperationException("secret-sink-error");
        }
    }
    private sealed class RecordingLogger : ILogger<LoggingAuthorizationAuditSink>
    {
        public List<KeyValuePair<string, object?>> Fields { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Assert.Null(exception);
            Fields.AddRange((IEnumerable<KeyValuePair<string, object?>>)(object)state!);
        }
    }
}
