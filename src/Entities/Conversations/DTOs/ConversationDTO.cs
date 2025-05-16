using Backend.Entities.Agents.DTOs;
using Backend.Entities.Scenarios.DTOs;
using Backend.Entities.Messages.DTOs;

namespace Backend.Entities.Conversations.DTOs;

public class ConversationDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int UserId { get; set; }
    public int AgentId { get; set; }
    public int? ScenarioId { get; set; }
    public bool Completed { get; set; }
    public DateTime CreatedAt { get; set; }
    public TimeSpan? TimeElapsed { get; set; }
    public int? MessageCount { get; set; }
    public int? TokenCount { get; set; }
    public AgentDTO Agent { get; set; } = null!;
    public ScenarioDTO? Scenario { get; set; }
    public List<MessageDTO> Messages { get; set; } = new();
}