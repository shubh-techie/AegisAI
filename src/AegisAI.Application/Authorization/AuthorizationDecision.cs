namespace AegisAI.Application.Authorization;

public enum AuthorizationOutcome { DENY, ALLOW, STEP_UP, LIMIT }
public enum AuthorizationReason { NoAssignedRoles, NoMatchingPermission, PermissionGranted }

public sealed record AuthorizationDecision(
    AuthorizationOutcome Outcome,
    AuthorizationReason Reason,
    string PolicyVersion);
