// src/Entities/Agents/DTOs/AgentDTO.cs
using System.ComponentModel.DataAnnotations;

namespace Backend.Entities.Agents.DTOs;

public class AgentDTO
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = default!;

    [Required]
    public int UserId { get; set; }

    public string? PromptBody { get; set; }

    public string? AvatarId { get; set; }

    public float? Openness { get; set; }

    public float? Conscientiousness { get; set; }

    public float? Extroversion { get; set; }

    public float? Agreeableness { get; set; }

    public float? Neuroticism { get; set; }
}