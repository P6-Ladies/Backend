// src\Extensions\ApplicationBuilder.cs
using backend.Data;
using backend.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Extensions
{
    public static class ApplicationBuilder
    {
        public static async Task SeedDataAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            // Create a user
            var user = await userManager.FindByEmailAsync("user@user.com");
            if (user == null)
            {
                user = new User { Email = "user@user.com" };
                await userManager.CreateAsync(user, "Password123");
            }
        }
        
        public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}