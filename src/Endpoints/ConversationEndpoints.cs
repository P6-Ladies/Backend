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
using Backend.Entities.Conversations.DTOs;
using Microsoft.AspNetCore.Identity;
using Backend.Mappings;


namespace Backend.Endpoints;

public static class ConversationEndpoints
{
    // Register the conversation-related endpoints
    public static RouteGroupBuilder MapConversationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("conversations");

        // Create a new conversation
        group.MapPost("/", async ([FromBody] CreateConversationDTO request, PrototypeDbContext dbContext) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Results.BadRequest("Title is required.");

            var agent = await dbContext.Agents.FindAsync(request.AgentId);
            if (agent is null)
                return Results.NotFound("Agent not found.");

            var scenario = await dbContext.Scenarios.FindAsync(request.ScenarioId);
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
            
            dbContext.Conversations.Add(conversation);

            await dbContext.SaveChangesAsync();

            // Add initial messages from scenario.
            var initialUserMessage = new Message{Body = scenario.InitialUserMessage, UserSent = true, ConversationId = conversation.Id};
            var initialAgentMessage = new Message{Body = scenario.InitialAgentMessage, UserSent = false, ConversationId = conversation.Id, ReceivedAt = DateTime.UtcNow.AddMilliseconds(50)};

            dbContext.Messages.Add(initialUserMessage);
            dbContext.Messages.Add(initialAgentMessage);
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
        app.MapPut("/conversations/{conversationId}/complete", async (int conversationId, PrototypeDbContext db) =>
        {
            var convo = await db.Conversations.FindAsync(conversationId);
            if (convo is null)
                return Results.NotFound("Conversation not found.");

            convo.Completed = true;
            await db.SaveChangesAsync();

            return Results.Ok("Marked as completed.");
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

/*        group.MapGet("/{userId}", (int userId, PrototypeDbContext dBContext) =>
        {

            try
            {
                List<Conversation> conversations = dBContext.Conversations.FromSqlRaw("SELECT * FROM conversations WHERE \"UserId\" = {0}", userId).ToList();
                List<ShowConversationDTO> dtoConversations = new List<ShowConversationDTO>();
                foreach (Conversation conversation in conversations)
                {
                    dtoConversations.Add(conversation.ToShowDTO());
                }
                ListConversationsDTO listConversationsDto = new ListConversationsDTO { Conversations = dtoConversations };
                return Results.Ok(listConversationsDto);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }

        })
        .WithName("ShowConversations")
        .WithTags("Conversations")
        .WithDescription("Shows all conversations for specified user.")
        .Produces<ShowConversationDTO>(StatusCodes.Status201Created)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);

        group.MapDelete("/{conversationId}", async (int id, PrototypeDbContext db) =>
        {
            var convo = await db.Conversations.FindAsync(id);
            if (convo is null)
                return Results.NotFound("Conversation not found.");

            db.Conversations.Remove(convo);
            await db.SaveChangesAsync();

            return Results.NoContent();
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
*/
    return group;
    }
}