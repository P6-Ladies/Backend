namespace Backend.Entities.Agents.DTOs
{
    public class UpdateAgentDTO
    {
        public string? Name { get; set; }
        public string? PromptBody { get; set; }
        public int? AvatarId { get; set; }
        public int? Openness { get; set; }
        public int? Conscientiousness { get; set; }
        public int? Extroversion { get; set; }
        public int? Agreeableness { get; set; }
        public int? Neuroticism { get; set; }
    }
}
