// src\Entities\Messages\Messages.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Entities.Conversations;

namespace backend.Entities.Messages;
public class Message
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ConversationId { get; set; }

    public bool UserSent { get; set; }
    public string? Body { get; set; }

    [Required]
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ConversationId))]
    public Conversation Conversation { get; set; } = null!;
}
