namespace AegisAI.Application.Authorization;

public interface IRbacPolicyProvider
{
    RbacPolicy GetPolicy();
}
