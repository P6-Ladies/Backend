// src\Endpoints\AssessmentEndpoints.cs

using backend.Data;
using backend.Entities.Assessments;
using backend.Entities.Assessments.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints
{
    public static class AssessmentEndpoints
    {
        // Register the assessment-related endpoints
        public static void MapAssessmentEndpoints(this IEndpointRouteBuilder app)
        {
            // Create a new assessment by Python microservice
            app.MapPost("/assessments", async ([FromServices] IHttpClientFactory http, [FromBody] CreateAssessmentDTO request, PrototypeDbContext dbContext) =>
            {
                // 1. Verify conversation exists
                var conversation = await dbContext.Conversations
                    .FirstOrDefaultAsync(c => c.Id == request.ConversationId);

                if (conversation is null)
                    return Results.NotFound("Conversation not found.");

                var messages = await dbContext.Messages.Where(m => m.ConversationId == request.ConversationId).ToListAsync();

                // 2. Build the raw text payload
                var convoText = string.Join("\n",
                    messages
                            .OrderBy(m => m.ReceivedAt)
                            .Select(m => $"{(m.UserSent ? "User" : "Agent")}: {m.Body}")
                );

                // 3. Call the python /assess endpoint
                var client = http.CreateClient("HF");
                var response = await client.PostAsJsonAsync("/assess", new { conversation = convoText });

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    return Results.Problem(
                        detail: $"Assessment service error: {err}",
                        statusCode: (int)response.StatusCode);
                }

                // 4. Deserialize into our AssessmentDTO
                var dto = await response.Content.ReadFromJsonAsync<AssessmentResultDTO>();
                if (dto is null)
                    return Results.Problem("Invalid response from assessment service.");

                // 5. Map & persist
                var assessment = new Assessment
                {
                    UserId                        = request.UserId,
                    ConversationId                = request.ConversationId,
                    Body                          = dto.Body,
                    ConflictManagementStrategy    = dto.ConflictManagementStrategy,
                    Openness                      = dto.Openness,
                    Conscientiousness             = dto.Conscientiousness,
                    Extroversion                  = dto.Extroversion,
                    Agreeableness                 = dto.Agreeableness,
                    Neuroticism                   = dto.Neuroticism
                };

                dbContext.Assessments.Add(assessment);
                await dbContext.SaveChangesAsync();

                return Results.Created($"/assessments/{assessment.Id}", assessment);
            })
            .WithName("CreateAssessment")
            .WithTags("Assessments")
            .WithDescription("Creates a new assessment by invoking the Python microservice.")
            .Accepts<CreateAssessmentDTO>("application/json")
            .Produces<Assessment>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status502BadGateway);

            // Get all assessments for a user
            app.MapGet("/users/{userId}/assessments", async (int userId, PrototypeDbContext dbContext) =>
            {
                var assessments = await dbContext.Assessments
                    .Include(a => a.Conversation)
                    .Where(a => a.UserId == userId)
                    .ToListAsync();

                if (assessments.Count == 0)
                {
                    return Results.NotFound("No assessments found for this user.");
                }

                return Results.Ok(assessments);
            })
            .WithName("GetUserAssessments")
            .WithTags("Assessments")
            .WithDescription("Retrieves all assessments for a user.")
            .Produces<List<Assessment>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // Get a specific assessment by ID
            app.MapGet("/assessments/{assessmentId}", async (int assessmentId, PrototypeDbContext dbContext) =>
            {
                var assessment = await dbContext.Assessments
                    .Include(a => a.Conversation)
                    .FirstOrDefaultAsync(a => a.Id == assessmentId);

                if (assessment == null)
                {
                    return Results.NotFound("Assessment not found.");
                }

                return Results.Ok(assessment);
            })
            .WithName("GetAssessmentById")
            .WithTags("Assessments")
            .WithDescription("Retrieves a specific assessment by its ID.")
            .Produces<Assessment>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // Update an existing assessment
            app.MapPut("/assessments/{assessmentId}", async (int assessmentId, [FromBody] UpdateAssessmentDTO request, PrototypeDbContext dbContext) =>
            {
                var assessment = await dbContext.Assessments.FindAsync(assessmentId);

                if (assessment == null)
                {
                    return Results.NotFound("Assessment not found.");
                }

                // Update properties
                assessment.Body = request.Body ?? assessment.Body;
                assessment.ConflictManagementStrategy = request.ConflictManagementStrategy ?? assessment.ConflictManagementStrategy;
                assessment.Openness = request.Openness = (int)assessment.Openness;
                assessment.Conscientiousness = request.Conscientiousness = (int)assessment.Conscientiousness;
                assessment.Extroversion = request.Extroversion = (int)assessment.Extroversion;
                assessment.Agreeableness = request.Agreeableness = (int)assessment.Agreeableness;
                assessment.Neuroticism = request.Neuroticism = (int)assessment.Neuroticism;

                dbContext.Assessments.Update(assessment);
                await dbContext.SaveChangesAsync();

                return Results.Ok(assessment);
            })
            .WithName("UpdateAssessment")
            .WithTags("Assessments")
            .WithDescription("Updates an existing assessment.")
            .Accepts<UpdateAssessmentDTO>("application/json")
            .Produces<Assessment>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // Delete an assessment
            app.MapDelete("/assessments/{assessmentId}", async (int assessmentId, PrototypeDbContext dbContext) =>
            {
                var assessment = await dbContext.Assessments.FindAsync(assessmentId);

                if (assessment == null)
                {
                    return Results.NotFound("Assessment not found.");
                }

                dbContext.Assessments.Remove(assessment);
                await dbContext.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithName("DeleteAssessment")
            .WithTags("Assessments")
            .WithDescription("Deletes a specific assessment.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}