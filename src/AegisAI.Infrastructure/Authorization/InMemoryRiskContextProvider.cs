using AegisAI.Application.Authorization;
using AegisAI.Domain.Authorization;

namespace AegisAI.Infrastructure.Authorization;

public sealed class InMemoryRiskContextProvider : IRiskContextProvider
{
    private readonly Dictionary<AuthorizationRequest, RiskContext> _contexts;
    private readonly RiskContext _missing;

    public InMemoryRiskContextProvider(string version, IEnumerable<KeyValuePair<AuthorizationRequest, RiskContext>> contexts)
    {
        _missing = new(version, []);
        ArgumentNullException.ThrowIfNull(contexts);
        _contexts = contexts.ToDictionary(pair => pair.Key, pair => pair.Value);
        if (_contexts.Values.Any(value => value is null || value.Version != version))
            throw new ArgumentException("Contexts must be nonnull and share the snapshot version.", nameof(contexts));
    }

    public RiskContext GetContext(AuthorizationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return _contexts.GetValueOrDefault(request, _missing);
    }
}
