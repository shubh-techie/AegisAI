using AegisAI.Application.Authorization;
using AegisAI.Domain.Authorization;
using Xunit;

namespace AegisAI.Application.Tests;

public sealed class RbacAuthorizationTests
{
    private static readonly Subject Alice = new("alice", "issuer");
    private static readonly Role Reader = new("reader", [new(new("reports"), new("read"))]);
    private static readonly Role Writer = new("writer", [new(new("reports"), new("write"))]);
    private static readonly Role Empty = new("empty", []);

    private static RbacPolicy Policy(params string[] roles) => new("v1", [Reader, Writer, Empty],
        [new(Alice, roles)]);
    private static AuthorizationDecision Evaluate(RbacPolicy policy, string resource = "reports",
        string action = "read", Subject? subject = null) =>
        new RbacAuthorizationEngine(new Provider(policy)).Authorize(
            new(subject ?? Alice, new(resource), new(action)));

    [Theory]
    [InlineData("reports", "read", AuthorizationOutcome.ALLOW)]
    [InlineData("reports", "write", AuthorizationOutcome.DENY)]
    [InlineData("other", "read", AuthorizationOutcome.DENY)]
    [InlineData("Reports", "read", AuthorizationOutcome.DENY)]
    [InlineData("reports", "Read", AuthorizationOutcome.DENY)]
    [InlineData("reports ", "read", AuthorizationOutcome.DENY)]
    [InlineData("*", "read", AuthorizationOutcome.DENY)]
    public void Permission_requires_exact_resource_and_action(string resource, string action, AuthorizationOutcome outcome)
    {
        var result = Evaluate(Policy("reader"), resource, action);
        Assert.Equal(outcome, result.Outcome);
        Assert.Equal(outcome == AuthorizationOutcome.ALLOW ? AuthorizationReason.PermissionGranted :
            AuthorizationReason.NoMatchingPermission, result.Reason);
        Assert.Equal("v1", result.PolicyVersion);
    }

    [Fact]
    public void Multiple_roles_union_permissions_and_order_does_not_matter()
    {
        Assert.Equal(AuthorizationOutcome.ALLOW, Evaluate(Policy("reader", "writer"), action: "write").Outcome);
        Assert.Equal(Evaluate(Policy("reader", "writer")), Evaluate(Policy("writer", "reader", "reader")));
    }

    [Fact]
    public void No_roles_unknown_subject_and_other_issuer_deny()
    {
        Assert.Equal(AuthorizationReason.NoAssignedRoles, Evaluate(Policy()).Reason);
        Assert.Equal(AuthorizationReason.NoAssignedRoles, Evaluate(Policy("reader"), subject: new("bob", "issuer")).Reason);
        Assert.Equal(AuthorizationReason.NoAssignedRoles, Evaluate(Policy("reader"), subject: new("alice", "other")).Reason);
        Assert.Equal(AuthorizationOutcome.DENY, Evaluate(new("empty", [], [])).Outcome);
        Assert.Equal(AuthorizationReason.NoMatchingPermission, Evaluate(Policy("empty")).Reason);
    }

    [Fact]
    public void Wildcards_have_no_special_meaning()
    {
        var policy = new RbacPolicy("v1", [new("star", [new(new("*"), new("*"))])], [new(Alice, new[] { "star" })]);
        Assert.Equal(AuthorizationOutcome.DENY, Evaluate(policy).Outcome);
        Assert.Equal(AuthorizationOutcome.ALLOW, Evaluate(policy, "*", "*").Outcome);
    }

    [Fact]
    public void Permissions_are_not_combined_across_pairs()
    {
        var policy = new RbacPolicy("v1", [new("mixed", [new(new("reports"), new("read")),
            new(new("accounts"), new("write"))])], [new(Alice, new[] { "mixed" })]);
        Assert.Equal(AuthorizationOutcome.DENY, Evaluate(policy, "reports", "write").Outcome);
    }

    [Fact]
    public void Invalid_policy_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => new RbacPolicy("v1", [Reader, Reader], []));
        Assert.Throws<ArgumentException>(() => Policy("undefined"));
        Assert.Throws<ArgumentException>(() => Policy("Reader"));
        Assert.Throws<ArgumentException>(() => new RbacPolicy("v1", [Reader],
            [new(Alice, new[] { "reader" }), new(Alice, Array.Empty<string>())]));
        Assert.ThrowsAny<ArgumentException>(() => new RbacPolicy(" ", [], []));
        Assert.Throws<ArgumentNullException>(() => new RbacPolicy("v1", null!, []));
        Assert.Throws<ArgumentNullException>(() => new RbacPolicy("v1", [], null!));
        Assert.Throws<ArgumentNullException>(() => new RbacPolicy("v1", [null!], []));
        Assert.Throws<ArgumentNullException>(() => new RbacPolicy("v1", [], [new(null!, [])]));
        Assert.Throws<ArgumentNullException>(() => new RbacPolicy("v1", [], [new(Alice, null!)]));
        Assert.ThrowsAny<ArgumentException>(() => Policy(" "));
    }

    [Fact]
    public void Snapshot_is_immutable_and_repeated_evaluation_is_deterministic()
    {
        var names = new List<string> { "reader" };
        var roles = new List<Role> { Reader };
        var assignments = new List<KeyValuePair<Subject, IEnumerable<string>>> { new(Alice, names) };
        var policy = new RbacPolicy("v1", roles, assignments);
        names.Clear(); roles.Clear(); assignments.Clear();
        Assert.Throws<NotSupportedException>(() => ((IList<Role>)policy.RolesFor(Alice)).Clear());
        var first = Evaluate(policy);
        Assert.Equal(AuthorizationOutcome.ALLOW, first.Outcome);
        for (var i = 0; i < 10; i++) Assert.Equal(first, Evaluate(policy));
    }

    [Fact]
    public void Invalid_requests_are_rejected_and_provider_failure_never_returns_allow()
    {
        Assert.Throws<ArgumentNullException>(() => new AuthorizationRequest(null!, new("reports"), new("read")));
        Assert.Throws<ArgumentNullException>(() => new AuthorizationRequest(Alice, null!, new("read")));
        Assert.Throws<ArgumentNullException>(() => new AuthorizationRequest(Alice, new("reports"), null!));
        Assert.Throws<ArgumentNullException>(() => new RbacAuthorizationEngine(new Provider(Policy())).Authorize(null!));
        Assert.Throws<InvalidOperationException>(() => new RbacAuthorizationEngine(new FailedProvider())
            .Authorize(new(Alice, new("reports"), new("read"))));
    }

    private sealed class Provider(RbacPolicy policy) : IRbacPolicyProvider
    {
        public RbacPolicy GetPolicy() => policy;
    }
    private sealed class FailedProvider : IRbacPolicyProvider
    {
        public RbacPolicy GetPolicy() => throw new InvalidOperationException("Policy unavailable");
    }
}
