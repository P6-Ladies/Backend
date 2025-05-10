// testing\backend.IntegrationTests\Utils\DbSeeders\BaseCaseDb.cs
using backend.Data;
using backend.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace backend.IntegrationTests.Utils.DbSeeders;

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