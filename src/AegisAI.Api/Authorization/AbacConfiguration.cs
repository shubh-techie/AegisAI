using AegisAI.Application.Authorization;
using AegisAI.Domain.Authorization;
using AegisAI.Infrastructure.Authorization;
using ResourceAction = AegisAI.Domain.Authorization.Action;

namespace AegisAI.Api.Authorization;

public static class AbacConfiguration
{
    public static IServiceCollection AddAbacBaseline(this IServiceCollection services)
    {
        services.AddSingleton<IAbacContextProvider>(provider =>
        {
            var options = provider.GetRequiredService<IConfiguration>().GetSection("Abac")
                .Get<AbacOptions>(binder => binder.ErrorOnUnknownConfiguration = true);
            if (options is null) return new InMemoryAbacContextProvider("unconfigured", [], [], [], AttributeSet.Empty);
            return new InMemoryAbacContextProvider(options.Version!,
                options.Rules.Select(rule => new AbacRule(rule.Id!, new Resource(rule.Resource!), new ResourceAction(rule.Action!),
                    rule.Conditions.Select(condition => new AttributeCondition(
                        Enum.Parse<AttributeScope>(condition.Scope!, ignoreCase: false), condition.Key!, condition.Expected!)))),
                options.Subjects.Select(subject => new KeyValuePair<Subject, AttributeSet>(
                    new(subject.Id!, subject.Issuer!), new(subject.Attributes))),
                options.Resources.Select(resource => new KeyValuePair<Resource, AttributeSet>(
                    new(resource.Id!), new(resource.Attributes))), new AttributeSet(options.Environment));
        });
        services.AddSingleton<IAbacEvaluator, AbacEvaluator>();
        services.AddSingleton<IModelBAuthorizationEngine, ModelBAuthorizationEngine>();
        return services;
    }
}

public sealed class AbacOptions
{
    public string? Version { get; set; }
    public AbacRuleOptions[] Rules { get; set; } = [];
    public SubjectAttributeOptions[] Subjects { get; set; } = [];
    public ResourceAttributeOptions[] Resources { get; set; } = [];
    public Dictionary<string, string> Environment { get; set; } = new();
}
public sealed class AbacRuleOptions
{
    public string? Id { get; set; }
    public string? Resource { get; set; }
    public string? Action { get; set; }
    public ConditionOptions[] Conditions { get; set; } = [];
}
public sealed class ConditionOptions
{
    public string? Scope { get; set; }
    public string? Key { get; set; }
    public string? Expected { get; set; }
}
public sealed class SubjectAttributeOptions
{
    public string? Id { get; set; }
    public string? Issuer { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
}
public sealed class ResourceAttributeOptions
{
    public string? Id { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
}
