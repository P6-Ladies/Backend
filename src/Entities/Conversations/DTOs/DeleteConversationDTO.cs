// src\Entities\Users\DTOs\CreateUserDTO.cs
using System.ComponentModel.DataAnnotations;

namespace backend.Entities.Conversations.DTOs;
public record DeleteConversationDTO
{
    [Required]
    public required int Id { get; set; }
}