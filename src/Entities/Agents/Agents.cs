// src\Entities\Agents\Agents.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Entities.Users;

namespace backend.Entities.Agents;
public class Agent
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public string? Name { get; set; }

    public string? PromptBody { get; set; }

    public int? AvatarId { get; set; }

    public int? Openness { get; set; }
    public int? Conscientiousness { get; set; }
    public int? Extroversion { get; set; }
    public int? Agreeableness { get; set; }
    public int? Neuroticism { get; set; }

    // Navigation property
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
