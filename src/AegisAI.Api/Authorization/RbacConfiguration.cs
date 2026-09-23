using AegisAI.Application.Authorization;
using AegisAI.Domain.Authorization;
using AegisAI.Infrastructure.Authorization;
using ResourceAction = AegisAI.Domain.Authorization.Action;

namespace AegisAI.Api.Authorization;

public static class RbacConfiguration
{
    public static IServiceCollection AddRbacBaseline(this IServiceCollection services)
    {
        services.AddSingleton<IRbacPolicyProvider>(provider => new InMemoryRbacPolicyProvider(
            CreatePolicy(provider.GetRequiredService<IConfiguration>())));
        services.AddSingleton<IAuthorizationEngine, RbacAuthorizationEngine>();
        return services;
    }

    private static RbacPolicy CreatePolicy(IConfiguration configuration)
    {
        var section = configuration.GetSection("Rbac");
        var options = section.Get<RbacOptions>(binder => binder.ErrorOnUnknownConfiguration = true);
        // Absent configuration is an explicit empty policy, never a demo grant.
        return options is null
            ? new RbacPolicy("unconfigured", [], [])
            : new RbacPolicy(options.Version!,
                options.Roles.Select(role => new Role(role.Name!,
                    role.Permissions.Select(permission => new Permission(
                        new Resource(permission.Resource!), new ResourceAction(permission.Action!))))),
                options.Assignments.Select(assignment =>
                    new KeyValuePair<Subject, IEnumerable<string>>(
                        new Subject(assignment.Subject!, assignment.Issuer!), assignment.Roles)));
    }
}

public sealed class RbacOptions
{
    public string? Version { get; set; }
    public RoleOptions[] Roles { get; set; } = [];
    public AssignmentOptions[] Assignments { get; set; } = [];
}

public sealed class RoleOptions
{
    public string? Name { get; set; }
    public PermissionOptions[] Permissions { get; set; } = [];
}

public sealed class PermissionOptions
{
    public string? Resource { get; set; }
    public string? Action { get; set; }
}

public sealed class AssignmentOptions
{
    public string? Subject { get; set; }
    public string? Issuer { get; set; }
    public string[] Roles { get; set; } = [];
}
