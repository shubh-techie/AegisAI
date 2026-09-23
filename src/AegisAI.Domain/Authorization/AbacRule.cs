namespace AegisAI.Domain.Authorization;

public sealed class AbacRule
{
    public string Id { get; }
    public Resource Resource { get; }
    public Action Action { get; }
    public IReadOnlyList<AttributeCondition> Conditions { get; }

    public AbacRule(string id, Resource resource, Action action, IEnumerable<AttributeCondition> conditions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(conditions);
        var copy = conditions.ToArray();
        if (copy.Length == 0 || copy.Any(condition => condition is null))
            throw new ArgumentException("A rule requires nonnull conditions.", nameof(conditions));
        Id = id;
        Resource = resource;
        Action = action;
        Conditions = Array.AsReadOnly(copy);
    }
}
