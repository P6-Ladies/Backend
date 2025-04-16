// src\Endpoints\UserEndpoints.cs

using backend.Entities.Conversations.DTOs;
using backend.Entities.Conversations;
using backend.Mappings;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Entities.Agents;
using backend.Entities.Scenarios;
using Microsoft.EntityFrameworkCore;
using backend.Entities.Messages;
using backend.Entities.Messages.DTOs;


namespace backend.Endpoints;

public static class ConversationEndpoints
{
    public static RouteGroupBuilder MapConversationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("conversations");

        // POST /conversation
        group.MapPost("/", async (CreateConversationDTO newConversationDTO, PrototypeDbContext dBContext) =>
        {
            Conversation conversation = newConversationDTO.ToEntity();
            try {
                dBContext.Conversations.Add(conversation);
                await dBContext.SaveChangesAsync();
                return  Results.Created($"/conversations/{conversation.Id}", conversation.ToShowDTO());
            } catch (Exception ex) {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
            
        })
        .WithName("CreateConversation")
        .WithTags("Conversations")
        .WithDescription("Creates a new conversation with the specified details.")
        .Accepts<CreateConversationDTO>("application/json")
        .Produces<Conversation>(StatusCodes.Status201Created)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);

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