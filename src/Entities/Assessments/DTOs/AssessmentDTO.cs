namespace Backend.Entities.Assessments.DTOs
{
    public class AssessmentDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Body { get; set; }
        public string? ConflictManagementStrategy { get; set; }

        // Big Five
        public int? Openness { get; set; }
        public int? Conscientiousness { get; set; }
        public int? Extroversion { get; set; }
        public int? Agreeableness { get; set; }
        public int? Neuroticism { get; set; }
        public int ConversationId { get; set; }
    }
}