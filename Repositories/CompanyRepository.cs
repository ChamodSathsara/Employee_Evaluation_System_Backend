using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
{
    public CompanyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<bool> ExistsAsync(int companyId) =>
        Context.Companies.AnyAsync(c => c.CompanyId == companyId);

    public Task<bool> ExistsByNameAsync(string companyName) =>
        Context.Companies.AnyAsync(c => c.CompanyName.ToLower() == companyName.ToLower());

    public Task<int> CountUsersAsync(int companyId) =>
        Context.Users.CountAsync(u => u.CompanyId == companyId);
}
