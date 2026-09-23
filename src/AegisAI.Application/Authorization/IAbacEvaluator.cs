namespace AegisAI.Application.Authorization;

public interface IAbacEvaluator
{
    AbacDecision Evaluate(AuthorizationRequest request, AbacContext context);
}

public enum AbacReason { NoApplicableRules, MissingAttribute, ConditionNotSatisfied, ConditionsSatisfied }
public sealed record AbacDecision(AuthorizationOutcome Outcome, AbacReason Reason, string SnapshotVersion);

public interface IAbacContextProvider
{
    AbacContext GetContext(AuthorizationRequest request);
}
