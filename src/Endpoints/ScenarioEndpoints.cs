// src\Endpoints\ScenarioEndpoints.cs

using Backend.Data;
using Backend.Entities.Scenarios;
using Backend.Entities.Scenarios.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Endpoints
{
    public static class ScenarioEndpoints
    {
        // Register the scenario-related endpoints
        public static void MapScenarioEndpoints(this IEndpointRouteBuilder app)
        {
            // Create a new scenario
            app.MapPost("/scenarios", async ([FromBody] CreateScenarioDTO request, PrototypeDbContext dbContext) =>
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return Results.BadRequest("Name is required.");
                }

                // Create a new scenario
                var scenario = new Scenario
                {
                    Name = request.Name,
                    UserId = request.UserId,
                    SettingPrompt = request.SettingPrompt,
                    ConflictPrompt = request.ConflictPrompt,
                    AdditionalPrompt = request.AdditionalPrompt,
                    CreatedAt = DateTime.UtcNow 
                };

                dbContext.Scenarios.Add(scenario);
                await dbContext.SaveChangesAsync();

                return Results.Created($"/scenarios/{scenario.Id}", scenario);
            })
            .WithName("CreateScenario")
            .WithTags("Scenarios")
            .WithDescription("Creates a new scenario.")
            .Accepts<CreateScenarioDTO>("application/json")
            .Produces<Scenario>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            // Get all scenarios for a specific user
            app.MapGet("/users/{userId}/scenarios", async (int userId, PrototypeDbContext dbContext) =>
            {
                var scenarios = await dbContext.Scenarios
                    .Where(s => s.UserId == userId)
                    .ToListAsync();

                if (scenarios.Count == 0)
                {
                    return Results.NotFound("No scenarios found for this user.");
                }

                return Results.Ok(scenarios);
            })
            .WithName("GetUserScenarios")
            .WithTags("Scenarios")
            .WithDescription("Retrieves all scenarios for a user.")
            .Produces<List<Scenario>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // Get a specific scenario by its ID
            app.MapGet("/scenarios/{scenarioId}", async (int scenarioId, PrototypeDbContext dbContext) =>
            {
                var scenario = await dbContext.Scenarios
                    .FirstOrDefaultAsync(s => s.Id == scenarioId);

                if (scenario == null)
                {
                    return Results.NotFound("Scenario not found.");
                }

                return Results.Ok(scenario);
            })
            .WithName("GetScenarioById")
            .WithTags("Scenarios")
            .WithDescription("Retrieves a specific scenario by its ID.")
            .Produces<Scenario>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // Update an existing scenario
            app.MapPut("/scenarios/{scenarioId}", async (int scenarioId, [FromBody] UpdateScenarioDTO request, PrototypeDbContext dbContext) =>
            {
                var scenario = await dbContext.Scenarios.FindAsync(scenarioId);

                if (scenario == null)
                {
                    return Results.NotFound("Scenario not found.");
                }

                scenario.Name = request.Name ?? scenario.Name;
                scenario.SettingPrompt = request.SettingPrompt ?? scenario.SettingPrompt;
                scenario.ConflictPrompt = request.ConflictPrompt ?? scenario.ConflictPrompt;
                scenario.AdditionalPrompt = request.AdditionalPrompt ?? scenario.AdditionalPrompt;

                dbContext.Scenarios.Update(scenario);
                await dbContext.SaveChangesAsync();

                return Results.Ok(scenario);
            })
            .WithName("UpdateScenario")
            .WithTags("Scenarios")
            .WithDescription("Updates an existing scenario.")
            .Accepts<UpdateScenarioDTO>("application/json")
            .Produces<Scenario>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // Delete a scenario
            app.MapDelete("/scenarios/{scenarioId}", async (int scenarioId, PrototypeDbContext dbContext) =>
            {
                var scenario = await dbContext.Scenarios.FindAsync(scenarioId);

                if (scenario == null)
                {
                    return Results.NotFound("Scenario not found.");
                }

                dbContext.Scenarios.Remove(scenario);
                await dbContext.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteScenario")
            .WithTags("Scenarios")
            .WithDescription("Deletes a specific scenario.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}