namespace AegisAI.Application.Identity;

/// <summary>A subject is unique only within its issuing authority.</summary>
public sealed record AuthenticatedIdentity
{
    public string Subject { get; }
    public string Issuer { get; }

    public AuthenticatedIdentity(string subject, string issuer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        Subject = subject;
        Issuer = issuer;
    }
}
