using System.Text.Json.Serialization;
using AegisAI.Application.Authorization;
using AegisAI.Application.Identity;
using AegisAI.Domain.Authorization;
using ResourceAction = AegisAI.Domain.Authorization.Action;

namespace AegisAI.Api.Authorization;

public static class AuthorizationEndpoints
{
    public static IEndpointRouteBuilder MapRbacBaseline(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/authorization/evaluate", Evaluate).RequireAuthorization();
        endpoints.MapPost("/authorization/evaluate/model-b", EvaluateModelB).RequireAuthorization();
        endpoints.MapPost("/authorization/evaluate/model-c", EvaluateModelC).RequireAuthorization();
        return endpoints;
    }

    private static IResult EvaluateModelC(EvaluationInput input, ICurrentIdentity current, IModelCAuthorizationEngine engine)
    {
        if (current.Identity is not { } identity) return Results.Unauthorized();
        if (string.IsNullOrWhiteSpace(input.Resource) || input.Resource.Length > 256 ||
            string.IsNullOrWhiteSpace(input.Action) || input.Action.Length > 256)
            return Results.BadRequest(new { error = "Resource and action must be nonblank and at most 256 characters." });
        var request = new AuthorizationRequest(new Subject(identity.Subject, identity.Issuer),
            new Resource(input.Resource), new ResourceAction(input.Action));
        var decision = engine.Authorize(request);
        return Results.Ok(new
        {
            model = "C", outcome = decision.Outcome.ToString(), decision.Reason,
            resource = request.Resource.Id, action = request.Action.Name,
            rbac = new { outcome = decision.Baseline.Rbac.Outcome.ToString(),
                reason = decision.Baseline.Rbac.Reason.ToString(), decision.Baseline.Rbac.PolicyVersion },
            abac = decision.Baseline.Abac is { } abac ? new { outcome = abac.Outcome.ToString(),
                reason = abac.Reason.ToString(), abac.SnapshotVersion } : null,
            risk = decision.Risk is { } risk ? new { risk.Score, level = risk.Level.ToString(),
                risk.AlgorithmVersion, risk.ContextVersion,
                contributions = risk.Contributions.Select(item => new { signal = item.Signal.ToString(),
                    item.Value, item.Weight, item.Contribution, item.Reason }) } : null,
            decision.Obligation
        });
    }

    private static IResult EvaluateModelB(EvaluationInput input, ICurrentIdentity current, IModelBAuthorizationEngine engine)
    {
        if (current.Identity is not { } identity) return Results.Unauthorized();
        if (string.IsNullOrWhiteSpace(input.Resource) || input.Resource.Length > 256 ||
            string.IsNullOrWhiteSpace(input.Action) || input.Action.Length > 256)
            return Results.BadRequest(new { error = "Resource and action must be nonblank and at most 256 characters." });
        var request = new AuthorizationRequest(new Subject(identity.Subject, identity.Issuer),
            new Resource(input.Resource), new ResourceAction(input.Action));
        var decision = engine.Authorize(request);
        return Results.Ok(new
        {
            model = "B",
            outcome = decision.Outcome.ToString(),
            resource = request.Resource.Id,
            action = request.Action.Name,
            rbac = new { outcome = decision.Rbac.Outcome.ToString(), reason = decision.Rbac.Reason.ToString(),
                policyVersion = decision.Rbac.PolicyVersion },
            abac = decision.Abac is { } result ? new { outcome = result.Outcome.ToString(),
                reason = result.Reason.ToString(), snapshotVersion = result.SnapshotVersion } : null
        });
    }

    private static IResult Evaluate(EvaluationInput input, ICurrentIdentity current, IAuthorizationEngine engine)
    {
        if (current.Identity is not { } identity) return Results.Unauthorized();
        if (string.IsNullOrWhiteSpace(input.Resource) || input.Resource.Length > 256 ||
            string.IsNullOrWhiteSpace(input.Action) || input.Action.Length > 256)
            return Results.BadRequest(new { error = "Resource and action must be nonblank and at most 256 characters." });

        var request = new AuthorizationRequest(new Subject(identity.Subject, identity.Issuer),
            new Resource(input.Resource), new ResourceAction(input.Action));
        var decision = engine.Authorize(request);
        return Results.Ok(new
        {
            model = "A",
            outcome = decision.Outcome.ToString(),
            reason = decision.Reason.ToString(),
            policyVersion = decision.PolicyVersion,
            resource = request.Resource.Id,
            action = request.Action.Name
        });
    }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record EvaluationInput(string? Resource, string? Action);
