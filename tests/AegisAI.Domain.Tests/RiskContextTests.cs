using AegisAI.Domain.Authorization;
using Xunit;

namespace AegisAI.Domain.Tests;

public sealed class RiskContextTests
{
    [Fact]
    public void Invalid_context_is_rejected()
    {
        Assert.ThrowsAny<ArgumentException>(() => new RiskContext(" ", []));
        Assert.Throws<ArgumentNullException>(() => new RiskContext("v", null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RiskContext("v", [new(RiskSignal.NetworkExposure, -0.01m)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RiskContext("v", [new(RiskSignal.NetworkExposure, 1.01m)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RiskContext("v", [new((RiskSignal)99, 0m)]));
        Assert.Throws<ArgumentException>(() => new RiskContext("v", [new(RiskSignal.NetworkExposure, 0m), new(RiskSignal.NetworkExposure, 1m)]));
    }
    [Fact]
    public void Context_copies_inputs_and_preserves_unknown_values()
    {
        var values = new Dictionary<RiskSignal, decimal> { [RiskSignal.NetworkExposure] = 0m };
        var context = new RiskContext("v", values);
        values[RiskSignal.NetworkExposure] = 1m;
        Assert.Equal(0m, context.Find(RiskSignal.NetworkExposure));
        Assert.Null(context.Find(RiskSignal.DeviceExposure));
    }
}
