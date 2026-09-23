using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AegisAI.IntegrationTests;

public sealed class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_returns_success_and_healthy_json()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(body);
        Assert.Single(body);
        Assert.Equal("healthy", body["status"]);
    }

    [Fact]
    public async Task Unmapped_route_requires_authentication_by_default()
    {
        using var client = _factory.CreateClient();
        using var response = await client.GetAsync("/not-an-endpoint");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
