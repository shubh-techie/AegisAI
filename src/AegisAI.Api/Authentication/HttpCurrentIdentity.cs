using AegisAI.Application.Identity;

namespace AegisAI.Api.Authentication;

public sealed class HttpCurrentIdentity(IHttpContextAccessor accessor) : ICurrentIdentity
{
    public AuthenticatedIdentity? Identity
    {
        get
        {
            var identities = accessor.HttpContext?.User.Identities
                .Where(identity => identity.IsAuthenticated).ToArray();
            if (identities is not { Length: 1 }) return null;
            var identity = identities[0];
            var subjects = identity.FindAll("sub").ToArray();
            var issuers = identity.FindAll("iss").ToArray();
            if (subjects.Length != 1 || issuers.Length != 1 ||
                string.IsNullOrWhiteSpace(subjects[0].Value) ||
                string.IsNullOrWhiteSpace(issuers[0].Value)) return null;
            return new AuthenticatedIdentity(subjects[0].Value, issuers[0].Value);
        }
    }
}
