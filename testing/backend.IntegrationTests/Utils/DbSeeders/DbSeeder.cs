// backend.IntegrationTests/Utils/DbSeeders/DbSeeder.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend.Data;
using Backend.Entities.Agents;
using Backend.Entities.Assessments;
using Backend.Entities.Conversations;
using Backend.Entities.Messages;
using Backend.Entities.Scenarios;
using Backend.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace Backend.IntegrationTests.Utils.DbSeeders;

public abstract class DbSeeder
{
    public Dictionary<string, User> Users { get; } = new();
    public List<Agent> Agents { get; } = new();
    public List<Scenario> Scenarios { get; } = new();
    public List<Conversation> Conversations { get; } = new();
    public List<Message> Messages { get; } = new();
    public List<Assessment> Assessments { get; } = new();

    public abstract void SeedData(PrototypeDbContext dbContext, UserManager<User> userManager);

    public void SeedUsers(UserManager<User> userManager)
    {
        var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
        var email = $"testuser_{uniqueId}@email.com";

        var user = new User
        {
            UserName = email,
            Email = email
        };

        var result = userManager.CreateAsync(user, "Password123!").GetAwaiter().GetResult();
        
        if (!result.Succeeded)
        {
            var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create test user: {errorMessages}");
        }

        Users["testuser"] = user;

        var claim1 = new Claim(ClaimTypes.Name, user.UserName!);
        var claim2 = new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString());

        userManager.AddClaimAsync(user, claim1).GetAwaiter().GetResult();
        userManager.AddClaimAsync(user, claim2).GetAwaiter().GetResult();
    }

    public void SeedAgent(PrototypeDbContext dbContext, User user)
    {
        var agent = new Agent
        {
            UserId = user.Id,
        };
        dbContext.Agents.Add(agent);
        dbContext.SaveChanges();
        Agents.Add(agent);
    }

    public void SeedScenario(PrototypeDbContext dbContext, User user)
    {
        var scenario = new Scenario
        {
            UserId = user.Id
        };
        dbContext.Scenarios.Add(scenario);
        dbContext.SaveChanges();
        Scenarios.Add(scenario);
    }

    public void SeedConversation(PrototypeDbContext dbContext, User user, Agent agent, Scenario scenario)
    {
        var conversation = new Conversation
        {
            UserId = user.Id,
            AgentId = agent.Id,
            ScenarioId = scenario.Id,
            Title = "Test Conversation"
        };
        dbContext.Conversations.Add(conversation);
        dbContext.SaveChanges();
        Conversations.Add(conversation);
    }

    public void SeedMessage(PrototypeDbContext dbContext, Conversation conversation)
    {
        var message = new Message
        {
            ConversationId = conversation.Id
        };
        dbContext.Messages.Add(message);
        dbContext.SaveChanges();
        Messages.Add(message);
    }

    public void SeedAssessment(PrototypeDbContext dbContext, User user, Conversation conversation)
    {
        var assessment = new Assessment
        {
            UserId = user.Id,
            ConversationId = conversation.Id
        };
        dbContext.Assessments.Add(assessment);
        dbContext.SaveChanges();
        Assessments.Add(assessment);
    }
}