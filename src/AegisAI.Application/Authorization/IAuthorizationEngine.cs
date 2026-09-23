namespace AegisAI.Application.Authorization;

public interface IAuthorizationEngine
{
    AuthorizationDecision Authorize(AuthorizationRequest request);
}
