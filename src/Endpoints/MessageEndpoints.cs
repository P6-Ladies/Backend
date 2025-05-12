// src\Endpoints\MessageEndpoints.cs

using backend.Data;
using backend.Entities.Messages;
using backend.Entities.Conversations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Entities.Messages.DTOs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using backend.Extensions;


namespace backend.Endpoints
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

                var userMessage = new Message
                    {
                        ConversationId = conversationId,
                        Body = request.Message,
                        UserSent = true,
                        ReceivedAt = DateTime.UtcNow
                    };

                try
                {
                    // 1) load conversation + agent + scenario
                    var conversation = await dbContext.Conversations
                        .Include(c => c.Agent)
                        .Include(c => c.Scenario)
                        .FirstOrDefaultAsync(c => c.Id == conversationId);

                    if (conversation == null)
                        return Results.NotFound("Conversation not found.");
                    
                    var messages = dbContext.Messages
                        .Where(m => m.ConversationId == conversationId)
                        .Select(m => new
                        {
                            m.Body,
                            m.ReceivedAt,
                            m.UserSent
                        })
                        .OrderBy(m => m.ReceivedAt)
                        .ToList();

                    if (messages == null)
                        {
                            return Results.NotFound("No messages found.");
                        }
                    

                    var agent = conversation.Agent;
                    var scenario = conversation.Scenario;
                    
                    //Make array of messages for the chat history instead of string, so it can be put into the chat template correctly.
                    List<Object> historyPrompt = [];
                    foreach (var msg in messages)
                        {
                            historyPrompt.Add(new
                            {
                                Sender = msg.UserSent ? "user" : "assistant",
                                Message = msg.Body
                            });
                        }
                    var requestBody = new
                    {
                        Agent = new
                        {
                            Name               = agent.Name,
                            PromptBody         = agent.PromptBody,
                            Openness           = agent.Openness,
                            Conscientiousness  = agent.Conscientiousness,
                            Extroversion       = agent.Extroversion,
                            Agreeableness      = agent.Agreeableness,
                            Neuroticism        = agent.Neuroticism
                        },
                        Scenario = new
                        {
                            Name               = scenario.Name,
                            SettingPrompt      = scenario.SettingPrompt,
                            ConflictPrompt     = scenario.ConflictPrompt,
                            AdditionalPrompt   = scenario.AdditionalPrompt ?? ""
                        },
                        History = historyPrompt,
                        Prompt     = request.Message,
                        MaxLength = 8192
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
                    var generatedText = JsonDocument.Parse(responseJson)
                                                    .RootElement
                                                    .GetProperty("result")
                                                    .GetString();

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
                var messages = dbContext.Messages
                    .Where(m => m.ConversationId == conversationId)
                    .Select(m => new
                    {
                        m.Body,
                        m.ReceivedAt,
                        Sender = m.UserSent ? "User" : "Agent"
                    })
                    .OrderBy(m => m.ReceivedAt)
                    .ToList();

                if (messages == null)
                    {
                        return Results.NotFound("No messages found.");
                    }

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