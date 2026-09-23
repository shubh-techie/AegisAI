namespace AegisAI.Domain.Authorization;

public sealed record Resource
{
    public string Id { get; }

    public Resource(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Id = value;
    }
}
