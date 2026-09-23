namespace AegisAI.Domain.Authorization;

public sealed record Permission
{
    public Resource Resource { get; }
    public Action Action { get; }

    public Permission(Resource resource, Action action)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ArgumentNullException.ThrowIfNull(action);
        Resource = resource;
        Action = action;
    }
}
