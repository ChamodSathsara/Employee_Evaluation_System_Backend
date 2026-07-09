using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories.Interfaces;

public interface IRoleRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string roleName);
}
