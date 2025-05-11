// File: testing/backend.IntegrationTests/Endpoints/MessageEndpointTests.cs

using System.Net;
using System.Net.Http.Json;
using Backend.Entities.Messages.DTOs;
using Backend.Entities.Messages;
using Microsoft.Extensions.DependencyInjection;
using Backend.Data;
using System.Text.Json;
using Backend.IntegrationTests.Utils;
using Backend.IntegrationTests.Utils.DbSeeders;
using Backend.Entities.Conversations;
using Xunit;
using ServiceHealthChecker = Backend.IntegrationTests.Utils.ServiceHealthChecker;

namespace Backend.IntegrationTests.Endpoints;

public class MessageEndpointTests : IClassFixture<backendWebApplicationFactory>
{
    private readonly backendWebApplicationFactory _factory;

    public MessageEndpointTests(backendWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddMessageToConversation_ShouldReturnOk_WhenMessageIsValid()
    {
        if (!await ServiceHealthChecker.IsPythonServiceAvailableAsync())
            throw new Xunit.Sdk.SkipException("Skipping test: Python server is not running.");
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var user);
        using var scope = _factory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();

        var conversation = db.Conversations.First();

        var message = new SendMessageDTO
        {
            Message = "Hello, how are you?"
        };

        var response = await client.PostAsJsonAsync($"/conversations/{conversation.Id}/messages", message);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var text = json.GetProperty("response").GetString();
        var msg = json.GetProperty("message").Deserialize<Message>();

        Assert.False(string.IsNullOrEmpty(text));
        Assert.NotNull(msg);
        Assert.False(msg!.UserSent);
    }

    [Fact]
    public async Task AddMessageToConversation_ShouldReturnNotFound_WhenConversationDoesNotExist()
    {
        var client = _factory.CreateClientWithSeed(new EmptyDb(), out _);

        var message = new SendMessageDTO
        {
            Message = "Should not work"
        };

        var response = await client.PostAsJsonAsync("/conversations/999/messages", message);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMessagesInConversation_ShouldReturnOk_WhenConversationExists()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out _);
        using var scope = _factory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();

        var conversation = db.Conversations.First();

        var response = await client.GetAsync($"/conversations/{conversation.Id}/messages");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var messages = await response.Content.ReadFromJsonAsync<List<Message>>();
        Assert.NotNull(messages);
        Assert.True(messages!.Count() >= 2); // user + agent response
    }

    [Fact]
    public async Task GetMessagesInConversation_ShouldReturnNotFound_WhenConversationDoesNotExist()
    {
        var client = _factory.CreateClientWithSeed(new EmptyDb(), out _);
        var response = await client.GetAsync("/conversations/999/messages");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
