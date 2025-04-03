// src\Entities\Conversations\Conversations.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Entities.Agents;
using Backend.Entities.Assessments;
using Backend.Entities.Messages;
using Backend.Entities.Scenarios;
using Backend.Entities.Users;

namespace Backend.Entities.Conversations;
public class Conversation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int AgentId { get; set; }

    public int? ScenarioId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TimeSpan? TimeElapsed { get; set; }

    public int? MessageCount { get; set; }
    public int? TokenCount { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(AgentId))]
    public Agent Agent { get; set; } = null!;

    [ForeignKey(nameof(ScenarioId))]
    public Scenario? Scenario { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
}
