using AegisAI.Domain.Authorization;

namespace AegisAI.Application.Authorization;

/// <summary>An immutable, validated snapshot for one reproducible policy version.</summary>
public sealed class RbacPolicy
{
    private readonly Dictionary<Subject, IReadOnlyList<Role>> _assignments = new();
    public string Version { get; }

    public RbacPolicy(string version, IEnumerable<Role> roles,
        IEnumerable<KeyValuePair<Subject, IEnumerable<string>>> assignments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentNullException.ThrowIfNull(roles);
        ArgumentNullException.ThrowIfNull(assignments);
        Version = version;
        var roleMap = new Dictionary<string, Role>(StringComparer.Ordinal);
        foreach (var role in roles)
        {
            ArgumentNullException.ThrowIfNull(role);
            if (!roleMap.TryAdd(role.Name, role))
                throw new ArgumentException("Duplicate role name.", nameof(roles));
        }
        foreach (var (subject, names) in assignments)
        {
            ArgumentNullException.ThrowIfNull(subject);
            ArgumentNullException.ThrowIfNull(names);
            var resolved = new List<Role>();
            foreach (var name in names)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(name);
                if (!roleMap.TryGetValue(name, out var role))
                    throw new ArgumentException("Assignment references an undefined role.", nameof(assignments));
                resolved.Add(role);
            }
            if (!_assignments.TryAdd(subject, Array.AsReadOnly(resolved.Distinct().ToArray())))
                throw new ArgumentException("Duplicate subject assignment.", nameof(assignments));
        }
    }

    public IReadOnlyList<Role> RolesFor(Subject subject)
    {
        ArgumentNullException.ThrowIfNull(subject);
        return _assignments.TryGetValue(subject, out var roles) ? roles : Array.Empty<Role>();
    }
}
