namespace EmployeeEvaluationSystem.Models;

public class Competency
{
    public int CompetencyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<CompetencyEvaluation> CompetencyEvaluations { get; set; } = new List<CompetencyEvaluation>();
}
