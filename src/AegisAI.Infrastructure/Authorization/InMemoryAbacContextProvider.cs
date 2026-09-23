using AegisAI.Application.Authorization;
using AegisAI.Domain.Authorization;

namespace AegisAI.Infrastructure.Authorization;

public sealed class InMemoryAbacContextProvider : IAbacContextProvider
{
    private readonly AbacContext _template;
    private readonly Dictionary<Subject, AttributeSet> _subjects;
    private readonly Dictionary<Resource, AttributeSet> _resources;

    public InMemoryAbacContextProvider(string version, IEnumerable<AbacRule> rules,
        IEnumerable<KeyValuePair<Subject, AttributeSet>> subjects,
        IEnumerable<KeyValuePair<Resource, AttributeSet>> resources, AttributeSet environment)
    {
        _template = new(version, rules, AttributeSet.Empty, AttributeSet.Empty, environment);
        _subjects = subjects.ToDictionary(pair => pair.Key, pair => pair.Value);
        _resources = resources.ToDictionary(pair => pair.Key, pair => pair.Value);
        if (_subjects.Values.Any(value => value is null) || _resources.Values.Any(value => value is null))
            throw new ArgumentException("Attribute sets cannot be null.");
    }

    public AbacContext GetContext(AuthorizationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new(_template.Version, _template.Rules,
            _subjects.GetValueOrDefault(request.Subject, AttributeSet.Empty),
            _resources.GetValueOrDefault(request.Resource, AttributeSet.Empty), _template.Environment);
    }
}
