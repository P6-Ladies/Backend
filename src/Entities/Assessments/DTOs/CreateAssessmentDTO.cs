namespace Backend.Entities.Assessments.DTOs
{
    // DTO for creating a new assessment
    public class CreateAssessmentDTO
    {
        public int UserId { get; set; }
        public int ConversationId { get; set; }
        public string? Body { get; set; }
        public string? ConflictManagementStrategy { get; set; }

        // Big Five
        public int? Openness { get; set; }
        public int? Conscientiousness { get; set; }
        public int? Extroversion { get; set; }
        public int? Agreeableness { get; set; }
        public int? Neuroticism { get; set; }
    }
}
