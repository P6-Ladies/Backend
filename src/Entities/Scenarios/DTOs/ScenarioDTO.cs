namespace Backend.Entities.Scenarios.DTOs;

public class ScenarioDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Name { get; set; }
    public string? SettingPrompt { get; set; }
    public string? ConflictPrompt { get; set; }
    public string? AdditionalPrompt { get; set; }
    public DateTime CreatedAt { get; internal set; }
}