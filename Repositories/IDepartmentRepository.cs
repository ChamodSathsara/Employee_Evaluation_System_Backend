using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<bool> ExistsAsync(int departmentId);
}
