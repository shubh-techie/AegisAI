namespace AegisAI.Domain.Authorization;

public sealed class Role
{
    public string Name { get; }
    public IReadOnlyList<Permission> Permissions { get; }

    public Role(string name, IEnumerable<Permission> permissions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(permissions);
        var copy = permissions.ToArray();
        if (copy.Any(permission => permission is null))
            throw new ArgumentException("Permissions cannot contain null.", nameof(permissions));
        Name = name;
        Permissions = Array.AsReadOnly(copy.Distinct().ToArray());
    }
}
