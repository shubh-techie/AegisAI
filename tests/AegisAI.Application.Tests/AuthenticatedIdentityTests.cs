using AegisAI.Application.Identity;
using Xunit;

namespace AegisAI.Application.Tests;

public sealed class AuthenticatedIdentityTests
{
    [Theory]
    [InlineData(null, "issuer")]
    [InlineData("", "issuer")]
    [InlineData(" ", "issuer")]
    [InlineData("subject", null)]
    [InlineData("subject", "")]
    [InlineData("subject", " ")]
    public void Missing_identity_components_are_rejected(string? subject, string? issuer)
    {
        Assert.ThrowsAny<ArgumentException>(() => new AuthenticatedIdentity(subject!, issuer!));
    }

    [Fact]
    public void Same_subject_from_different_issuers_is_a_different_identity()
    {
        Assert.NotEqual(new AuthenticatedIdentity("subject", "issuer-a"),
            new AuthenticatedIdentity("subject", "issuer-b"));
    }
}
