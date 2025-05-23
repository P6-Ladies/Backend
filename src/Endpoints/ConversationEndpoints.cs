// src\Endpoints\UserEndpoints.cs

using Backend.Entities.Conversations;
using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Entities.Agents;
using Backend.Entities.Agents.DTOs;
using Backend.Entities.Scenarios;
using Backend.Entities.Scenarios.DTOs;
using Microsoft.EntityFrameworkCore;
using Backend.Entities.Messages;
using Backend.Entities.Messages.DTOs;
using Backend.Entities.Conversations;
using Backend.Entities.Conversations.DTOs;
using Microsoft.AspNetCore.Identity;
using Backend.Mappings;
using Backend.Entities.Assessments.DTOs;


namespace Backend.Endpoints;

public static class ConversationEndpoints
{
    // Register the conversation-related endpoints
    public static RouteGroupBuilder MapConversationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("conversations");

        // Create a new conversation
        group.MapPost("/", async ([FromBody] CreateConversationDTO request, PrototypeDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Results.BadRequest("Title is required.");

            var agent = await db.Agents.FindAsync(request.AgentId);
            if (agent is null)
                return Results.NotFound("Agent not found.");

            var scenario = await db.Scenarios.FindAsync(request.ScenarioId);
            if (scenario is null)
                return Results.NotFound("Scenario not found.");

            var conversation = new Conversation
            {
                Title = request.Title,
                UserId = request.UserId,
                AgentId = request.AgentId,
                ScenarioId = request.ScenarioId,
                CreatedAt = DateTime.UtcNow,
                Completed = false
            };

            db.Conversations.Add(conversation);
            await db.SaveChangesAsync();

            return Results.Created($"/conversations/{conversation.Id}", conversation);
        })
        .WithName("CreateConversation")
        .WithTags("Conversations")
        .WithDescription("Creates a new conversation with an agent and scenario.")
        .Accepts<CreateConversationDTO>("application/json")
        .Produces<Conversation>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        // Get all conversations for a user
        group.MapGet("/user/{userId}", async (int userId, PrototypeDbContext db) =>
        {
            var conversations = await db.Conversations
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (conversations.Count == 0)
                return Results.NotFound("No conversations found.");

            var dtos = conversations.Select(c => c.ToShowDTO()).ToList();
            return Results.Ok(new ListConversationsDTO { Conversations = dtos });
        })
        .WithName("GetUserConversations")
        .WithTags("Conversations")
        .WithDescription("Retrieves all conversations for a user.")
        .Produces<List<Conversation>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // Get a specific conversation by its ID
        app.MapGet("/conversations/{conversationId}", async (int conversationId, PrototypeDbContext db) =>
        {
            var convo = await db.Conversations
                .Include(c => c.Agent)
                .Include(c => c.Scenario)
                .Include(c => c.Messages)
                .Where(c => c.Id == conversationId)
                .Select(c => new ConversationDTO
                {
                    Id = c.Id,
                    Title = c.Title,
                    CreatedAt = c.CreatedAt,
                    Agent = new AgentDTO
                    {
                        Id = c.Agent.Id,
                        Name = c.Agent.Name
                    },
                    Scenario = c.Scenario == null ? null : new ScenarioDTO
                    {
                        Id = c.Scenario.Id,
                        Name = c.Scenario.Name
                    },
                    Messages = c.Messages.Select(m => new MessageDTO
                    {
                        Body = m.Body,
                        UserSent = m.UserSent,
                        ReceivedAt = m.ReceivedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return convo is null
                ? Results.NotFound("Conversation not found.")
                : Results.Ok(convo);
        })
        .WithName("GetConversationById")
        .WithTags("Conversations")
        .WithDescription("Retrieves a specific conversation by its ID.")
        .Produces<Conversation>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // Mark conversation as completed
        app.MapPut("/conversations/{conversationId}/complete", async (int conversationId, [FromServices] IHttpClientFactory http, [FromServices] PrototypeDbContext db) =>
        {
            var convo = await db.Conversations.FindAsync(conversationId);
            if (convo is null)
                return Results.NotFound("Conversation not found.");

            convo.Completed = true;
            await db.SaveChangesAsync();
            // Trigger the assessment process
            var client = http.CreateClient("Internal");

            var assessmentRequest = new CreateAssessmentDTO
            {
                ConversationId = conversationId,
                UserId = convo.UserId
            };

            var response = await client.PostAsJsonAsync("/assessments", assessmentRequest);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return Results.Problem(
                    detail: $"Failed to trigger assessment: {err}",
                    statusCode: (int)response.StatusCode);
            }

            return Results.Ok("Marked as completed and assessment triggered.");
        })
        .WithName("CompleteConversation")
        .WithTags("Conversations")
        .WithDescription("Marks the conversation as completed and triggers the assessment.");

        // Delete a conversation
        app.MapDelete("/conversations/{conversationId}", async (int conversationId, PrototypeDbContext dbContext) =>
        {
            var conversation = await dbContext.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                return Results.NotFound("Conversation not found.");
            }

            dbContext.Conversations.Remove(conversation);
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteConversation")
        .WithTags("Conversations")
        .WithDescription("Deletes a specific conversation.");

    return group;
    }
}