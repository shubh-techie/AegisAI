using AegisAI.Application.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace AegisAI.Api.Authentication;

public static class AuthenticationSetup
{
    public static IServiceCollection AddAegisAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration["Authentication:Authority"];
        var audience = configuration["Authentication:Audience"];
        var configured = !string.IsNullOrWhiteSpace(authority) || !string.IsNullOrWhiteSpace(audience);
        if (configured && (!Uri.TryCreate(authority, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) ||
            string.IsNullOrWhiteSpace(audience)))
            throw new InvalidOperationException("Authentication requires an HTTPS Authority and a nonempty Audience.");

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentIdentity, HttpCurrentIdentity>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.Authority = configured ? authority : null;
            options.Audience = audience;
            options.RequireHttpsMetadata = true;
            options.MapInboundClaims = false;
            options.SaveToken = false;
            options.IncludeErrorDetails = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var subjects = context.Principal?.FindAll("sub").ToArray() ?? [];
                    var issuers = context.Principal?.FindAll("iss").ToArray() ?? [];
                    if (subjects.Length != 1 || issuers.Length != 1 ||
                        string.IsNullOrWhiteSpace(subjects[0].Value) ||
                        string.IsNullOrWhiteSpace(issuers[0].Value))
                        context.Fail("A unique subject and issuer are required.");
                    return Task.CompletedTask;
                }
            };
        });
        services.AddAuthorization(options =>
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser().Build());
        return services;
    }
}
