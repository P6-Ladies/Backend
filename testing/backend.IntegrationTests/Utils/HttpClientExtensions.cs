
using System.Net.Http.Headers;
using System.Security.Claims;
using Backend.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Backend.IntegrationTests.Utils;
using Backend.IntegrationTests.Utils.DbSeeders;

namespace Backend.IntegrationTests.Utils;

public static class HttpClientExtensions
{
    public static void AttachClaimsToken(this HttpClient httpClient, IServiceScope scope, User user)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var claims = userManager.GetClaimsAsync(user).GetAwaiter().GetResult().ToList();
        
        var token = new TestJwtToken();
        var tokenString = token.Build(claims);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenString);
    }
    public static HttpClient CreateClientWithSeed(this backendWebApplicationFactory factory, DbSeeder seeder, out User testUser)
    {
        var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();

        factory.SeedDb(scope, seeder);

        if (seeder is DbSeeder prototypeSeeder && prototypeSeeder.Users.TryGetValue("testuser", out var seededUser))
        {
            testUser = seededUser;
            client.AttachClaimsToken(scope, testUser);
        }
        else
        {
            testUser = null!;
        }

        return client;
    }
}