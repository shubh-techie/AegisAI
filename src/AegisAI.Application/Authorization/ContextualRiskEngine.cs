using AegisAI.Domain.Authorization;

namespace AegisAI.Application.Authorization;

public enum RiskLevel { Low, Medium, High, Unknown }
public sealed record RiskContribution(RiskSignal Signal, decimal? Value, decimal Weight, decimal Contribution, string Reason);
public sealed record RiskAssessment(decimal Score, RiskLevel Level, string AlgorithmVersion,
    string ContextVersion, IReadOnlyList<RiskContribution> Contributions);

public interface IContextualRiskEngine
{
    RiskAssessment Assess(RiskContext context);
}
public interface IRiskContextProvider
{
    RiskContext GetContext(AuthorizationRequest request);
}

public sealed class ContextualRiskEngine : IContextualRiskEngine
{
    public const string Version = "contextual-weighted-v1";
    private static readonly (RiskSignal Signal, decimal Weight)[] Weights =
    [
        (RiskSignal.AuthenticationWeakness, 0.25m),
        (RiskSignal.ResourceSensitivity, 0.25m),
        (RiskSignal.OperationSensitivity, 0.20m),
        (RiskSignal.NetworkExposure, 0.15m),
        (RiskSignal.DeviceExposure, 0.15m)
    ];

    public RiskAssessment Assess(RiskContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var contributions = Weights.Select(item =>
        {
            var value = context.Find(item.Signal);
            return new RiskContribution(item.Signal, value, item.Weight, (value ?? 1m) * item.Weight,
                value is null ? "MissingContextConservativeMaximum" : "ConfiguredIndicator");
        }).ToArray();
        var score = contributions.Sum(item => item.Contribution);
        var level = contributions.Any(item => item.Value is null) ? RiskLevel.Unknown :
            score < 0.25m ? RiskLevel.Low : score < 0.50m ? RiskLevel.Medium : RiskLevel.High;
        return new(score, level, Version, context.Version, Array.AsReadOnly(contributions));
    }
}
