using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IDepartmentRepository : IGenericRepository<Department>
{
    Task<bool> ExistsAsync(int departmentId);
    Task<bool> ExistsByNameAsync(string departmentName, int? excludeDepartmentId = null);
    Task<int> CountUsersAsync(int departmentId);
    Task<String> DepartmentNameById(int departmentId);
}
