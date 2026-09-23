namespace AegisAI.Domain.Authorization;

public enum AttributeScope { Subject, Resource, Environment, Action }

/// <summary>Exact ordinal equality against a configured literal; no expression execution.</summary>
public sealed record AttributeCondition
{
    public AttributeScope Scope { get; }
    public string Key { get; }
    public string Expected { get; }

    public AttributeCondition(AttributeScope scope, string key, string expected)
    {
        if (!Enum.IsDefined(scope)) throw new ArgumentOutOfRangeException(nameof(scope));
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(expected);
        if (scope == AttributeScope.Action && key != "name")
            throw new ArgumentException("Action supports only the name attribute.", nameof(key));
        Scope = scope;
        Key = key;
        Expected = expected;
    }
}
