namespace AegisAI.Domain.Authorization;

public sealed record Subject
{
    public string Id { get; }
    public string Issuer { get; }

    public Subject(string id, string issuer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        Id = id;
        Issuer = issuer;
    }
}
