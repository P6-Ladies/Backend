// src\Endpoints\AgentEndpoints.cs

using backend.Data;
using backend.Entities.Agents;
using backend.Entities.Agents.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints
{
    public static class AgentEndpoints
    {
        // Register the agent-related endpoints
        public static void MapAgentEndpoints(this IEndpointRouteBuilder app)
        {
            // Create a new agent
            app.MapPost("/agents", async ([FromBody] CreateAgentDTO request, PrototypeDbContext dbContext) =>
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return Results.BadRequest("Name is required.");
                }

                // Create a new agent
                var agent = new Agent
                {
                    Name = request.Name,
                    UserId = request.UserId,
                    PromptBody = request.PromptBody,
                    AvatarId = request.AvatarId,
                    Openness = request.Openness,
                    Conscientiousness = request.Conscientiousness,
                    Extroversion = request.Extroversion,
                    Agreeableness = request.Agreeableness,
                    Neuroticism = request.Neuroticism
                };

                dbContext.Agents.Add(agent);
                await dbContext.SaveChangesAsync();

                return Results.Created($"/agents/{agent.Id}", agent);
            })
            .WithName("CreateAgent")
            .WithTags("Agents")
            .WithDescription("Creates a new agent.")
            .Accepts<CreateAgentDTO>("application/json")
            .Produces<Agent>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            // Get all agents for a specific user
            app.MapGet("/users/{userId}/agents", async (int userId, PrototypeDbContext dbContext) =>
            {
                var agents = await dbContext.Agents
                    .Where(a => a.UserId == userId)
                    .ToListAsync();

                if (agents.Count == 0)
                {
                    return Results.NotFound("No agents found for this user.");
                }

                return Results.Ok(agents);
            })
            .WithName("GetUserAgents")
            .WithTags("Agents")
            .WithDescription("Retrieves all agents for a user.")
            .Produces<List<Agent>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // Get a specific agent by its ID
            app.MapGet("/agents/{agentId}", async (int agentId, PrototypeDbContext dbContext) =>
            {
                var agent = await dbContext.Agents
                    .FirstOrDefaultAsync(a => a.Id == agentId);

                if (agent == null)
                {
                    return Results.NotFound("Agent not found.");
                }

                return Results.Ok(agent);
            })
            .WithName("GetAgentById")
            .WithTags("Agents")
            .WithDescription("Retrieves a specific agent by its ID.")
            .Produces<Agent>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // Update an existing agent
            app.MapPut("/agents/{agentId}", async (int agentId, [FromBody] UpdateAgentDTO request, PrototypeDbContext dbContext) =>
            {
                var agent = await dbContext.Agents.FindAsync(agentId);

                if (agent == null)
                {
                    return Results.NotFound("Agent not found.");
                }

                agent.Name = request.Name ?? agent.Name;
                agent.PromptBody = request.PromptBody ?? agent.PromptBody;
                agent.AvatarId = request.AvatarId ?? agent.AvatarId;
                agent.Openness = request.Openness ?? agent.Openness;
                agent.Conscientiousness = request.Conscientiousness ?? agent.Conscientiousness;
                agent.Extroversion = request.Extroversion ?? agent.Extroversion;
                agent.Agreeableness = request.Agreeableness ?? agent.Agreeableness;
                agent.Neuroticism = request.Neuroticism ?? agent.Neuroticism;

                dbContext.Agents.Update(agent);
                await dbContext.SaveChangesAsync();

                return Results.Ok(agent);
            })
            .WithName("UpdateAgent")
            .WithTags("Agents")
            .WithDescription("Updates an existing agent.")
            .Accepts<UpdateAgentDTO>("application/json")
            .Produces<Agent>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // Delete an agent by its ID
            app.MapDelete("/agents/{agentId}", async (int agentId, PrototypeDbContext dbContext) =>
            {
                var agent = await dbContext.Agents.FindAsync(agentId);

                if (agent == null)
                {
                    return Results.NotFound("Agent not found.");
                }

                dbContext.Agents.Remove(agent);
                await dbContext.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteAgent")
            .WithTags("Agents")
            .WithDescription("Deletes an agent by its ID.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
