using backend.Data;
using backend.Entities.Users;
using Microsoft.AspNetCore.Identity;
using backend.IntegrationTests.Utils.DbSeeders;

namespace backend.IntegrationTests.Utils.DbSeeders;

public class EmptyDb : DbSeeder
{
    public override void SeedData(PrototypeDbContext dbContext, UserManager<User> userManager)
    {
    }
}