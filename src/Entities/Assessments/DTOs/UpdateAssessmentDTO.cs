
namespace backend.Entities.Assessments.DTOs;
// DTO for updating an assessment
public class UpdateAssessmentDTO
{
    public required string Body { get; set; }
    public required string ConflictManagementStrategy { get; set; }
    public int Openness { get; set; }
    public int Conscientiousness { get; set; }
    public int Extroversion { get; set; }
    public int Agreeableness { get; set; }
    public int Neuroticism { get; set; }
}
