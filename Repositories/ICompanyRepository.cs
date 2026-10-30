using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface ICompanyRepository : IGenericRepository<Company>
{
    Task<bool> ExistsAsync(int companyId);
    Task<bool> ExistsByNameAsync(string companyName);
    Task<int> CountUsersAsync(int companyId);
}
