using System.Net;
using System.Net.Http.Json;
using backend.Entities.Conversations;
using backend.Entities.Conversations.DTOs;
using backend.IntegrationTests.Utils;
using backend.IntegrationTests.Utils.DbSeeders;
using backend.Entities.Users;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace backend.IntegrationTests.Endpoints;

public class ConversationEndpointTests : IClassFixture<backendWebApplicationFactory>
{
    private readonly backendWebApplicationFactory _factory;

    public ConversationEndpointTests(backendWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateConversation_ShouldReturnCreated_WhenDataIsValid()
    {
        var dbSeeder = new BaseCaseDb();
        var client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        _factory.SeedDb(scope, dbSeeder);
        var user = dbSeeder.Users["testuser"];
        var agent = dbSeeder.Agents.First();
        var scenario = dbSeeder.Scenarios.First();

        var request = new CreateConversationDTO
        {
            Title = "New Conversation",
            UserId = user.Id,
            AgentId = agent.Id,
            ScenarioId = scenario.Id
        };

        var response = await client.PostAsJsonAsync("/conversations", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<Conversation>();
        Assert.Equal("New Conversation", created?.Title);
    }

    [Fact]
    public async Task GetUserConversations_ShouldReturnOk_WhenConversationsExist()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var user);
        var response = await client.GetAsync($"/users/{user.Id}/conversations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var conversations = await response.Content.ReadFromJsonAsync<List<Conversation>>();
        Assert.NotEmpty(conversations);
    }

    [Fact]
    public async Task GetConversationById_ShouldReturnOk_WhenExists()
    {
        var dbSeeder = new BaseCaseDb();
        var client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        _factory.SeedDb(scope, dbSeeder);
        var conversation = dbSeeder.Conversations.First();

        var response = await client.GetAsync($"/conversations/{conversation.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CompleteConversation_ShouldReturnOk_WhenExists()
    {
        var dbSeeder = new BaseCaseDb();
        var client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        _factory.SeedDb(scope, dbSeeder);
        var conversation = dbSeeder.Conversations.First();

        var response = await client.PutAsync($"/conversations/{conversation.Id}/complete", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<Conversation>();
        Assert.True(updated?.Completed);
    }

    [Fact]
    public async Task DeleteConversation_ShouldReturnNoContent_WhenExists()
    {
        var dbSeeder = new BaseCaseDb();
        var client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        _factory.SeedDb(scope, dbSeeder);
        var conversation = dbSeeder.Conversations.First();

        var response = await client.DeleteAsync($"/conversations/{conversation.Id}");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
