using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IDesignationRepository : IGenericRepository<Designation>
{
    Task<bool> ExistsAsync(int designationId);
    Task<bool> ExistsByNameAsync(string designationName, int? excludeDesignationId = null);
    Task<int> CountUsersAsync(int designationId);
}
