using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Backend.Entities.Assessments;
using Backend.Entities.Assessments.DTOs;
using Backend.Entities.Users;
using Backend.IntegrationTests.Utils;
using Backend.IntegrationTests.Utils.DbSeeders;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.IntegrationTests.Endpoints;

public class AssessmentEndpointTests : IClassFixture<backendWebApplicationFactory>
{
    private readonly backendWebApplicationFactory _factory;

    public AssessmentEndpointTests(backendWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUserAssessments_ShouldReturnOk_WhenAssessmentsExist()
    {
        var client = _factory.CreateClientWithSeed(new BaseCaseDb(), out var user);

        var response = await client.GetAsync($"/users/{user.Id}/assessments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var assessments = await response.Content.ReadFromJsonAsync<List<Assessment>>();
        Assert.NotNull(assessments);
        Assert.NotEmpty(assessments);
    }

    [Fact]
    public async Task GetAssessmentById_ShouldReturnOk_WhenAssessmentExists()
    {
        var dbSeeder = new BaseCaseDb();
        var client = _factory.CreateClientWithSeed(dbSeeder, out _);
        var assessmentId = dbSeeder.Assessments.First().Id;

        var response = await client.GetAsync($"/assessments/{assessmentId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var assessment = await response.Content.ReadFromJsonAsync<AssessmentDTO>();
        Assert.NotNull(assessment);
        Assert.Equal(assessmentId, assessment.Id);
    }


    [Fact]
    public async Task DeleteAssessment_ShouldReturnNoContent_WhenAssessmentExists()
    {
        var dbSeeder = new BaseCaseDb();
        var client = _factory.CreateClientWithSeed(dbSeeder, out _);
        var assessmentId = dbSeeder.Assessments.First().Id;

        var response = await client.DeleteAsync($"/assessments/{assessmentId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAssessment_ShouldReturnOk_WhenAssessmentExists()
    {
        var dbSeeder = new BaseCaseDb();
        var client = _factory.CreateClientWithSeed(dbSeeder, out _);
        var assessment = dbSeeder.Assessments.First();

        var updateDto = new UpdateAssessmentDTO
        {
            Body = "Updated",
            ConflictManagementStrategy = "New Strategy",
            Openness = 1,
            Conscientiousness = 2,
            Extroversion = 3,
            Agreeableness = 4,
            Neuroticism = 5
        };

        var response = await client.PutAsJsonAsync($"/assessments/{assessment.Id}", updateDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<Assessment>();
        Assert.Equal("Updated", updated!.Body);
        Assert.Equal("New Strategy", updated.ConflictManagementStrategy);
    }

    [Fact]
    public async Task GetUserAssessments_ShouldReturnNotFound_WhenNoneExist()
    {
        var client = _factory.CreateClientWithSeed(new EmptyDb(), out _);

        // Create a new scope to access services
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var seeder = new EmptyDb();

        seeder.SeedUsers(userManager);
        var user = seeder.Users["testuser"];

        var response = await client.GetAsync($"/users/{user.Id}/assessments");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}