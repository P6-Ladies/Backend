// src\Entities\Assesments\Assesments.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Entities.Conversations;
using Backend.Entities.Users;

namespace Backend.Entities.Assessments;
public class Assessment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ConversationId { get; set; }

    public string? Body { get; set; }
    public string? ConflictManagementStrategy { get; set; }

    // Big Five
    public int? Openness { get; set; }
    public int? Conscientiousness { get; set; }
    public int? Extroversion { get; set; }
    public int? Agreeableness { get; set; }
    public int? Neuroticism { get; set; }

    // Navigation
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [ForeignKey(nameof(ConversationId))]
    public Conversation Conversation { get; set; } = null!;
}
