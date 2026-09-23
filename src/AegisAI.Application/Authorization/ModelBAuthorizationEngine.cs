namespace AegisAI.Application.Authorization;

public interface IModelBAuthorizationEngine
{
    ModelBDecision Authorize(AuthorizationRequest request);
}

public sealed record ModelBDecision(AuthorizationOutcome Outcome, AuthorizationDecision Rbac, AbacDecision? Abac);

public sealed class ModelBAuthorizationEngine(
    IAuthorizationEngine rbac, IAbacContextProvider contexts, IAbacEvaluator abac) : IModelBAuthorizationEngine
{
    public ModelBDecision Authorize(AuthorizationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var baseline = rbac.Authorize(request);
        if (baseline.Outcome != AuthorizationOutcome.ALLOW)
            return new(AuthorizationOutcome.DENY, baseline, null);
        var attributes = abac.Evaluate(request, contexts.GetContext(request));
        return new(attributes.Outcome, baseline, attributes);
    }
}
