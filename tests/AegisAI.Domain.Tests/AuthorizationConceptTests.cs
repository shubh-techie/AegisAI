using AegisAI.Domain.Authorization;
using ResourceAction = AegisAI.Domain.Authorization.Action;
using Xunit;

namespace AegisAI.Domain.Tests;

public sealed class AuthorizationConceptTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t")]
    public void Identifiers_must_be_nonblank(string? value)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Subject(value!, "issuer"));
        Assert.ThrowsAny<ArgumentException>(() => new Subject("id", value!));
        Assert.ThrowsAny<ArgumentException>(() => new Resource(value!));
        Assert.ThrowsAny<ArgumentException>(() => new ResourceAction(value!));
        Assert.ThrowsAny<ArgumentException>(() => new Role(value!, []));
    }

    [Fact]
    public void Permission_requires_resource_and_action()
    {
        Assert.Throws<ArgumentNullException>(() => new Permission(null!, new("read")));
        Assert.Throws<ArgumentNullException>(() => new Permission(new("reports"), null!));
    }

    [Fact]
    public void Identity_and_permission_equality_are_exact()
    {
        Assert.Equal(new Subject("alice", "issuer"), new Subject("alice", "issuer"));
        Assert.NotEqual(new Subject("alice", "issuer-a"), new Subject("alice", "issuer-b"));
        Assert.NotEqual(new Subject("alice", "issuer"), new Subject("Alice", "issuer"));
        Assert.Equal(new Permission(new("reports"), new("read")), new Permission(new("reports"), new("read")));
        Assert.NotEqual(new Resource("reports"), new Resource("Reports"));
        Assert.NotEqual(new ResourceAction("read"), new ResourceAction("read "));
    }

    [Fact]
    public void Role_copies_permissions_and_deduplicates_them()
    {
        var permission = new Permission(new("reports"), new("read"));
        var source = new List<Permission> { permission, permission };
        var role = new Role("reader", source);
        source.Clear();
        Assert.Equal(permission, Assert.Single(role.Permissions));
        Assert.Throws<NotSupportedException>(() => ((IList<Permission>)role.Permissions).Clear());
        Assert.Throws<ArgumentNullException>(() => new Role("reader", null!));
        Assert.Throws<ArgumentException>(() => new Role("reader", [null!]));
    }
}
