using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IEvaluationPeriodRepository : IGenericRepository<EvaluationPeriod>
{
    Task<EvaluationPeriod?> GetActiveAsync();
    Task<List<EvaluationPeriod>> GetAllOrderedAsync();
    Task<bool> ExistsByNameAsync(string name, int? excludePeriodId = null);

    /// <summary>Sets every currently-Active period (other than <paramref name="exceptPeriodId"/>) to "Inactive". Used when activating a new period, since only one period should be Active at a time.</summary>
    Task DeactivateAllExceptAsync(int exceptPeriodId);

    Task<bool> HasKpiAssignmentsAsync(int periodId);
}
