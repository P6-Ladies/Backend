// src\Entities\Users\DTOs\CreateUserDTO.cs
using System.ComponentModel.DataAnnotations;

namespace backend.Entities.Conversations.DTOs;
public record ShowConversationDTO
{
    [Required]
    [StringLength(100)]
    public required string Title { get; set; }

    [Required]
    public required int Id { get; set; }

    [Required]
    public required bool Completed {get; set; }

    [Required]
    public required DateTime CreatedAt { get; set; }

}