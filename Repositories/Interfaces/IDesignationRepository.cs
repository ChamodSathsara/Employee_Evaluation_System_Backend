using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories.Interfaces;

public interface IDesignationRepository : IGenericRepository<Designation>
{
    Task<bool> ExistsAsync(int designationId);
}
