using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string roleName);
    Task<bool> ExistsByNameAsync(string roleName, int? excludeRoleId = null);
    Task<int> CountUsersAsync(int roleId);
}
