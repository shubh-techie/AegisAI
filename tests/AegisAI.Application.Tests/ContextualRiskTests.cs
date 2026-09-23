using AegisAI.Application.Authorization;
using AegisAI.Domain.Authorization;
using Xunit;

namespace AegisAI.Application.Tests;

public sealed class ContextualRiskTests
{
    private static readonly AuthorizationRequest Request = new(new("alice", "issuer"), new("reports"), new("read"));
    private static RiskContext Context(decimal value) => new("v1", Enum.GetValues<RiskSignal>().Select(signal => new KeyValuePair<RiskSignal, decimal>(signal, value)));
    private static ModelBDecision Granted => new(AuthorizationOutcome.ALLOW,
        new(AuthorizationOutcome.ALLOW, AuthorizationReason.PermissionGranted, "rbac-v1"),
        new(AuthorizationOutcome.ALLOW, AbacReason.ConditionsSatisfied, "abac-v1"));

    [Theory]
    [InlineData("0", RiskLevel.Low, AuthorizationOutcome.ALLOW)]
    [InlineData("0.2499", RiskLevel.Low, AuthorizationOutcome.ALLOW)]
    [InlineData("0.25", RiskLevel.Medium, AuthorizationOutcome.STEP_UP)]
    [InlineData("0.4999", RiskLevel.Medium, AuthorizationOutcome.STEP_UP)]
    [InlineData("0.50", RiskLevel.High, AuthorizationOutcome.LIMIT)]
    [InlineData("0.7499", RiskLevel.High, AuthorizationOutcome.LIMIT)]
    [InlineData("0.75", RiskLevel.High, AuthorizationOutcome.DENY)]
    [InlineData("1", RiskLevel.High, AuthorizationOutcome.DENY)]
    public void Exact_boundaries_control_levels_and_outcomes(string text, RiskLevel level, AuthorizationOutcome outcome)
    {
        var value = decimal.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
        var engine = new ModelCAuthorizationEngine(new Baseline(Granted), new Provider(Context(value)), new ContextualRiskEngine());
        var result = engine.Authorize(Request);
        Assert.Equal(outcome, result.Outcome);
        Assert.NotNull(result.Risk);
        Assert.Equal(value, result.Risk.Score);
        Assert.Equal(level, result.Risk.Level);
        Assert.Equal(result.Risk.Score, result.Risk.Contributions.Sum(item => item.Contribution));
        Assert.Equal(1m, result.Risk.Contributions.Sum(item => item.Weight));
        if (outcome == AuthorizationOutcome.LIMIT) Assert.Equal(10, result.Obligation?.MaximumRequestsPerMinute);
        else if (outcome == AuthorizationOutcome.STEP_UP) Assert.Equal("VerifyStrongerAuthenticationAndReevaluate", result.Obligation?.Code);
        else Assert.Null(result.Obligation);
    }

    [Fact]
    public void Weighted_signals_are_explained_and_deterministic()
    {
        var context = new RiskContext("fixture-v1", new Dictionary<RiskSignal, decimal>
        {
            [RiskSignal.AuthenticationWeakness] = 1m, [RiskSignal.ResourceSensitivity] = 0.5m,
            [RiskSignal.OperationSensitivity] = 0m, [RiskSignal.NetworkExposure] = 1m,
            [RiskSignal.DeviceExposure] = 0m
        });
        var engine = new ContextualRiskEngine();
        var result = engine.Assess(context);
        Assert.Equal(0.525m, result.Score);
        Assert.Equal("fixture-v1", result.ContextVersion);
        Assert.Equal(ContextualRiskEngine.Version, result.AlgorithmVersion);
        Assert.Equal(new[] { 0.25m, 0.125m, 0m, 0.15m, 0m }, result.Contributions.Select(item => item.Contribution));
        Assert.All(result.Contributions, item => Assert.Equal("ConfiguredIndicator", item.Reason));
        Assert.Equal(result.Contributions, engine.Assess(context).Contributions);
    }

    [Theory]
    [InlineData(RiskSignal.AuthenticationWeakness)]
    [InlineData(RiskSignal.ResourceSensitivity)]
    [InlineData(RiskSignal.OperationSensitivity)]
    [InlineData(RiskSignal.NetworkExposure)]
    [InlineData(RiskSignal.DeviceExposure)]
    public void Any_missing_signal_is_unknown_and_denied(RiskSignal missing)
    {
        var context = new RiskContext("v1", Enum.GetValues<RiskSignal>().Where(s => s != missing)
            .Select(s => new KeyValuePair<RiskSignal, decimal>(s, 0m)));
        var result = new ModelCAuthorizationEngine(new Baseline(Granted), new Provider(context), new ContextualRiskEngine()).Authorize(Request);
        Assert.Equal(AuthorizationOutcome.DENY, result.Outcome);
        Assert.Equal("MissingRiskContext", result.Reason);
        Assert.Equal(RiskLevel.Unknown, result.Risk!.Level);
        var contribution = Assert.Single(result.Risk.Contributions.Where(item => item.Value is null));
        Assert.Equal(contribution.Weight, result.Risk.Score);
        Assert.Equal("MissingContextConservativeMaximum", contribution.Reason);
    }

    [Fact]
    public void Entirely_missing_context_is_unknown_with_maximum_score()
    {
        var result = new ContextualRiskEngine().Assess(new("missing", []));
        Assert.Equal(1m, result.Score);
        Assert.Equal(RiskLevel.Unknown, result.Level);
        Assert.Equal(5, result.Contributions.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Earlier_denial_skips_risk_provider_and_cannot_be_overridden(bool abacDenied)
    {
        var denied = abacDenied ? Granted with { Outcome = AuthorizationOutcome.DENY,
            Abac = new(AuthorizationOutcome.DENY, AbacReason.ConditionNotSatisfied, "a") }
            : new ModelBDecision(AuthorizationOutcome.DENY, new(AuthorizationOutcome.DENY, AuthorizationReason.NoAssignedRoles, "r"), null);
        var result = new ModelCAuthorizationEngine(new Baseline(denied), new FailedProvider(), new ContextualRiskEngine()).Authorize(Request);
        Assert.Equal(AuthorizationOutcome.DENY, result.Outcome);
        Assert.Null(result.Risk);
        Assert.Null(result.Obligation);
    }

    [Fact]
    public void Provider_failure_is_an_error_not_an_allow()
    {
        Assert.Throws<InvalidOperationException>(() => new ModelCAuthorizationEngine(new Baseline(Granted),
            new FailedProvider(), new ContextualRiskEngine()).Authorize(Request));
    }
    private sealed class Baseline(ModelBDecision decision) : IModelBAuthorizationEngine
    {
        public ModelBDecision Authorize(AuthorizationRequest request) => decision;
    }
    private sealed class Provider(RiskContext context) : IRiskContextProvider
    {
        public RiskContext GetContext(AuthorizationRequest request) => context;
    }
    private sealed class FailedProvider : IRiskContextProvider
    {
        public RiskContext GetContext(AuthorizationRequest request) => throw new InvalidOperationException("Unavailable");
    }
}
