using AegisAI.Api.Authentication;
using AegisAI.Application.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAegisAuthentication(builder.Configuration);
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous();
app.MapGet("/identity", (ICurrentIdentity current) =>
    current.Identity is { } identity ? Results.Ok(identity) : Results.Unauthorized())
    .RequireAuthorization();

app.Run();

// Expose the entry point to the integration test host.
public partial class Program { }
