using Backend.Data;
using Backend.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Backend.IntegrationTests.Utils.DbSeeders;

namespace Backend.IntegrationTests.Utils.DbSeeders;

public class EmptyDb : DbSeeder
{
    public override void SeedData(PrototypeDbContext dbContext, UserManager<User> userManager)
    {
    }
}