namespace AegisAI.Application.Observability;

/// <summary>Allowlisted decision metadata; identifiers are SHA-256 pseudonyms.</summary>
public sealed record AuthorizationAuditEvent(
    string EventId, string CorrelationId, string? TraceId, string? SpanId,
    string SubjectId, string ResourceId, string ActionId,
    string Model, string Decision, decimal? RiskScore, string? RiskLevel,
    IReadOnlyList<string> Reasons, DateTimeOffset Timestamp, double ProcessingDurationMs);

public interface IAuthorizationAuditSink
{
    void Write(AuthorizationAuditEvent auditEvent);
}
