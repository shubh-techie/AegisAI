using AegisAI.Api.Authentication;
using AegisAI.Api.Authorization;
using AegisAI.Application.Identity;
using AegisAI.Application.Authorization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAegisAuthentication(builder.Configuration);
builder.Services.AddRbacBaseline();
builder.Services.AddAbacBaseline();
builder.Services.AddContextualRisk();
var app = builder.Build();
// Validate and freeze policy after all host configuration providers have been applied.
_ = app.Services.GetRequiredService<IRbacPolicyProvider>();
_ = app.Services.GetRequiredService<IAbacContextProvider>();
_ = app.Services.GetRequiredService<IRiskContextProvider>();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();
app.MapGet("/identity", (ICurrentIdentity current) =>
    current.Identity is { } identity ? Results.Ok(identity) : Results.Unauthorized())
    .RequireAuthorization();

app.MapRbacBaseline();

app.Run();

// Expose the entry point to the integration test host.
public partial class Program { }
