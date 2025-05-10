// src\Entities\Users\DTOs\CreateUserDTO.cs
namespace Backend.Entities.Messages.DTOs
{
    public class SendMessageDTO
    {
        public int ConversationId { get; set; }
        public string? Message { get; set; }
    }
}