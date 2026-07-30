using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface ICompetencyEvaluationRepository : IGenericRepository<CompetencyEvaluation>
{
    Task<List<CompetencyEvaluation>> GetByHodEvaluationAsync(int hodEvaluationId);
    Task<CompetencyEvaluation?> GetByHodEvaluationAndCompetencyAsync(int hodEvaluationId, int competencyId);
}
