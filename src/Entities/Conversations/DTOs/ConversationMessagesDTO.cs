// src\Entities\Users\DTOs\CreateUserDTO.cs
using System.ComponentModel.DataAnnotations;
using backend.Entities.Messages.DTOs;

namespace backend.Entities.Conversations.DTOs;
public record ConversationMessagesDTO
{
    [Required]
    [StringLength(100)]
    public required string Title { get; set; }
    public required bool Completed { get; set; }
    public required List<MessageDTO> Messages { get; set; }

}