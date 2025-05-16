// testing\backend.IntegrationTests\Utils\DbSeeders\BaseCaseDb.cs
using Backend.Data;
using Backend.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace Backend.IntegrationTests.Utils.DbSeeders;

public class BaseCaseDb : DbSeeder
{
    public override void SeedData(PrototypeDbContext dbContext, UserManager<User> userManager)
    {
        SeedUsers(userManager);
        SeedAgent(dbContext, Users["testuser"]);
        SeedScenario(dbContext, Users["testuser"]);
        SeedConversation(dbContext, Users["testuser"], Agents.First(), Scenarios.First());
        SeedMessage(dbContext, Conversations.First());
        SeedAssessment(dbContext, Users["testuser"], Conversations.First());
    }
}