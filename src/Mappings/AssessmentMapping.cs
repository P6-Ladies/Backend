using Backend.Entities.Assessments;
using Backend.Entities.Assessments.DTOs;

namespace Backend.Mappings;

public static class AssessmentMappings
{
    public static AssessmentDTO ToDTO(this Assessment entity)
    {
        return new AssessmentDTO
        {
            Id = entity.Id,
            UserId = entity.UserId,
            ConversationId = entity.ConversationId,
            Body = entity.Body,
            ConflictManagementStrategy = entity.ConflictManagementStrategy,
            Openness = entity.Openness,
            Conscientiousness = entity.Conscientiousness,
            Extroversion = entity.Extroversion,
            Agreeableness = entity.Agreeableness,
            Neuroticism = entity.Neuroticism
        };
    }

    public static Assessment ToEntity(this CreateAssessmentDTO dto)
    {
        return new Assessment
        {
            UserId = dto.UserId,
            ConversationId = dto.ConversationId,
            Body = dto.Body,
            ConflictManagementStrategy = dto.ConflictManagementStrategy,
            Openness = dto.Openness,
            Conscientiousness = dto.Conscientiousness,
            Extroversion = dto.Extroversion,
            Agreeableness = dto.Agreeableness,
            Neuroticism = dto.Neuroticism
        };
    }
}
