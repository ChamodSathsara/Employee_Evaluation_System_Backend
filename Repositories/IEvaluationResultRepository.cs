using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IEvaluationResultRepository : IGenericRepository<EvaluationResult>
{
    Task<EvaluationResult?> GetByHodEvaluationAsync(int hodEvaluationId);
    Task<EvaluationResult?> GetByIdWithDetailsAsync(int resultId);
    Task<List<EvaluationResult>> GetByEmployeeAsync(int employeeId);
    Task<List<EvaluationResult>> GetByPeriodAsync(int periodId);

    /// <summary>Results that are Processed but have no ManagerReview decision yet, restricted to the given Manager's organizational subtree.</summary>
    Task<List<EvaluationResult>> GetPendingManagerReviewAsync(IReadOnlySet<int> subordinateEmployeeIds);
}
