// src\Endpoints\AssessmentEndpoints.cs

using backend.Data;
using backend.Entities.Assessments;
using backend.Entities.Assessments.DOTs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints
{
    public static class AssessmentEndpoints
    {
        // Register the assessment-related endpoints
        public static void MapAssessmentEndpoints(this IEndpointRouteBuilder app)
        {
            // Create a new assessment
            app.MapPost("/assessments", async ([FromBody] CreateAssessmentDTO request, PrototypeDbContext dbContext) =>
            {
                // Ensure the conversation exists
                var conversation = await dbContext.Conversations.FindAsync(request.ConversationId);
                if (conversation == null)
                {
                    return Results.NotFound("Conversation not found.");
                }

                // Create a new assessment
                var assessment = new Assessment
                {
                    UserId = request.UserId,
                    ConversationId = request.ConversationId,
                    Body = request.Body,
                    ConflictManagementStrategy = request.ConflictManagementStrategy,
                    Openness = request.Openness,
                    Conscientiousness = request.Conscientiousness,
                    Extroversion = request.Extroversion,
                    Agreeableness = request.Agreeableness,
                    Neuroticism = request.Neuroticism
                };

                dbContext.Assessments.Add(assessment);
                await dbContext.SaveChangesAsync();

                return Results.Created($"/assessments/{assessment.Id}", assessment);
            })
            .WithName("CreateAssessment")
            .WithTags("Assessments")
            .WithDescription("Creates a new assessment for a conversation.")
            .Accepts<CreateAssessmentDTO>("application/json")
            .Produces<Assessment>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

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
                assessment.Openness = request.Openness ?? assessment.Openness;
                assessment.Conscientiousness = request.Conscientiousness ?? assessment.Conscientiousness;
                assessment.Extroversion = request.Extroversion ?? assessment.Extroversion;
                assessment.Agreeableness = request.Agreeableness ?? assessment.Agreeableness;
                assessment.Neuroticism = request.Neuroticism ?? assessment.Neuroticism;

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