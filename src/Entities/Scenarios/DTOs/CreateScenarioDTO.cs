namespace Backend.Entities.Scenarios.DTOs
{
    public class CreateScenarioDTO
    {
        public required string Name { get; set; }
        public int UserId { get; set; }
        public string? SettingPrompt { get; set; }
        public string? ConflictPrompt { get; set; }
        public string? AdditionalPrompt { get; set; }
    }
}
