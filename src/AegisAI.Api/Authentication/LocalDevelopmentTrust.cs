using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AegisAI.Api.Authentication;

/// <summary>Explicit local-only public-key trust; normal JWT validation still applies.</summary>
public static class LocalDevelopmentTrust
{
    public const string Issuer = "https://aegisai-local.example.invalid";
    public const string Audience = "aegisai-local-development";

    public static void Configure(JwtBearerOptions options, IConfiguration configuration, string environmentName)
    {
        var pem = configuration["LocalDevelopment:PublicKeyPem"];
        if (pem is null) return;
        if (environmentName != "Development" ||
            !string.IsNullOrWhiteSpace(configuration["Authentication:Authority"]) ||
            !string.IsNullOrWhiteSpace(configuration["Authentication:Audience"]))
            throw new InvalidOperationException("Local trust requires Development and cannot coexist with provider trust.");
        if (!pem.StartsWith("-----BEGIN PUBLIC KEY-----", StringComparison.Ordinal) || pem.Contains("PRIVATE"))
            throw new InvalidOperationException("Local trust accepts only an RSA public key.");
        using var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        if (rsa.KeySize < 2048) throw new InvalidOperationException("Local RSA keys must be at least 2048 bits.");
        var key = new RsaSecurityKey(rsa.ExportParameters(false));
        options.Authority = null;
        options.ConfigurationManager = null;
        options.TokenValidationParameters.ValidIssuer = Issuer;
        options.TokenValidationParameters.ValidAudience = Audience;
        options.TokenValidationParameters.IssuerSigningKey = key;
        options.TokenValidationParameters.ValidAlgorithms = [SecurityAlgorithms.RsaSha256];
    }
}
