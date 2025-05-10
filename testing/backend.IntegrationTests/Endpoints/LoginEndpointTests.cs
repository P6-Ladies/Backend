using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using backend.IntegrationTests.Utils;
using backend.IntegrationTests.Utils.DbSeeders;
using backend.Entities.Users.DTOs;
using backend.Entities.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace backend.IntegrationTests.Endpoints;

public class LoginEndpointTests : IClassFixture<backendWebApplicationFactory>
{
    private readonly backendWebApplicationFactory _factory;

    public LoginEndpointTests(backendWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var testUser);

        var loginDto = new LoginDTO
        {
            Email = testUser.Email,
            Password = "Password123!"
        };

        var response = await client.PostAsJsonAsync("/login", loginDto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.True(json!.ContainsKey("Token"));
        Assert.False(string.IsNullOrEmpty(json["Token"]));
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        var client = _factory.CreateClientWithSeed(new EmptyDb(), out _);

        var loginDto = new LoginDTO
        {
            Email = "nonexistent@example.com",
            Password = "DoesNotMatter"
        };

        var response = await client.PostAsJsonAsync("/login", loginDto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordIsIncorrect()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var testUser);

        var loginDto = new LoginDTO
        {
            Email = testUser.Email,
            Password = "WrongPassword!"
        };

        var response = await client.PostAsJsonAsync("/login", loginDto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}