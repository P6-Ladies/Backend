// src\Entities\Users\DTOs\CreateUserDTO.cs
using System.ComponentModel.DataAnnotations;

namespace Backend.Entities.Conversations.DTOs;
public record CreateConversationDTO
{
    [Required]
    [StringLength(100)]
    public required string Title { get; set; }

    [Required]
    public required int AgentId { get; set; }

    [Required]
    public required int? ScenarioId { get; set; }

    [Required]
    public required int UserId { get; set; }

}