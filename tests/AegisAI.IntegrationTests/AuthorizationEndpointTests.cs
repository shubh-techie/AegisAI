using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AegisAI.Api.Authorization;
using AegisAI.Application.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace AegisAI.IntegrationTests;

public sealed partial class AuthorizationEndpointTests : IDisposable
{
    private const string Issuer = "https://rbac.example.invalid";
    private readonly RSA _key = RSA.Create(2048);
    private readonly WebApplicationFactory<Program> _factory;

    private static Dictionary<string, string?> Configuration() => new()
    {
        ["Authentication:Authority"] = null,
        ["Authentication:Audience"] = null,
        ["Rbac:Version"] = "test-policy-v1",
        ["Rbac:Roles:0:Name"] = "reader",
        ["Rbac:Roles:0:Permissions:0:Resource"] = "reports",
        ["Rbac:Roles:0:Permissions:0:Action"] = "read",
        ["Rbac:Assignments:0:Subject"] = "alice",
        ["Rbac:Assignments:0:Issuer"] = Issuer,
        ["Rbac:Assignments:0:Roles:0"] = "reader"
    };

    public AuthorizationEndpointTests()
    {
        _factory = CreateFactory(Configuration());
    }

    private WebApplicationFactory<Program> CreateFactory(Dictionary<string, string?> config) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(config));
            builder.ConfigureServices(services => services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = null;
                    options.ConfigurationManager = null;
                    options.TokenValidationParameters.ValidIssuers = [Issuer, "https://other.example.invalid"];
                    options.TokenValidationParameters.ValidAudience = "rbac-tests";
                    options.TokenValidationParameters.IssuerSigningKey = new RsaSecurityKey(_key);
                }));
        });

    private HttpClient Client(string? subject = "alice", string issuer = Issuer)
    {
        var client = _factory.CreateClient();
        if (subject is not null)
        {
            var token = new JwtSecurityToken(issuer, "rbac-tests",
                [new Claim("sub", subject), new Claim("role", "reader")],
                DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(5),
                new SigningCredentials(new RsaSecurityKey(_key), SecurityAlgorithms.RsaSha256));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                new JwtSecurityTokenHandler().WriteToken(token));
        }
        return client;
    }

    [Theory]
    [InlineData("alice", "reports", "read", "ALLOW", "PermissionGranted")]
    [InlineData("alice", "reports", "write", "DENY", "NoMatchingPermission")]
    [InlineData("alice", "unknown", "read", "DENY", "NoMatchingPermission")]
    [InlineData("alice", "Reports", "read", "DENY", "NoMatchingPermission")]
    [InlineData("bob", "reports", "read", "DENY", "NoAssignedRoles")]
    public async Task Endpoint_uses_server_policy_and_returns_decision(string subject, string resource,
        string action, string outcome, string reason)
    {
        using var client = Client(subject);
        using var response = await client.PostAsJsonAsync("/authorization/evaluate", new { resource, action });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(outcome, body.GetProperty("outcome").GetString());
        Assert.Equal(reason, body.GetProperty("reason").GetString());
        Assert.Equal("test-policy-v1", body.GetProperty("policyVersion").GetString());
        Assert.Equal("A", body.GetProperty("model").GetString());
        Assert.Equal(resource, body.GetProperty("resource").GetString());
        Assert.Equal(action, body.GetProperty("action").GetString());
    }

    [Fact]
    public async Task Same_subject_from_another_trusted_issuer_does_not_inherit_roles()
    {
        using var client = Client(issuer: "https://other.example.invalid");
        using var response = await client.PostAsJsonAsync("/authorization/evaluate", new { resource = "reports", action = "read" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("DENY", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("outcome").GetString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid-token")]
    public async Task Authentication_is_required(string? token)
    {
        using var client = Client(null);
        if (token is not null) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var response = await client.PostAsJsonAsync("/authorization/evaluate", new { resource = "reports", action = "read" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"resource\":null,\"action\":\"read\"}")]
    [InlineData("{\"resource\":\"reports\",\"action\":\" \"}")]
    [InlineData("{\"resource\":\"\",\"action\":\"read\"}")]
    [InlineData("{\"resource\":\"reports\",\"action\":42}")]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("{\"resource\":\"reports\",\"action\":\"read\",\"subject\":\"alice\"}")]
    [InlineData("{\"resource\":\"reports\",\"action\":\"read\",\"roles\":[\"reader\"]}")]
    public async Task Invalid_or_identity_injecting_body_is_rejected(string json)
    {
        using var client = Client("bob");
        using var response = await client.PostAsync("/authorization/evaluate", new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Overlong_identifiers_are_rejected()
    {
        using var client = Client();
        using var response = await client.PostAsJsonAsync("/authorization/evaluate", new { resource = new string('r', 257), action = "read" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Headers_cannot_override_authenticated_subject()
    {
        using var client = Client("bob");
        client.DefaultRequestHeaders.Add("X-User-Id", "alice");
        client.DefaultRequestHeaders.Add("X-Roles", "reader");
        using var response = await client.PostAsJsonAsync("/authorization/evaluate", new { resource = "reports", action = "read" });
        Assert.Equal("DENY", (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("outcome").GetString());
    }

    [Fact]
    public void Bad_policy_configuration_fails_startup_registration()
    {
        var values = Configuration();
        values["Rbac:Assignments:0:Roles:0"] = "undefined";
        var config = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        using var services = new ServiceCollection().AddSingleton<IConfiguration>(config)
            .AddRbacBaseline().BuildServiceProvider();
        Assert.Throws<ArgumentException>(() => services.GetRequiredService<IRbacPolicyProvider>());
    }

    [Fact]
    public async Task Missing_policy_has_no_implicit_grants()
    {
        using var factory = CreateFactory(new Dictionary<string, string?>());
        using var client = factory.CreateClient();
        using var authenticatedClient = Client();
        client.DefaultRequestHeaders.Authorization = authenticatedClient.DefaultRequestHeaders.Authorization;
        using var response = await client.PostAsJsonAsync("/authorization/evaluate", new { resource = "reports", action = "read" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("DENY", body.GetProperty("outcome").GetString());
        Assert.Equal("unconfigured", body.GetProperty("policyVersion").GetString());
    }

    public void Dispose()
    {
        _factory.Dispose();
        _key.Dispose();
    }
}
