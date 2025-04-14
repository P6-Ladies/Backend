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

namespace backend.Endpoints;

public static class ConversationEndpoints
{
    public static RouteGroupBuilder MapConversationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("conversations");

        // POST /conversation
        group.MapPost("/", async (CreateConversationDTO newConversationDTO, PrototypeDbContext dBContext) =>
        {
            dBContext.Agents.Add(new Agent{Id = 0, UserId = 0});
            dBContext.Scenarios.Add(new Scenario{Id = 0, UserId = 0});
            Conversation conversation = newConversationDTO.ToEntity();
            try {
                dBContext.Conversations.Add(conversation);
                await dBContext.SaveChangesAsync();
                return  Results.Created($"/conversations/{conversation.Id}", newConversationDTO);
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

    return group;
    }
}