namespace AegisAI.Application.Authorization;

public enum AuthorizationOutcome { DENY, ALLOW }
public enum AuthorizationReason { NoAssignedRoles, NoMatchingPermission, PermissionGranted }

public sealed record AuthorizationDecision(
    AuthorizationOutcome Outcome,
    AuthorizationReason Reason,
    string PolicyVersion);
