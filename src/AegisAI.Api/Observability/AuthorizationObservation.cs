using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;
using AegisAI.Application.Authorization;
using AegisAI.Application.Observability;

namespace AegisAI.Api.Observability;

/// <summary>One observation for the requested model, not each nested evaluation stage.</summary>
public sealed class AuthorizationObservation(IAuthorizationAuditSink sink, IHttpContextAccessor http)
{
    public const string InstrumentationName = "AegisAI.Authorization";
    private static readonly ActivitySource Source = new(InstrumentationName, "1.0.0");
    private static readonly Meter Meter = new(InstrumentationName, "1.0.0");
    private static readonly Counter<long> Decisions = Meter.CreateCounter<long>("aegisai.authorization.decisions", "{decision}");
    private static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("aegisai.authorization.duration", "ms");
    private static readonly Counter<long> Failures = Meter.CreateCounter<long>("aegisai.authorization.audit.failures", "{failure}");
    private static readonly Counter<long> Errors = Meter.CreateCounter<long>("aegisai.authorization.errors", "{error}");

    public AuthorizationDecision Evaluate(AuthorizationRequest request, Func<AuthorizationDecision> evaluate) =>
        Observe("A", request, evaluate, d => new(d.Outcome, null, null, [d.Reason.ToString()]));

    public ModelBDecision EvaluateB(AuthorizationRequest request, Func<ModelBDecision> evaluate) =>
        Observe("B", request, evaluate, d => new(d.Outcome, null, null, Reasons(d)));

    public ModelCDecision EvaluateC(AuthorizationRequest request, Func<ModelCDecision> evaluate) =>
        Observe("C", request, evaluate, d => new(d.Outcome, d.Risk?.Score, d.Risk?.Level.ToString(),
            Reasons(d.Baseline).Append(SafeReason(d.Reason)).ToArray()));

    private static string[] Reasons(ModelBDecision decision) => decision.Abac is { } abac
        ? [decision.Rbac.Reason.ToString(), abac.Reason.ToString()] : [decision.Rbac.Reason.ToString()];

    private static string SafeReason(string reason) => reason is "BaselineDenied" or "MissingRiskContext" or
        "LowContextualRisk" or "AdditionalAssuranceRequired" or "RateRestrictionRequired" or "HighContextualRisk"
            ? reason : "Other";

    private T Observe<T>(string model, AuthorizationRequest request, Func<T> evaluate, Func<T, Summary> summarize)
    {
        var correlation = Guid.NewGuid().ToString("N");
        Activity? activity = null;
        BestEffort(() =>
        {
            if (http.HttpContext is { } context) context.Response.Headers["X-Correlation-ID"] = correlation;
            activity = Source.StartActivity("authorization.evaluate", ActivityKind.Internal);
            activity?.SetTag("aegisai.authorization.model", model);
            activity?.SetTag("aegisai.correlation_id", correlation);
        });
        var started = Stopwatch.GetTimestamp();
        try
        {
            T result;
            try { result = evaluate(); }
            catch
            {
                BestEffort(() => Errors.Add(1, new KeyValuePair<string, object?>("model", model)));
                BestEffort(() => activity?.SetStatus(ActivityStatusCode.Error));
                throw;
            }
            var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            BestEffort(() =>
            {
                var summary = summarize(result);
                var decision = summary.Outcome.ToString();
                var tags = new TagList { { "model", model }, { "decision", decision } };
                BestEffort(() => Decisions.Add(1, tags));
                BestEffort(() => Duration.Record(elapsed, tags));
                BestEffort(() =>
                {
                    activity?.SetTag("aegisai.authorization.decision", decision);
                    activity?.SetTag("aegisai.risk.level", summary.RiskLevel);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                });
                var trace = activity ?? Activity.Current;
                var audit = new AuthorizationAuditEvent(Guid.NewGuid().ToString("N"), correlation,
                    trace?.TraceId.ToHexString(), trace?.SpanId.ToHexString(),
                    Pseudonym(request.Subject.Issuer, request.Subject.Id), Pseudonym(request.Resource.Id),
                    Pseudonym(request.Action.Name), model, decision, summary.Score, summary.RiskLevel,
                    Array.AsReadOnly(summary.Reasons), DateTimeOffset.UtcNow, elapsed);
                try { sink.Write(audit); }
                catch
                {
                    // Do not log exception messages: sink exceptions can contain sensitive data.
                    BestEffort(() => Failures.Add(1, new KeyValuePair<string, object?>("model", model)));
                }
            });
            return result;
        }
        finally { BestEffort(() => activity?.Dispose()); }
    }

    public static string Pseudonym(params string[] parts)
    {
        // Length prefixes disambiguate issuer/subject pairs without logging either value.
        var canonical = string.Concat(parts.Select(part => $"{part.Length}:{part}"));
        return "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }

    private static void BestEffort(Action operation)
    {
        try { operation(); }
        catch { /* Observers must not change an authorization result or exception. */ }
    }
    private sealed record Summary(AuthorizationOutcome Outcome, decimal? Score, string? RiskLevel, string[] Reasons);
}
