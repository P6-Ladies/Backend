namespace backend.Entities.Scenarios.DTOs
{
    public class UpdateScenarioDTO
    {
        public string? Name { get; set; }
        public string? SettingPrompt { get; set; }
        public string? ConflictPrompt { get; set; }
        public string? AdditionalPrompt { get; set; }
    }
}
