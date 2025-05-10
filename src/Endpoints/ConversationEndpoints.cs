// src\Endpoints\UserEndpoints.cs

using backend.Entities.Conversations;
using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Entities.Agents;
using backend.Entities.Scenarios;
using Microsoft.EntityFrameworkCore;
using backend.Entities.Messages;
using backend.Entities.Messages.DTOs;
using backend.Entities.Conversations.DTOs;


namespace backend.Endpoints;

public static class ConversationEndpoints
{
    // Register the conversation-related endpoints
    public static RouteGroupBuilder MapConversationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("conversations");

        // Create a new conversation
        app.MapPost("/conversations", async ([FromBody] CreateConversationDTO request, PrototypeDbContext dbContext) =>
        {
            if (string.IsNullOrEmpty(request.Title))
            {
                return Results.BadRequest("Title is required.");
            }

            // Check if the agent and scenario exist
            var agent = await dbContext.Agents.FindAsync(request.AgentId);
            var scenario = await dbContext.Scenarios.FindAsync(request.ScenarioId);

            if (agent == null)
            {
                return Results.NotFound("Agent not found.");
            }

            if (scenario == null)
            {
                return Results.NotFound("Scenario not found.");
            }

            // Create a new conversation
            var conversation = new Conversation
            {
                Title = request.Title,
                UserId = request.UserId,
                AgentId = request.AgentId,
                ScenarioId = request.ScenarioId,
                CreatedAt = DateTime.UtcNow,
                Completed = false
            };

            dbContext.Conversations.Add(conversation);
            await dbContext.SaveChangesAsync();

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
        app.MapGet("/users/{userId}/conversations", async (int userId, PrototypeDbContext dbContext) =>
        {
            var conversations = await dbContext.Conversations
                .Where(c => c.UserId == userId)
                .Select(c => new 
                {
                    c.Id,
                    c.Title,
                    c.CreatedAt,
                    c.Completed
                })
                .ToListAsync();

            if (conversations.Count == 0)
            {
                return Results.NotFound("No conversations found for this user.");
            }

            return Results.Ok(conversations);
        })
        .WithName("GetUserConversations")
        .WithTags("Conversations")
        .WithDescription("Retrieves all conversations for a user.")
        .Produces<List<Conversation>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);


        // Get a specific conversation by its ID
        app.MapGet("/conversations/{conversationId}", async (int conversationId, PrototypeDbContext dbContext) =>
        {
            var conversation = await dbContext.Conversations
                .Include(c => c.Agent)
                .Include(c => c.Scenario)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
            {
                return Results.NotFound("Conversation not found.");
            }

            return Results.Ok(conversation);
        })
        .WithName("GetConversationById")
        .WithTags("Conversations")
        .WithDescription("Retrieves a specific conversation by its ID.")
        .Produces<Conversation>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // Mark conversation as completed
        app.MapPut("/conversations/{conversationId}/complete", async (int conversationId, PrototypeDbContext dbContext) =>
        {
            var conversation = await dbContext.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                return Results.NotFound("Conversation not found.");
            }

            conversation.Completed = true; // Update the status to completed
            dbContext.Conversations.Update(conversation);
            await dbContext.SaveChangesAsync();

            // Trigger assessment logic here

            return Results.Ok(conversation);
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

        group.MapGet("/{userId}", async (int userId, PrototypeDbContext dBContext) =>
        {
            
            try {
                List<Conversation> conversations = dBContext.Conversations.FromSqlRaw("SELECT * FROM conversations WHERE \"UserId\" = {0}", userId).ToList();
                List<ShowConversationDTO> dtoConversations = new List<ShowConversationDTO>();
                foreach (Conversation conversation in conversations) {
                    dtoConversations.Add(conversation.ToShowDTO());
                }
                ListConversationsDTO listConversationsDto = new ListConversationsDTO{Conversations = dtoConversations};
                return Results.Ok(listConversationsDto);
            } catch (Exception ex) {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
            
        })
        .WithName("ShowConversations")
        .WithTags("Conversations")
        .WithDescription("Shows all conversations for specified user.")
        .Produces<ShowConversationDTO>(StatusCodes.Status201Created)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);

        group.MapDelete("/{conversationId}", async (int conversationId, PrototypeDbContext dBContext) =>
        {
            
            try {
                dBContext.Remove(dBContext.Conversations.Single(c => c.Id == conversationId));
                await dBContext.SaveChangesAsync();
                return Results.Ok("Successfully deleted");
            } catch (Exception ex) {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
            
        })
        .WithName("DeleteConversation")
        .WithTags("Conversations")
        .WithDescription("Deletes conversation with specified ID.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);


        //THIS SHOULD ALSO START THE ANALYSIS PROCESS
        group.MapGet("/setComplete/{conversationId}", async (int conversationId, PrototypeDbContext dBContext) =>
        {
            try {
                var conversation = dBContext.Conversations.Single(c => c.Id == conversationId);
                conversation.Completed = true;
                await dBContext.SaveChangesAsync();
                return  Results.Ok("Successfully set completed");
            } catch (Exception ex) {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
            
        })
        .WithName("SetCompleted")
        .WithTags("Conversations")
        .WithDescription("Sets completed to true for the given conversation.")
        .Produces(StatusCodes.Status200OK)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);

    return group;
    }
}