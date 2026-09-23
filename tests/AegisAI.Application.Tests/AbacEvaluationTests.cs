using AegisAI.Application.Authorization;
using AegisAI.Domain.Authorization;
using Xunit;

namespace AegisAI.Application.Tests;

public sealed class AbacEvaluationTests
{
    private static readonly AuthorizationRequest Request = new(new("alice", "issuer"), new("reports"), new("read"));
    private static AttributeSet Attributes(string value) => new(new Dictionary<string, string> { ["department"] = value });
    private static AbacRule Rule(params AttributeCondition[] conditions) => new("rule", new("reports"), new("read"), conditions);
    private static AbacContext Context(AbacRule[] rules, AttributeSet? subject = null,
        AttributeSet? resource = null, AttributeSet? environment = null) =>
        new("abac-v1", rules, subject ?? AttributeSet.Empty, resource ?? AttributeSet.Empty, environment ?? AttributeSet.Empty);

    [Theory]
    [InlineData(AttributeScope.Subject)]
    [InlineData(AttributeScope.Resource)]
    [InlineData(AttributeScope.Environment)]
    public void Each_attribute_scope_is_independently_evaluated(AttributeScope scope)
    {
        var rule = Rule(new AttributeCondition(scope, "department", "engineering"));
        var context = Context([rule], scope == AttributeScope.Subject ? Attributes("engineering") : null,
            scope == AttributeScope.Resource ? Attributes("engineering") : null,
            scope == AttributeScope.Environment ? Attributes("engineering") : null);
        Assert.Equal(AuthorizationOutcome.ALLOW, new AbacEvaluator().Evaluate(Request, context).Outcome);
        Assert.Equal(AbacReason.MissingAttribute, new AbacEvaluator().Evaluate(Request, Context([rule])).Reason);
    }

    [Theory]
    [InlineData("engineering", AuthorizationOutcome.ALLOW)]
    [InlineData("Engineering", AuthorizationOutcome.DENY)]
    [InlineData("engineering ", AuthorizationOutcome.DENY)]
    [InlineData("finance", AuthorizationOutcome.DENY)]
    public void Conditions_use_exact_equality(string value, AuthorizationOutcome expected)
    {
        var result = new AbacEvaluator().Evaluate(Request,
            Context([Rule(new AttributeCondition(AttributeScope.Subject, "department", "engineering"))], Attributes(value)));
        Assert.Equal(expected, result.Outcome);
        Assert.Equal("abac-v1", result.SnapshotVersion);
    }

    [Fact]
    public void Requested_action_is_evaluated_without_an_attribute_override()
    {
        Assert.Equal(AuthorizationOutcome.ALLOW, new AbacEvaluator().Evaluate(Request,
            Context([Rule(new AttributeCondition(AttributeScope.Action, "name", "read"))])).Outcome);
        Assert.Equal(AbacReason.ConditionNotSatisfied, new AbacEvaluator().Evaluate(Request,
            Context([Rule(new AttributeCondition(AttributeScope.Action, "name", "write"))])).Reason);
    }

    [Fact]
    public void No_applicable_rule_denies_including_resource_and_action_mismatches()
    {
        foreach (var rules in new[] { Array.Empty<AbacRule>(),
            new[] { new AbacRule("r", new("other"), new("read"), [new(AttributeScope.Action, "name", "read")]) },
            new[] { new AbacRule("r", new("reports"), new("write"), [new(AttributeScope.Action, "name", "read")]) } })
            Assert.Equal(AbacReason.NoApplicableRules, new AbacEvaluator().Evaluate(Request, Context(rules)).Reason);
    }

    [Fact]
    public void All_applicable_rules_and_conditions_must_pass_with_order_independent_missing_reason()
    {
        var pass = Rule(new AttributeCondition(AttributeScope.Action, "name", "read"));
        var fail = new AbacRule("fail", new("reports"), new("read"), [new(AttributeScope.Action, "name", "write")]);
        var missing = new AbacRule("missing", new("reports"), new("read"), [new(AttributeScope.Subject, "department", "engineering")]);
        var evaluator = new AbacEvaluator();
        Assert.Equal(AbacReason.ConditionNotSatisfied, evaluator.Evaluate(Request, Context([pass, fail])).Reason);
        Assert.Equal(evaluator.Evaluate(Request, Context([fail, missing])), evaluator.Evaluate(Request, Context([missing, fail])));
        Assert.Equal(AbacReason.MissingAttribute, evaluator.Evaluate(Request, Context([pass, fail, missing])).Reason);
    }

    [Theory]
    [InlineData(true, "engineering", AuthorizationOutcome.ALLOW)]
    [InlineData(true, "finance", AuthorizationOutcome.DENY)]
    [InlineData(false, "engineering", AuthorizationOutcome.DENY)]
    public void Model_b_requires_both_rbac_and_abac(bool assigned, string department, AuthorizationOutcome expected)
    {
        var policy = new RbacPolicy("rbac-v1", [new("reader", [new(new("reports"), new("read"))])],
            assigned ? [new(Request.Subject, new[] { "reader" })] : []);
        var rbac = new RbacAuthorizationEngine(new RbacProvider(policy));
        var provider = new ContextProvider(Context([Rule(new AttributeCondition(AttributeScope.Subject, "department", "engineering"))], Attributes(department)));
        var result = new ModelBAuthorizationEngine(rbac, provider, new AbacEvaluator()).Authorize(Request);
        Assert.Equal(assigned ? AuthorizationOutcome.ALLOW : AuthorizationOutcome.DENY, rbac.Authorize(Request).Outcome);
        Assert.Equal(expected, result.Outcome);
        Assert.Equal(assigned ? 1 : 0, provider.Calls);
        if (!assigned) Assert.Null(result.Abac);
        else Assert.NotNull(result.Abac);
    }

    [Fact]
    public void Invalid_context_and_provider_errors_do_not_grant_access()
    {
        Assert.Throws<ArgumentException>(() => Context([Rule(new AttributeCondition(AttributeScope.Action, "name", "read")), Rule(new AttributeCondition(AttributeScope.Action, "name", "read"))]));
        Assert.Throws<ArgumentNullException>(() => new AbacEvaluator().Evaluate(Request, null!));
        var rbac = new RbacAuthorizationEngine(new RbacProvider(new("v1",
            [new("reader", [new(new("reports"), new("read"))])], [new(Request.Subject, new[] { "reader" })])));
        Assert.Throws<InvalidOperationException>(() => new ModelBAuthorizationEngine(rbac, new FailedProvider(), new AbacEvaluator()).Authorize(Request));
    }

    private sealed class RbacProvider(RbacPolicy policy) : IRbacPolicyProvider
    {
        public RbacPolicy GetPolicy() => policy;
    }
    private sealed class ContextProvider(AbacContext context) : IAbacContextProvider
    {
        public int Calls { get; private set; }
        public AbacContext GetContext(AuthorizationRequest request) { Calls++; return context; }
    }
    private sealed class FailedProvider : IAbacContextProvider
    {
        public AbacContext GetContext(AuthorizationRequest request) => throw new InvalidOperationException("Unavailable");
    }
}
