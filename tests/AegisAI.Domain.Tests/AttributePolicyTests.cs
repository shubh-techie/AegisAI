using AegisAI.Domain.Authorization;
using Xunit;

namespace AegisAI.Domain.Tests;

public sealed class AttributePolicyTests
{
    [Fact]
    public void Attribute_sets_are_immutable_and_case_sensitive()
    {
        var source = new Dictionary<string, string> { ["department"] = "engineering" };
        var set = new AttributeSet(source);
        source["department"] = "finance";
        Assert.Equal("engineering", set.Find("department"));
        Assert.Null(set.Find("Department"));
        Assert.Null(set.Find("missing"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Invalid_keys_values_and_conditions_are_rejected(string? value)
    {
        Assert.ThrowsAny<ArgumentException>(() => new AttributeSet([new(value!, "v")]));
        Assert.ThrowsAny<ArgumentException>(() => new AttributeSet([new("k", value!)]));
        Assert.ThrowsAny<ArgumentException>(() => new AttributeCondition(AttributeScope.Subject, value!, "v"));
        Assert.ThrowsAny<ArgumentException>(() => new AttributeCondition(AttributeScope.Subject, "k", value!));
    }

    [Fact]
    public void Malformed_rules_and_scopes_are_rejected()
    {
        Assert.Throws<ArgumentException>(() => new AttributeSet([new("key", "one"), new("key", "two")]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AttributeCondition((AttributeScope)99, "key", "v"));
        Assert.Throws<ArgumentException>(() => new AttributeCondition(AttributeScope.Action, "other", "v"));
        Assert.Throws<ArgumentException>(() => new AbacRule("r", new("reports"), new("read"), []));
        Assert.Throws<ArgumentException>(() => new AbacRule("r", new("reports"), new("read"), [null!]));
    }

    [Fact]
    public void Rule_copies_conditions()
    {
        var conditions = new List<AttributeCondition> { new(AttributeScope.Action, "name", "read") };
        var rule = new AbacRule("r", new("reports"), new("read"), conditions);
        conditions.Clear();
        Assert.Single(rule.Conditions);
        Assert.Throws<NotSupportedException>(() => ((IList<AttributeCondition>)rule.Conditions).Clear());
    }
}
