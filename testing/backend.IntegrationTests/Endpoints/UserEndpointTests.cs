using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Backend;
using Backend.IntegrationTests.Utils;
using Backend.IntegrationTests.Utils.DbSeeders;
using Backend.Entities.Users.DTOs;
using Backend.Entities.Users;
using Backend.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace Backend.IntegrationTests.Endpoints
{
    public class UserEndpointTests : IClassFixture<backendWebApplicationFactory>
    {
        private readonly backendWebApplicationFactory _factory;

        public UserEndpointTests(backendWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetUser_ShouldReturnOk_WhenUserExists()
        {
            // Arrange: Create a client with a seeded database and a test user
            var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var testUser);
            
            // Act: Send a GET request to retrieve the user
            var response = await client.GetAsync($"/users/{testUser.Id}");
            
            // Assert: Check if the response is OK and the user data matches
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var user = await response.Content.ReadFromJsonAsync<UserDTO>();
            Assert.NotNull(user);
            Assert.Equal(testUser.Id, user.Id);

            // Verify Database State
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
            Assert.True(dbContext.Users.Any(u => u.Id == testUser.Id));
        }

        [Fact]
        public async Task GetUser_ShouldReturnUnauthorized_WhenUserDoesNotExist()
        {
            // Arrange: Create a client with an empty database
            var client = _factory.CreateClientWithSeed(new EmptyDb(), out _);

            // Act: Send a GET request to retrieve a non-existing user
            var response = await client.GetAsync("/users/1");

            // Assert: Check if the response is NotFound
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            // Verify Database State (should be empty)
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
            Assert.False(dbContext.Users.Any());
        }

        [Fact]
        public async Task CreateUser_ShouldReturnCreated_WhenDataIsValid()
        {
            // Arrange: Create a client with an empty database
            // and a new user object to be created
            var client = _factory.CreateClientWithSeed(new EmptyDb(), out _);

            var newUser = new CreateUserDTO
            {
                Email = "newuser@example.com",
                Password = "StrongPass123!"
            };

            // Act: Send a POST request to create the user
            var response = await client.PostAsJsonAsync("/users", newUser);

            // Assert: Check if the response is Created and the user data matches
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var createdUser = await response.Content.ReadFromJsonAsync<UserDTO>();
            Assert.NotNull(createdUser);
            Assert.Equal(newUser.Email, createdUser.Email);

            // Verify Database State
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var userInDb = await userManager.FindByEmailAsync(newUser.Email);
            Assert.NotNull(userInDb);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenDataIsInvalid()
        {
            // Arrange: Create a client with an empty database
            // and an invalid user object to be created
            var client = _factory.CreateClientWithSeed(new EmptyDb(), out _);

            var invalidUser = new CreateUserDTO
            {
                Email = "invalidemail",
                Password = "weak"
            };

            // Act: Send a POST request to create the user
            var response = await client.PostAsJsonAsync("/users", invalidUser);

            // Assert: Check if the response is BadRequest
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // Verify Database State (user should not be created)
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
            Assert.False(dbContext.Users.Any());
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnOk_WhenCredentialsAreCorrect()
        {
            // Arrange: Create a client with a seeded database and a test user
            // and a valid password update object
            var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var testUser);

            var updatePassword = new UpdateUserPasswordDTO("Password123!", "NewStrongPass123!");

            // Act: Send a PUT request to change the password
            var response = await client.PutAsJsonAsync($"/users/{testUser.Id}/change-password", updatePassword);

            // Assert: Check if the response is OK
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify Database State (password change)
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var userInDb = await userManager.FindByIdAsync(testUser.Id.ToString());
            Assert.True(await userManager.CheckPasswordAsync(userInDb, updatePassword.NewPassword));
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnBadRequest_WhenOldPasswordIsIncorrect()
        {
            // Arrange: Create a client with a seeded database and a test user
            var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var testUser);

            var updatePassword = new UpdateUserPasswordDTO("WrongPass!", "NewStrongPass123!");

            // Act: Send a PUT request to change the password
            var response = await client.PutAsJsonAsync($"/users/{testUser.Id}/change-password", updatePassword);

            // Assert: Check if the response is BadRequest
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // Verify Database State (password unchanged)
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var userInDb = await userManager.FindByIdAsync(testUser.Id.ToString());
            Assert.True(await userManager.CheckPasswordAsync(userInDb, "Password123!"));
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNoContent_WhenCredentialsAreCorrect()
        {
            // Arrange: Create a client with a seeded database and a test user
            var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var testUser);

            var deleteUser = new DeleteUserDTO
            {
                Id = testUser.Id,
                Password = "Password123!"
            };

            // Act: Send a DELETE request to delete the user
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{testUser.Id}")
            {
                Content = JsonContent.Create(deleteUser)
            };

            var response = await client.SendAsync(request);

            // Assert: Check if the response is NoContent
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify Database State (user deleted)
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
            Assert.False(dbContext.Users.Any(u => u.Id == testUser.Id));
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnBadRequest_WhenPasswordIsIncorrect()
        {
            // Arrange: Create a client with a seeded database and a test user
            var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var testUser);

            var deleteUser = new DeleteUserDTO
            {
                Id = testUser.Id,
                Password = "WrongPass!"
            };

            // Act: Send a DELETE request to delete the user
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/users/{testUser.Id}")
            {
                Content = JsonContent.Create(deleteUser)
            };

            var response = await client.SendAsync(request);

            // Assert: Check if the response is BadRequest
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            // Verify Database State (user not deleted)
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
            Assert.True(dbContext.Users.Any(u => u.Id == testUser.Id));
        }
    }
}