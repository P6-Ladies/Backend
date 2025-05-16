using Backend.Entities.Assessments.DTOs;
using Backend.Entities.Messages.DTOs;

namespace Backend.Entities.Conversations.DTOs;

public record ConversationDetailDTO
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required bool Completed { get; set; }
    public required DateTime CreatedAt { get; set; }
    public List<MessageDTO> Messages { get; set; } = new();
    public List<AssessmentDTO> Assessments { get; set; } = new();
}