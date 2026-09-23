namespace AegisAI.Application.Identity;

public interface ICurrentIdentity
{
    // Null represents an unauthenticated request or absence of a request context.
    AuthenticatedIdentity? Identity { get; }
}
