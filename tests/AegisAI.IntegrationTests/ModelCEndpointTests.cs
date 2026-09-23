using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AegisAI.Domain.Authorization;
using Xunit;

namespace AegisAI.IntegrationTests;

public sealed partial class AuthorizationEndpointTests
{
    private static Dictionary<string, string?> ModelCConfiguration(string value)
    {
        var config = ModelBConfiguration();
        config["ContextualRisk:Version"] = "risk-fixture-v1";
        config["ContextualRisk:Contexts:0:Subject"] = "alice";
        config["ContextualRisk:Contexts:0:Issuer"] = Issuer;
        config["ContextualRisk:Contexts:0:Resource"] = "reports";
        config["ContextualRisk:Contexts:0:Action"] = "read";
        foreach (var signal in Enum.GetValues<RiskSignal>()) config[$"ContextualRisk:Contexts:0:Signals:{signal}"] = value;
        return config;
    }

    [Theory]
    [InlineData("0", "ALLOW", "Low")]
    [InlineData("0.25", "STEP_UP", "Medium")]
    [InlineData("0.5", "LIMIT", "High")]
    [InlineData("0.75", "DENY", "High")]
    public async Task Model_c_returns_explainable_risk_and_does_not_change_model_b(string score, string outcome, string level)
    {
        using var factory = CreateFactory(ModelCConfiguration(score));
        using var client = factory.CreateClient();
        using var authenticated = Client();
        client.DefaultRequestHeaders.Authorization = authenticated.DefaultRequestHeaders.Authorization;
        using var b = await client.PostAsJsonAsync("/authorization/evaluate/model-b", new { resource = "reports", action = "read" });
        Assert.Equal("ALLOW", (await b.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("outcome").GetString());
        using var c = await client.PostAsJsonAsync("/authorization/evaluate/model-c", new { resource = "reports", action = "read" });
        Assert.Equal(HttpStatusCode.OK, c.StatusCode);
        var body = await c.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(outcome, body.GetProperty("outcome").GetString());
        Assert.Equal("C", body.GetProperty("model").GetString());
        var risk = body.GetProperty("risk");
        Assert.Equal(decimal.Parse(score, System.Globalization.CultureInfo.InvariantCulture), risk.GetProperty("score").GetDecimal());
        Assert.Equal(level, risk.GetProperty("level").GetString());
        Assert.Equal(5, risk.GetProperty("contributions").GetArrayLength());
        Assert.Equal("risk-fixture-v1", risk.GetProperty("contextVersion").GetString());
        if (outcome == "LIMIT") Assert.Equal(10, body.GetProperty("obligation").GetProperty("maximumRequestsPerMinute").GetInt32());
    }

    [Theory]
    [InlineData("alice", "finance")]
    [InlineData("bob", "engineering")]
    public async Task Earlier_denials_skip_risk(string subject, string department)
    {
        var config = ModelCConfiguration("0"); config["Abac:Subjects:0:Attributes:department"] = department;
        using var factory = CreateFactory(config);
        using var client = factory.CreateClient();
        using var authenticated = Client(subject);
        client.DefaultRequestHeaders.Authorization = authenticated.DefaultRequestHeaders.Authorization;
        using var response = await client.PostAsJsonAsync("/authorization/evaluate/model-c", new { resource = "reports", action = "read" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("DENY", body.GetProperty("outcome").GetString());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("risk").ValueKind);
    }

    [Fact]
    public async Task Missing_context_denies_and_client_cannot_supply_it()
    {
        using var factory = CreateFactory(ModelBConfiguration());
        using var client = factory.CreateClient();
        using var authenticated = Client();
        client.DefaultRequestHeaders.Authorization = authenticated.DefaultRequestHeaders.Authorization;
        using var response = await client.PostAsJsonAsync("/authorization/evaluate/model-c", new { resource = "reports", action = "read" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("DENY", body.GetProperty("outcome").GetString());
        Assert.Equal("Unknown", body.GetProperty("risk").GetProperty("level").GetString());
        using var injected = await client.PostAsJsonAsync("/authorization/evaluate/model-c", new { resource = "reports", action = "read", riskScore = 0 });
        Assert.Equal(HttpStatusCode.BadRequest, injected.StatusCode);
        client.DefaultRequestHeaders.Authorization = null;
        using var anonymous = await client.PostAsJsonAsync("/authorization/evaluate/model-c", new { resource = "reports", action = "read" });
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
    }
    [Theory]
    [InlineData("ContextualRisk:Contexts:0:Signals:DeviceExposure", null)]
    [InlineData("ContextualRisk:Contexts:0:Issuer", "https://other.example.invalid")]
    [InlineData("ContextualRisk:Contexts:0:Action", "write")]
    public async Task Null_or_wrongly_bound_context_cannot_allow(string key, string? value)
    {
        var config = ModelCConfiguration("0"); config[key] = value;
        using var factory = CreateFactory(config);
        using var client = factory.CreateClient();
        using var authenticated = Client();
        client.DefaultRequestHeaders.Authorization = authenticated.DefaultRequestHeaders.Authorization;
        using var response = await client.PostAsJsonAsync("/authorization/evaluate/model-c", new { resource = "reports", action = "read" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("DENY", body.GetProperty("outcome").GetString());
        Assert.Equal("Unknown", body.GetProperty("risk").GetProperty("level").GetString());
    }

    [Theory]
    [InlineData("-0.01")]
    [InlineData("1.01")]
    [InlineData("not-a-number")]
    public void Invalid_configured_indicators_fail_startup(string value)
    {
        using var factory = CreateFactory(ModelCConfiguration(value));
        Assert.ThrowsAny<Exception>(() => factory.CreateClient());
    }

}
