
namespace backend.Entities.Assessments.DOTs;
// DTO for updating an assessment
public class UpdateAssessmentDTO
{
    public string? Body { get; set; }
    public string? ConflictManagementStrategy { get; set; }
    public int? Openness { get; set; }
    public int? Conscientiousness { get; set; }
    public int? Extroversion { get; set; }
    public int? Agreeableness { get; set; }
    public int? Neuroticism { get; set; }
}