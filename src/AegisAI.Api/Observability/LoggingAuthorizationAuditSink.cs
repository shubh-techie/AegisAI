using AegisAI.Application.Observability;

namespace AegisAI.Api.Observability;

public sealed class LoggingAuthorizationAuditSink(ILogger<LoggingAuthorizationAuditSink> logger) : IAuthorizationAuditSink
{
    public void Write(AuthorizationAuditEvent e) => logger.LogInformation(new EventId(8001, "AuthorizationDecision"),
        "AuthorizationDecision {EventId} {CorrelationId} {TraceId} {SpanId} {SubjectId} {ResourceId} {ActionId} {Model} {Decision} {RiskScore} {RiskLevel} {Reasons} {Timestamp} {ProcessingDurationMs}",
        e.EventId, e.CorrelationId, e.TraceId, e.SpanId, e.SubjectId, e.ResourceId, e.ActionId,
        e.Model, e.Decision, e.RiskScore, e.RiskLevel, string.Join(",", e.Reasons), e.Timestamp, e.ProcessingDurationMs);
}
