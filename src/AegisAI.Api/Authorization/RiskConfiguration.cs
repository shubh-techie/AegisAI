using AegisAI.Application.Authorization;
using AegisAI.Domain.Authorization;
using AegisAI.Infrastructure.Authorization;

namespace AegisAI.Api.Authorization;

public static class RiskConfiguration
{
    public static IServiceCollection AddContextualRisk(this IServiceCollection services)
    {
        services.AddSingleton<IRiskContextProvider>(provider =>
        {
            var section = provider.GetRequiredService<IConfiguration>().GetSection("ContextualRisk");
            if (!section.Exists()) return new InMemoryRiskContextProvider("unconfigured", []);
            ValidateKeys(section, "Version", "Contexts");
            var version = Scalar(section.GetSection("Version"));
            var contexts = section.GetSection("Contexts");
            if (contexts.Value is not null) throw new ArgumentException("Contexts must be a collection.");
            return new InMemoryRiskContextProvider(version!, contexts.GetChildren().Select(context =>
            {
                ValidateKeys(context, "Subject", "Issuer", "Resource", "Action", "Signals");
                var request = new AuthorizationRequest(
                    new(Scalar(context.GetSection("Subject"))!, Scalar(context.GetSection("Issuer"))!),
                    new(Scalar(context.GetSection("Resource"))!), new(Scalar(context.GetSection("Action"))!));
                return new KeyValuePair<AuthorizationRequest, RiskContext>(request,
                    new(version!, ReadSignals(context.GetSection("Signals"))));
            }));
        });
        services.AddSingleton<IContextualRiskEngine, ContextualRiskEngine>();
        services.AddSingleton<IModelCAuthorizationEngine, ModelCAuthorizationEngine>();
        return services;
    }

    private static IEnumerable<KeyValuePair<RiskSignal, decimal>> ReadSignals(IConfigurationSection signals)
    {
        if (signals.Value is not null) throw new ArgumentException("Signals must be an object.");
        foreach (var entry in signals.GetChildren())
        {
            if (!Enum.TryParse<RiskSignal>(entry.Key, false, out var signal) ||
                !Enum.IsDefined(signal) || signal.ToString() != entry.Key)
                throw new ArgumentException("Undefined risk signal.");
            // Read raw scalar values: numeric binding can turn absent evidence into zero.
            var text = Scalar(entry);
            if (string.IsNullOrWhiteSpace(text)) continue;
            var value = decimal.Parse(text, System.Globalization.NumberStyles.AllowDecimalPoint |
                System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture);
            yield return new(signal, value);
        }
    }

    private static string? Scalar(IConfigurationSection section)
    {
        if (section.GetChildren().Any()) throw new ArgumentException("Expected a scalar configuration value.");
        return section.Value;
    }

    private static void ValidateKeys(IConfigurationSection section, params string[] allowed)
    {
        if (section.Value is not null || section.GetChildren().Any(child => !allowed.Contains(child.Key, StringComparer.OrdinalIgnoreCase)))
            throw new ArgumentException("Unexpected contextual risk configuration.");
    }
}
