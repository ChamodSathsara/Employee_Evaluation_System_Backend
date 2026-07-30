using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface ISupervisorEvaluationRepository : IGenericRepository<SupervisorEvaluation>
{
    Task<SupervisorEvaluation?> GetByIdWithDetailsAsync(int supervisorEvaluationId);
    Task<SupervisorEvaluation?> GetBySelfEvaluationAsync(int selfEvaluationId);
    Task ReplaceDetailsAsync(int supervisorEvaluationId, List<SupervisorEvaluationDetail> newDetails);
}
