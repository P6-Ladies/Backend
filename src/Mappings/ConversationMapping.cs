// src\Mappings\UserMapping.cs
using backend.Endpoints;
using backend.Entities.Conversations;
using backend.Entities.Conversations.DTOs;

namespace backend.Mappings;

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

    public static CreateConversationDTO ToDTO(this Conversation conversation)
    {
        return new CreateConversationDTO{
            Title = conversation.Title,
            AgentId = conversation.AgentId,
            ScenarioId = conversation.ScenarioId,
            UserId = conversation.UserId
        };
    }
}
