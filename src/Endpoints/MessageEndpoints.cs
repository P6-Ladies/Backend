// src\Endpoints\MessageEndpoints.cs

using Backend.Data;
using Backend.Entities.Messages;
using Backend.Entities.Conversations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Entities.Messages.DTOs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;


namespace Backend.Endpoints
{
    public static class MessageEndpoints
    {
        // Register the message-related endpoints
        public static void MapMessageEndpoints(this IEndpointRouteBuilder app)
        {
            // Add a message to a conversation
            app.MapPost("/conversations/{conversationId}/messages", async (int conversationId, [FromBody] SendMessageDTO request, PrototypeDbContext dbContext, IHttpClientFactory httpClientFactory, HttpContext httpContext) =>
            {
                if (string.IsNullOrEmpty(request.Message))
                {
                    return Results.BadRequest("Message cannot be empty.");
                }

                try
                {
                    // Retrieve the conversation and agent from the database
                    var conversation = await dbContext.Conversations
                        .Include(c => c.Agent)
                        .FirstOrDefaultAsync(c => c.Id == conversationId);
                    
                    if (conversation == null)
                    {
                        return Results.NotFound("Conversation not found.");
                    }

                    // Construct the JSON body for the Python microservice
                    var requestBody = new
                    {
                        prompt = request.Message,
                        max_length = 256 // Set maximum length for the LLM response
                    };

                    var jsonBody = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    var client = httpClientFactory.CreateClient("HF");

                    // Send request to the Python microservice to generate the response
                    using var response = await client.PostAsync("generate", content, httpContext.RequestAborted);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        return Results.Problem($"Error from Python service: {response.StatusCode}");
                    }

                    // Parse the generated text from the response
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var generatedText = JsonDocument.Parse(responseJson).RootElement.GetProperty("result").GetString();

                    if (string.IsNullOrEmpty(generatedText))
                    {
                        return Results.Problem("Failed to generate a valid response.");
                    }

                    // Store the user message and agent response in the database
                    var userMessage = new Message
                    {
                        ConversationId = conversationId,
                        Body = request.Message,
                        UserSent = true,
                        ReceivedAt = DateTime.UtcNow
                    };
                    var agentResponse = new Message
                    {
                        ConversationId = conversationId,
                        Body = generatedText,
                        UserSent = false,
                        ReceivedAt = DateTime.UtcNow
                    };

                    dbContext.Messages.AddRange(userMessage, agentResponse);
                    await dbContext.SaveChangesAsync();

                    // Return the latest message (agent's response as the last message)
                    return Results.Ok(new { response = generatedText, message = agentResponse });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"An error occurred: {ex.Message}");
                }
            })
            .WithName("AddMessageToConversation")
            .WithTags("Messages")
            .WithDescription("Adds a message to a conversation.")
            .Accepts<SendMessageDTO>("application/json")
            .Produces<Message>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound);

            // Get all messages in a conversation (with text, date, and sender)
            app.MapGet("/conversations/{conversationId}/messages", async (int conversationId, PrototypeDbContext dbContext) =>
            {
                var conversation = await dbContext.Conversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == conversationId);

                if (conversation == null)
                {
                    return Results.NotFound("Conversation not found.");
                }

                // Retrieve all messages and include who sent them (user/agent) and timestamp
                var messages = conversation.Messages
                    .Select(m => new
                    {
                        m.Body,
                        m.ReceivedAt,
                        Sender = m.UserSent ? "User" : "Agent"
                    })
                    .OrderBy(m => m.ReceivedAt)
                    .ToList();

                return Results.Ok(messages);
            })
            .WithName("GetMessagesInConversation")
            .WithTags("Messages")
            .WithDescription("Retrieves all messages in a conversation, including who sent them and when.")
            .Produces<List<Message>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}