using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories.Interfaces;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<bool> ExistsAsync(int departmentId);
}
