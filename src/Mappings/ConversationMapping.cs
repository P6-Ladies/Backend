// src\Mappings\UserMapping.cs
using Backend.Endpoints;
using Backend.Entities.Conversations;
using Backend.Entities.Conversations.DTOs;

namespace Backend.Mappings;

public static class ConversationMapping
{
    public static Conversation ToEntity(this CreateConversationDTO conversationDTO)
    {
        // Initialize a new conversation with the things we need
        return new Conversation
        {
            Title = conversationDTO.Title,
            AgentId = conversationDTO.AgentId,
            UserId = conversationDTO.UserId,
            ScenarioId = conversationDTO.ScenarioId
        };
    }

    public static CreateConversationDTO ToCreateDTO(this Conversation conversation)
    {
        return new CreateConversationDTO{
            Title = conversation.Title,
            AgentId = conversation.AgentId,
            ScenarioId = conversation.ScenarioId,
            UserId = conversation.UserId
        };
    }

    public static ShowConversationDTO ToShowDTO(this Conversation conversation)
    {
        return new ShowConversationDTO{
            Title = conversation.Title,
            Completed = conversation.Completed,
            Id = conversation.Id,
            CreatedAt = conversation.CreatedAt
        };
    }
}
