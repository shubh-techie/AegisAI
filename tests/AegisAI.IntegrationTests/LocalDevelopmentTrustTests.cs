using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using AegisAI.Api.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace AegisAI.IntegrationTests;

public sealed class LocalDevelopmentTrustTests
{
    [Theory]
    [InlineData("Production", null, null)]
    [InlineData("Staging", null, null)]
    [InlineData("Development", "https://provider.example.invalid", "api")]
    [InlineData("Development", null, "api")]
    public void Local_trust_rejects_other_environments_and_provider_configuration(string environment, string? authority, string? audience)
    {
        using var rsa = RSA.Create(2048);
        var config = Configuration(rsa.ExportSubjectPublicKeyInfoPem(), authority, audience);
        Assert.Throws<InvalidOperationException>(() => LocalDevelopmentTrust.Configure(new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions(), config, environment));
    }

    [Fact]
    public void Private_key_configuration_is_rejected()
    {
        using var rsa = RSA.Create(2048);
        Assert.Throws<InvalidOperationException>(() => LocalDevelopmentTrust.Configure(new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions(),
            Configuration(rsa.ExportPkcs8PrivateKeyPem()), "Development"));
    }

    [Theory]
    [InlineData("valid", HttpStatusCode.OK)]
    [InlineData("missing", HttpStatusCode.Unauthorized)]
    [InlineData("wrong-key", HttpStatusCode.Unauthorized)]
    [InlineData("wrong-issuer", HttpStatusCode.Unauthorized)]
    [InlineData("wrong-audience", HttpStatusCode.Unauthorized)]
    [InlineData("expired", HttpStatusCode.Unauthorized)]
    [InlineData("wrong-algorithm", HttpStatusCode.Unauthorized)]
    public async Task Local_public_key_keeps_real_bearer_validation(string scenario, HttpStatusCode expected)
    {
        using var rsa = RSA.Create(2048);
        using var other = RSA.Create(2048);
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) => config.AddConfiguration(Configuration(rsa.ExportSubjectPublicKeyInfoPem())));
        });
        using var client = factory.CreateClient();
        if (scenario != "missing")
        {
            var now = DateTime.UtcNow;
            var token = new JwtSecurityToken(
                scenario == "wrong-issuer" ? "https://other.invalid" : LocalDevelopmentTrust.Issuer,
                scenario == "wrong-audience" ? "other" : LocalDevelopmentTrust.Audience,
                [new Claim("sub", "synthetic-developer")], now.AddMinutes(-10),
                scenario == "expired" ? now.AddMinutes(-5) : now.AddMinutes(5),
                new SigningCredentials(new RsaSecurityKey(scenario == "wrong-key" ? other : rsa),
                    scenario == "wrong-algorithm" ? SecurityAlgorithms.RsaSha512 : SecurityAlgorithms.RsaSha256));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        }
        using var response = await client.GetAsync("/identity");
        Assert.Equal(expected, response.StatusCode);
    }

    private static IConfiguration Configuration(string publicKey, string? authority = null, string? audience = null) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["LocalDevelopment:PublicKeyPem"] = publicKey,
            ["Authentication:Authority"] = authority,
            ["Authentication:Audience"] = audience
        }).Build();
}
