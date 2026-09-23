using AegisAI.Domain.Authorization;

namespace AegisAI.Application.Authorization;

public sealed class RbacAuthorizationEngine(IRbacPolicyProvider policies) : IAuthorizationEngine
{
    public AuthorizationDecision Authorize(AuthorizationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var policy = policies.GetPolicy();
        var roles = policy.RolesFor(request.Subject);
        if (roles.Count == 0)
            return new(AuthorizationOutcome.DENY, AuthorizationReason.NoAssignedRoles, policy.Version);

        var permission = new Permission(request.Resource, request.Action);
        var granted = roles.Any(role => role.Permissions.Contains(permission));
        return new(granted ? AuthorizationOutcome.ALLOW : AuthorizationOutcome.DENY,
            granted ? AuthorizationReason.PermissionGranted : AuthorizationReason.NoMatchingPermission,
            policy.Version);
    }
}
