using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IDesignationRepository : IGenericRepository<Designation>
{
    Task<bool> ExistsAsync(int designationId);
}
