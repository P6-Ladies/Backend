// testing\backend.IntegrationTests\Utils\ApplicationFactory.cs
using System.Text;
using backend.IntegrationTests.Utils.DbSeeders;
using backend.Security.Authorization;
using backend.Security.Configuration;
using backend.Data;
using backend.Entities.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.Tokens;

namespace backend.IntegrationTests.Utils;

// This factory creates a backend web API configured for testing.
public class backendWebApplicationFactory : WebApplicationFactory<Program>
{

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); // Set the environment to "Testing"
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<PrototypeDbContext>));

            // Use a unique SQLite database file for each test to avoid concurrency issues
            var dbFileName = $"backendTestDb_{Guid.NewGuid()}.db";

            // Configure the DbContext for testing
            services.AddDbContext<PrototypeDbContext>(options =>
            {
                options.UseSqlite($"Data Source={dbFileName}");
            });

            // Configure JwtSettings for testing
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidIssuer = "TestIssuer";
                options.TokenValidationParameters.ValidAudience = "TestAudience";
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisIsASecretKeyForTestingPurposes!"));
            });

            // Add authorization policies
            services.AddScoped<IAuthorizationHandler, OwnDataAuthorizationHandler>();
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("OwnData", policy => 
                    policy.Requirements.Add(new OwnDataRequirement()));
            });

            // Build the service provider and create a scope
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();

            // Clear the database before seeding
            dbContext.Database.EnsureDeleted();

            // Use migrations to apply schema, especially for identity tables
            dbContext.Database.EnsureCreated();
        });
    }

    public void SeedDb(IServiceScope scope, DbSeeder seeder)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        seeder.SeedData(dbContext, userManager);
    }
    public IServiceScope CreateScope()
    {
        return Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    }

}