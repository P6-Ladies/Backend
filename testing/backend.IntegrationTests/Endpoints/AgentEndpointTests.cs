using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Backend.Entities.Agents;
using Backend.Entities.Agents.DTOs;
using Backend.IntegrationTests.Utils;
using Backend.IntegrationTests.Utils.DbSeeders;
using Backend.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;


namespace Backend.IntegrationTests.Endpoints;

public class AgentEndpointTests : IClassFixture<backendWebApplicationFactory>
{
    private readonly backendWebApplicationFactory _factory;

    public AgentEndpointTests(backendWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateAgent_ShouldReturnCreated_WhenAgentIsValid()
    {
        // Arrange: Create a client with a seeded database and a test user
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var testUser);

        // Create a new agent DTO
        var newAgent = new CreateAgentDTO
        {
            Name = "Test Agent",
            UserId = testUser.Id,
            PromptBody = "This is a test agent.",
            AvatarId = 1,
            Openness = 5,
            Conscientiousness = 5,
            Extroversion = 5,
            Agreeableness = 5,
            Neuroticism = 5
        };

        // Act: Send a POST request to create the agent
        var response = await client.PostAsJsonAsync("/agents", newAgent);

        // Assert: Check if the response is Created and the agent data matches
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdAgent = await response.Content.ReadFromJsonAsync<Agent>();
        Assert.NotNull(createdAgent);
        Assert.Equal(newAgent.Name, createdAgent.Name);
        Assert.Equal(newAgent.UserId, createdAgent.UserId);

        // Verify Database State
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
        Assert.True(dbContext.Agents.Any(a => a.Name == newAgent.Name && a.UserId == newAgent.UserId));
    }

    [Fact]
    public async Task CreateAgent_ShouldReturnBadRequest_WhenNameIsMissing()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var user);

        var agent = new CreateAgentDTO
        {
            Name = null!,
            UserId = user.Id
        };

        var response = await client.PostAsJsonAsync("/agents", agent);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUserAgents_ShouldReturnAgents_WhenTheyExist()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var user);

        var response = await client.GetAsync($"/users/{user.Id}/agents");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var agents = await response.Content.ReadFromJsonAsync<List<Agent>>();
        Assert.NotNull(agents);
        Assert.NotEmpty(agents);
    }


    [Fact]
    public async Task GetAgent_ShouldReturnOk_WhenAgentExists()
    {
        // Arrange: Create a client with a seeded database and a test agent
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var testUser);
        
        // Act: Send a GET request to retrieve the agent
        var response = await client.GetAsync($"/agents/{testUser.Id}");
        
        // Assert: Check if the response is OK and the agent data matches
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var agent = await response.Content.ReadFromJsonAsync<AgentDTO>();
        Assert.NotNull(agent);
        Assert.Equal(testUser.Id, agent.UserId);

        // Verify Database State
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
        Assert.True(dbContext.Agents.Any(a => a.UserId == testUser.Id));
    }


    [Fact]
    public async Task GetAgent_ShouldReturnNotFound_WhenAgentDoesNotExist()
    {
        // Arrange: Create a client with an empty database
        var client = _factory.CreateClientWithSeed(new EmptyDb(), out _);

        // Act: Send a GET request to retrieve a non-existing agent
        var response = await client.GetAsync("/agents/1");

        // Assert: Check if the response is NotFound
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // Verify Database State (should be empty)
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
        Assert.False(dbContext.Agents.Any(a => a.UserId == 1));
    }

    [Fact]
    public async Task GetAgentById_ShouldReturnAgent_WhenExists()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var user);

        var agentId = new BaseCaseDb().Agents.First().Id;
        var response = await client.GetAsync($"/agents/{agentId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var agent = await response.Content.ReadFromJsonAsync<Agent>();
        Assert.NotNull(agent);
        Assert.Equal(agentId, agent.Id);
    }

    [Fact]
    public async Task GetAgentById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out _);

        var response = await client.GetAsync($"/agents/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact]
    public async Task UpdateAgent_ShouldReturnOk_WhenAgentExists()
    {
        var dbSeeder = new BaseCaseDb();
        var client = _factory.CreateClientWithSeed(dbSeeder, out var user);

        var agent = dbSeeder.Agents.First();

        var update = new UpdateAgentDTO
        {
            Name = "Updated Name"
        };

        var response = await client.PutAsJsonAsync($"/agents/{agent.Id}", update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<Agent>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.Name);

        // Verify update in DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
        var agentInDb = await db.Agents.FindAsync(agent.Id);
        Assert.NotNull(agentInDb);
        Assert.Equal("Updated Name", agentInDb.Name);
    }

    [Fact]
    public async Task UpdateAgent_ShouldReturnNotFound_WhenAgentDoesNotExist()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out _);
        var update = new UpdateAgentDTO { Name = "New Name" };

        var response = await client.PutAsJsonAsync("/agents/99999", update);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAgent_ShouldReturnNoContent_WhenAgentExists()
    {
        var dbSeeder = new BaseCaseDb();
        var client = _factory.CreateClientWithSeed(dbSeeder, out _);

        var agentId = dbSeeder.Agents.First().Id;

        var response = await client.DeleteAsync($"/agents/{agentId}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deletion from DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
        Assert.False(db.Agents.Any(a => a.Id == agentId));
    }


    [Fact]
    public async Task DeleteAgent_ShouldReturnNotFound_WhenAgentDoesNotExist()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out _);

        var response = await client.DeleteAsync("/agents/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}