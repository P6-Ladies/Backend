// File: testing/backend.IntegrationTests/Endpoints/ScenarioEndpointTests.cs

using System.Net;
using System.Net.Http.Json;
using backend.Entities.Scenarios;
using backend.Entities.Scenarios.DTOs;
using Microsoft.Extensions.DependencyInjection;
using backend.Data;
using backend.IntegrationTests.Utils;
using backend.IntegrationTests.Utils.DbSeeders;
using Xunit;

namespace backend.IntegrationTests.Endpoints;

public class ScenarioEndpointTests : IClassFixture<backendWebApplicationFactory>
{
    private readonly backendWebApplicationFactory _factory;

    public ScenarioEndpointTests(backendWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateScenario_ShouldReturnCreated_WhenValid()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var user);

        var request = new CreateScenarioDTO
        {
            Name = "Test Scenario",
            UserId = user.Id,
            SettingPrompt = "A dark forest",
            ConflictPrompt = "A stranger appears",
            AdditionalPrompt = "Thunder rumbles in the distance"
        };

        var response = await client.PostAsJsonAsync("/scenarios", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Scenario>();
        Assert.NotNull(created);
        Assert.Equal("Test Scenario", created!.Name);
    }

    [Fact]
    public async Task GetUserScenarios_ShouldReturnOk_WhenScenariosExist()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var user);

        var response = await client.GetAsync($"/users/{user.Id}/scenarios");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var scenarios = await response.Content.ReadFromJsonAsync<List<Scenario>>();
        Assert.NotNull(scenarios);
        Assert.NotEmpty(scenarios!);
    }

    [Fact]
    public async Task GetScenarioById_ShouldReturnOk_WhenScenarioExists()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out _);
        using var scope = _factory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();

        var scenario = db.Scenarios.First();

        var response = await client.GetAsync($"/scenarios/{scenario.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Scenario>();
        Assert.NotNull(result);
        Assert.Equal(scenario.Id, result!.Id);
    }

    [Fact]
    public async Task UpdateScenario_ShouldReturnOk_WhenValid()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out _);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
        var scenario = db.Scenarios.First();

        var update = new UpdateScenarioDTO
        {
            Name = "Updated Name"
        };

        var response = await client.PutAsJsonAsync($"/scenarios/{scenario.Id}", update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<Scenario>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated!.Name);
    }

    [Fact]
    public async Task DeleteScenario_ShouldReturnNoContent_WhenScenarioExists()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out _);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
        var scenario = db.Scenarios.First();

        var response = await client.DeleteAsync($"/scenarios/{scenario.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
