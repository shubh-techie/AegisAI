using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using AegisAI.Api.Authentication;
using AegisAI.Application.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace AegisAI.IntegrationTests;

public sealed class AuthenticationTests : IDisposable
{
    private const string Issuer = "https://issuer.example.invalid";
    private const string Audience = "aegisai-tests";
    private readonly RSA _rsa = RSA.Create(2048);
    private readonly WebApplicationFactory<Program> _factory;

    public AuthenticationTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:Authority"] = null,
                    ["Authentication:Audience"] = null
                }));
            builder.ConfigureServices(services => services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    // Keep real validation and events; replace only external discovery with test trust.
                    options.Authority = null;
                    options.ConfigurationManager = null;
                    options.TokenValidationParameters.ValidIssuer = Issuer;
                    options.TokenValidationParameters.ValidAudience = Audience;
                    options.TokenValidationParameters.IssuerSigningKey = new RsaSecurityKey(_rsa);
                }));
        });
    }

    private string Token(string scenario = "valid")
    {
        using var other = RSA.Create(2048);
        var now = DateTime.UtcNow;
        var claims = scenario == "missing-sub" ? Array.Empty<Claim>() :
            new[] { new Claim("sub", scenario == "blank-sub" ? " " : "alice") };
        var token = new JwtSecurityToken(
            scenario == "wrong-issuer" ? "https://other.example.invalid" : Issuer,
            scenario == "wrong-audience" ? "other-api" : Audience,
            claims,
            scenario == "future" ? now.AddMinutes(5) : now.AddMinutes(-10),
            scenario == "expired" ? now.AddMinutes(-5) : now.AddMinutes(10),
            scenario == "unsigned" ? null : new SigningCredentials(
                new RsaSecurityKey(scenario == "wrong-key" ? other : _rsa), SecurityAlgorithms.RsaSha256));
        if (scenario == "missing-exp") token.Payload.Remove("exp");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task Valid_token_maps_identity_and_does_not_leak_to_next_request()
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/identity");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token());
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var identity = await response.Content.ReadFromJsonAsync<AuthenticatedIdentity>();
        Assert.Equal(new AuthenticatedIdentity("alice", Issuer), identity);
        using var anonymous = await client.GetAsync("/identity");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("malformed")]
    [InlineData("wrong-key")]
    [InlineData("wrong-issuer")]
    [InlineData("wrong-audience")]
    [InlineData("expired")]
    [InlineData("missing-exp")]
    [InlineData("future")]
    [InlineData("unsigned")]
    [InlineData("missing-sub")]
    [InlineData("blank-sub")]
    public async Task Untrusted_credentials_are_challenged(string scenario)
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/identity");
        if (scenario != "missing") request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer", scenario == "malformed" ? "invalid-token" : Token(scenario));
        request.Headers.Add("X-User-Id", "spoofed-user");
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains(response.Headers.WwwAuthenticate, value => value.Scheme == "Bearer");
        Assert.DoesNotContain("invalid-token", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Unconfigured_host_rejects_even_a_correctly_signed_test_token()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Authentication:Authority"] = null,
                    ["Authentication:Audience"] = null
                })));
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token());
        using var response = await client.GetAsync("/identity");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        using var health = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
    }

    [Theory]
    [InlineData("http://issuer.example.invalid", "api")]
    [InlineData("https://issuer.example.invalid", null)]
    [InlineData(null, "api")]
    [InlineData("invalid", "api")]
    public void Partial_or_insecure_configuration_is_rejected(string? authority, string? audience)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Authentication:Authority"] = authority,
            ["Authentication:Audience"] = audience
        }).Build();
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddAegisAuthentication(config));
    }

    [Fact]
    public void Adapter_does_not_trust_unauthenticated_claims_or_missing_context()
    {
        var accessor = new HttpContextAccessor();
        var adapter = new HttpCurrentIdentity(accessor);
        Assert.Null(adapter.Identity);
        accessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("sub", "spoofed"), new Claim("iss", Issuer)
            }))
        };
        Assert.Null(adapter.Identity);
    }

    public void Dispose()
    {
        _factory.Dispose();
        _rsa.Dispose();
    }
}
