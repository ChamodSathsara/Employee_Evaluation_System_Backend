using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IHODEvaluationRepository : IGenericRepository<HODEvaluation>
{
    Task<HODEvaluation?> GetByIdWithDetailsAsync(int hodEvaluationId);
    Task<HODEvaluation?> GetBySelfEvaluationAsync(int selfEvaluationId);
    Task ReplaceDetailsAsync(int hodEvaluationId, List<HODEvaluationDetail> newDetails);
}
