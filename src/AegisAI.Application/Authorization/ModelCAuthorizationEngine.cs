namespace AegisAI.Application.Authorization;

public interface IModelCAuthorizationEngine
{
    ModelCDecision Authorize(AuthorizationRequest request);
}
public sealed record RiskObligation(string Code, int? MaximumRequestsPerMinute = null);
public sealed record ModelCDecision(AuthorizationOutcome Outcome, string Reason, ModelBDecision Baseline,
    RiskAssessment? Risk, RiskObligation? Obligation);

public sealed class ModelCAuthorizationEngine(IModelBAuthorizationEngine baseline,
    IRiskContextProvider contexts, IContextualRiskEngine risk) : IModelCAuthorizationEngine
{
    public ModelCDecision Authorize(AuthorizationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var prior = baseline.Authorize(request);
        if (prior.Outcome != AuthorizationOutcome.ALLOW)
            return new(AuthorizationOutcome.DENY, "BaselineDenied", prior, null, null);
        var assessment = risk.Assess(contexts.GetContext(request));
        if (assessment.Level == RiskLevel.Unknown)
            return new(AuthorizationOutcome.DENY, "MissingRiskContext", prior, assessment, null);
        if (assessment.Score < 0.25m)
            return new(AuthorizationOutcome.ALLOW, "LowContextualRisk", prior, assessment, null);
        if (assessment.Score < 0.50m)
            return new(AuthorizationOutcome.STEP_UP, "AdditionalAssuranceRequired", prior, assessment,
                new("VerifyStrongerAuthenticationAndReevaluate"));
        if (assessment.Score < 0.75m)
            return new(AuthorizationOutcome.LIMIT, "RateRestrictionRequired", prior, assessment,
                new("EnforcePerSubjectResourceActionRateLimit", 10));
        return new(AuthorizationOutcome.DENY, "HighContextualRisk", prior, assessment, null);
    }
}
