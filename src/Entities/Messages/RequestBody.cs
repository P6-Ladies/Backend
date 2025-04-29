using backend.Entities.Agents;
using backend.Entities.Scenarios;

namespace backend.Entities.Messages;

public class RequestBody {
    public Agent Agent;
    public Scenario Scenario;
    public string Prompt;
    public int MaxLength;
}