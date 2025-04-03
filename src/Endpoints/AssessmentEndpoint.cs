using Backend.Entities.Assessments;
using Backend.Entities.Assessments.DTOs;
using Backend.Mappings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data; 
using Microsoft.AspNetCore.Http.HttpResults;

namespace backend.Endpoints;

public static class AssessmentEndpoints
{
    public static RouteGroupBuilder MapAssessmentsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("assessments");

        // POST /assessments
        group.MapPost("/", async (CreateAssessmentDTO newAssessment, PrototypeDbContext db) =>
        {
            var assessment = newAssessment.ToEntity();
            db.Assessments.Add(assessment);
            await db.SaveChangesAsync();

            return Results.Created($"/assessments/{assessment.Id}", assessment.ToDTO());
        })
        .WithName("CreateAssessment")
        .WithTags("Assessments")
        .WithDescription("Creates a new assessment.")
        .Accepts<CreateAssessmentDTO>("application/json")
        .Produces<AssessmentDTO>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        // GET /assessments/{id}
        group.MapGet("/{id}", async (int id, PrototypeDbContext db) =>
        {
            var assessment = await db.Assessments.FindAsync(id);
            return assessment is null
                ? Results.NotFound()
                : Results.Ok(assessment.ToDTO());
        })
        .WithName("GetAssessment")
        .WithTags("Assessments")
        .WithDescription("Returns an assessment by id.")
        .Produces<AssessmentDTO>()
        .Produces<NotFound>(StatusCodes.Status404NotFound);

        // PUT /assessments/{id}
        group.MapPut("/{id}", async (int id, UpdateAssessmentDTO updateDTO, PrototypeDbContext db) =>
        {
            var assessment = await db.Assessments.FindAsync(id);
            if (assessment == null)
            {
                return Results.NotFound();
            }

            // Apply updates
            assessment.Body = updateDTO.Body ?? assessment.Body;
            assessment.ConflictManagementStrategy = updateDTO.ConflictManagementStrategy ?? assessment.ConflictManagementStrategy;
            assessment.Openness = updateDTO.Openness;
            assessment.Conscientiousness = updateDTO.Conscientiousness;
            assessment.Extroversion = updateDTO.Extroversion;
            assessment.Agreeableness = updateDTO.Agreeableness;
            assessment.Neuroticism = updateDTO.Neuroticism;

            await db.SaveChangesAsync();
            return Results.Ok(assessment.ToDTO());
        })
        .WithName("UpdateAssessment")
        .WithTags("Assessments")
        .WithDescription("Updates an assessment by id.")
        .Accepts<UpdateAssessmentDTO>("application/json")
        .Produces<AssessmentDTO>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // DELETE /assessments/{id}
        group.MapDelete("/{id}", async (int id, PrototypeDbContext db) =>
        {
            var assessment = await db.Assessments.FindAsync(id);
            if (assessment == null)
            {
                return Results.NotFound();
            }

            db.Assessments.Remove(assessment);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteAssessment")
        .WithTags("Assessments")
        .WithDescription("Deletes an assessment by id.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
