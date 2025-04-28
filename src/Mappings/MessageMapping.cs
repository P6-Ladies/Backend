// src\Mappings\UserMapping.cs
using backend.Endpoints;
using backend.Entities.Messages;
using backend.Entities.Messages.DTOs;

namespace backend.Mappings;

public static class MessageMapping
{
    public static Message ToEntity(this SendMessageDTO messageDTO)
    {
        // Initialize a new conversation with the things we need
        return new Message
        {
            Body = messageDTO.Message,
            ReceivedAt = DateTime.UtcNow,
            UserSent = true,
        };
    }

    public static MessageDTO ToDTO(this Message message)
    {
        return new MessageDTO{
            Body = message.Body,
            UserSent = message.UserSent,
            ReceivedAt = message.ReceivedAt
        };
    }
}
