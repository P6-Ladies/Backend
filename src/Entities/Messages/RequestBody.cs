using Backend.Entities.Agents;
using Backend.Entities.Scenarios;

namespace Backend.Entities.Messages;

public class RequestBody {
    public Agent Agent;
    public Scenario Scenario;
    public string Prompt;
    public int MaxLength;
}