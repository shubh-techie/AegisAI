using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AegisAI.Api.Authorization;
using AegisAI.Application.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AegisAI.IntegrationTests;

public sealed partial class AuthorizationEndpointTests
{
    private static Dictionary<string, string?> ModelBConfiguration(string department = "engineering")
    {
        var config = Configuration();
        config["Abac:Version"] = "attributes-policy-v1";
        config["Abac:Rules:0:Id"] = "reports-rule";
        config["Abac:Rules:0:Resource"] = "reports";
        config["Abac:Rules:0:Action"] = "read";
        var scopes = new[] { "Subject", "Resource", "Environment", "Action" };
        var keys = new[] { "department", "classification", "network", "name" };
        var values = new[] { "engineering", "internal", "trusted", "read" };
        for (var i = 0; i < scopes.Length; i++)
        {
            config[$"Abac:Rules:0:Conditions:{i}:Scope"] = scopes[i];
            config[$"Abac:Rules:0:Conditions:{i}:Key"] = keys[i];
            config[$"Abac:Rules:0:Conditions:{i}:Expected"] = values[i];
        }
        config["Abac:Subjects:0:Id"] = "alice";
        config["Abac:Subjects:0:Issuer"] = Issuer;
        config["Abac:Subjects:0:Attributes:department"] = department;
        config["Abac:Resources:0:Id"] = "reports";
        config["Abac:Resources:0:Attributes:classification"] = "internal";
        config["Abac:Environment:network"] = "trusted";
        return config;
    }

    [Theory]
    [InlineData("engineering", "alice", "ALLOW", "ALLOW")]
    [InlineData("finance", "alice", "ALLOW", "DENY")]
    [InlineData("engineering", "bob", "DENY", "DENY")]
    public async Task Models_a_and_b_produce_policy_driven_comparisons(string department, string subject,
        string outcomeA, string outcomeB)
    {
        using var factory = CreateFactory(ModelBConfiguration(department));
        using var client = factory.CreateClient();
        using var authenticated = Client(subject);
        client.DefaultRequestHeaders.Authorization = authenticated.DefaultRequestHeaders.Authorization;
        using var a = await client.PostAsJsonAsync("/authorization/evaluate", new { resource = "reports", action = "read" });
        using var b = await client.PostAsJsonAsync("/authorization/evaluate/model-b", new { resource = "reports", action = "read" });
        Assert.Equal(HttpStatusCode.OK, a.StatusCode);
        Assert.Equal(HttpStatusCode.OK, b.StatusCode);
        var bodyA = await a.Content.ReadFromJsonAsync<JsonElement>();
        var bodyB = await b.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(outcomeA, bodyA.GetProperty("outcome").GetString());
        Assert.Equal(outcomeB, bodyB.GetProperty("outcome").GetString());
        Assert.Equal("B", bodyB.GetProperty("model").GetString());
        Assert.Equal(outcomeA, bodyB.GetProperty("rbac").GetProperty("outcome").GetString());
        if (outcomeA == "DENY") Assert.Equal(JsonValueKind.Null, bodyB.GetProperty("abac").ValueKind);
        else Assert.Equal("attributes-policy-v1", bodyB.GetProperty("abac").GetProperty("snapshotVersion").GetString());
    }

    [Theory]
    [InlineData("Abac:Environment:network", "untrusted", "ConditionNotSatisfied")]
    [InlineData("Abac:Resources:0:Attributes:classification", "public", "ConditionNotSatisfied")]
    [InlineData("Abac:Subjects:0:Issuer", "https://other.example.invalid", "MissingAttribute")]
    public async Task Trusted_attribute_changes_restrict_model_b(string key, string value, string reason)
    {
        var config = ModelBConfiguration(); config[key] = value;
        using var factory = CreateFactory(config);
        using var client = factory.CreateClient();
        using var authenticated = Client();
        client.DefaultRequestHeaders.Authorization = authenticated.DefaultRequestHeaders.Authorization;
        client.DefaultRequestHeaders.Add("X-Department", "engineering");
        client.DefaultRequestHeaders.Add("X-Network", "trusted");
        using var response = await client.PostAsJsonAsync("/authorization/evaluate/model-b", new { resource = "reports", action = "read" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("DENY", body.GetProperty("outcome").GetString());
        Assert.Equal(reason, body.GetProperty("abac").GetProperty("reason").GetString());
    }

    [Fact]
    public async Task Missing_abac_configuration_denies_rbac_granted_request()
    {
        using var client = Client();
        using var response = await client.PostAsJsonAsync("/authorization/evaluate/model-b", new { resource = "reports", action = "read" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("DENY", body.GetProperty("outcome").GetString());
        Assert.Equal("NoApplicableRules", body.GetProperty("abac").GetProperty("reason").GetString());
    }

    [Theory]
    [InlineData("{\"resource\":\"reports\",\"action\":\"read\",\"subjectAttributes\":{\"department\":\"engineering\"}}")]
    [InlineData("{}")]
    [InlineData("{")]
    public async Task Model_b_rejects_invalid_or_caller_supplied_attributes(string json)
    {
        using var client = Client();
        using var response = await client.PostAsync("/authorization/evaluate/model-b", new StringContent(json, Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Model_b_requires_authentication()
    {
        using var client = Client(null);
        using var response = await client.PostAsJsonAsync("/authorization/evaluate/model-b", new { resource = "reports", action = "read" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("Abac:Rules:0:Conditions:0:Scope", "Unsupported")]
    [InlineData("Abac:Rules:0:Conditions:0:Scope", "99")]
    [InlineData("Abac:Version", " ")]
    [InlineData("Abac:Unexpected", "value")]
    public void Invalid_abac_configuration_is_rejected(string key, string value)
    {
        var config = ModelBConfiguration(); config[key] = value;
        using var services = new ServiceCollection().AddSingleton<IConfiguration>(
            new ConfigurationBuilder().AddInMemoryCollection(config).Build()).AddAbacBaseline().BuildServiceProvider();
        Assert.ThrowsAny<Exception>(() => services.GetRequiredService<IAbacContextProvider>());
    }
}
