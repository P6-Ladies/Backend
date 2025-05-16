// src\Entities\Scenarios\Scenarios.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Entities.Users;

namespace backend.Entities.Scenarios;
public class Scenario
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public required string Name { get; set; }
    public string? SettingPrompt { get; set; }
    public string? ConflictPrompt { get; set; }
    public string? AdditionalPrompt { get; set; }
    public string? InitialUserMessage { get; set; }
    public string? InitialAgentMessage { get; set; }
    public DateTime CreatedAt { get; internal set; }

    // Navigation property
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}