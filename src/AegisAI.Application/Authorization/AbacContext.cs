using AegisAI.Domain.Authorization;

namespace AegisAI.Application.Authorization;

/// <summary>Trusted immutable attributes and policy from a single versioned snapshot.</summary>
public sealed class AbacContext
{
    public string Version { get; }
    public IReadOnlyList<AbacRule> Rules { get; }
    public AttributeSet Subject { get; }
    public AttributeSet Resource { get; }
    public AttributeSet Environment { get; }

    public AbacContext(string version, IEnumerable<AbacRule> rules, AttributeSet subject,
        AttributeSet resource, AttributeSet environment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(subject);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(environment);
        var copy = rules.ToArray();
        if (copy.Any(rule => rule is null) || copy.Select(rule => rule.Id).Distinct(StringComparer.Ordinal).Count() != copy.Length)
            throw new ArgumentException("Rules require unique IDs and cannot contain null.", nameof(rules));
        Version = version;
        Rules = Array.AsReadOnly(copy);
        Subject = subject;
        Resource = resource;
        Environment = environment;
    }
}
