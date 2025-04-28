namespace backend.Entities.Messages.DTOs;
// DTO for adding a message to a conversation
public sealed class SendMessageDTO
{
    public string Message { get; set; } = default!;
}