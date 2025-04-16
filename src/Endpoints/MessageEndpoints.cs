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

public static class MessageEndpoints
{
    public static RouteGroupBuilder MapMessageEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("messages");

        group.MapGet("/{conversationId}", async (int conversationId, PrototypeDbContext dBContext) =>
        {
            
            try {
                var conversation = dBContext.Conversations.Single(c => c.Id == conversationId);
                List<Message> messages = dBContext.Messages.FromSqlRaw("SELECT * FROM messages WHERE \"ConversationId\" = {0}", conversationId).ToList();
                
                List<MessageDTO> dtoMessages = new List<MessageDTO>();
                foreach (Message message in messages) {
                    dtoMessages.Add(message.ToDTO());
                }
                ConversationMessagesDTO finalDTO = new ConversationMessagesDTO{Title = conversation.Title, Completed = conversation.Completed, Messages = dtoMessages};
    
                
                return Results.Ok(finalDTO);
            } catch (Exception ex) {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
            
        })
        .WithName("ShowConversationMessages")
        .WithTags("Messages")
        .WithDescription("Shows title, completed and all conversation messages to user.")
        .Produces<ConversationMessagesDTO>(StatusCodes.Status201Created)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);

        group.MapPost("/", async (SendMessageDTO sentMessageDTO, PrototypeDbContext dBContext) =>
        {
            
            try {
                Message message = sentMessageDTO.ToEntity();
                dBContext.Messages.Add(message);
                await dBContext.SaveChangesAsync();
            
                return Results.Ok(message.ToDTO());
            } catch (Exception ex) {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
            
        })
        .WithName("SendMessage")
        .WithTags("Messages")
        .WithDescription("Sends a message to the database.")
        .Produces<MessageDTO>(StatusCodes.Status201Created)
        .Produces<IEnumerable<IdentityError>>(StatusCodes.Status400BadRequest);

    return group;
    }
}