// src\Entities\Users\DTOs\CreateUserDTO.cs
using System.ComponentModel.DataAnnotations;

namespace backend.Entities.Messages.DTOs;
public record SendMessageDTO
{
    [Required]
    [StringLength(2400)]
    public required string Body { get; set; }

    [Required]
    public required int ConversationId { get; set; }
}