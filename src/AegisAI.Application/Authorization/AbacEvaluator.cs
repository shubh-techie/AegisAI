using AegisAI.Domain.Authorization;

namespace AegisAI.Application.Authorization;

public sealed class AbacEvaluator : IAbacEvaluator
{
    public AbacDecision Evaluate(AuthorizationRequest request, AbacContext context)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);
        var rules = context.Rules.Where(rule => rule.Resource == request.Resource && rule.Action == request.Action).ToArray();
        if (rules.Length == 0) return new(AuthorizationOutcome.DENY, AbacReason.NoApplicableRules, context.Version);
        var missing = false;
        var mismatch = false;
        foreach (var condition in rules.SelectMany(rule => rule.Conditions))
        {
            var actual = condition.Scope switch
            {
                AttributeScope.Subject => context.Subject.Find(condition.Key),
                AttributeScope.Resource => context.Resource.Find(condition.Key),
                AttributeScope.Environment => context.Environment.Find(condition.Key),
                AttributeScope.Action => request.Action.Name,
                _ => null
            };
            missing |= actual is null;
            mismatch |= actual is not null && !string.Equals(actual, condition.Expected, StringComparison.Ordinal);
        }
        // Missing evidence takes precedence independent of configuration ordering.
        if (missing) return new(AuthorizationOutcome.DENY, AbacReason.MissingAttribute, context.Version);
        if (mismatch) return new(AuthorizationOutcome.DENY, AbacReason.ConditionNotSatisfied, context.Version);
        return new(AuthorizationOutcome.ALLOW, AbacReason.ConditionsSatisfied, context.Version);
    }
}
