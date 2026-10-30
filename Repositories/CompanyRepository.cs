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

    public async Task<bool> ExistsByNameAsync(string companyName, int? excludeCompanyId = null)
    {
        var query = Context.Companies.Where(c => c.CompanyName.ToLower() == companyName.ToLower());
        if (excludeCompanyId.HasValue)
        {
            query = query.Where(c => c.CompanyId != excludeCompanyId.Value);
        }

        return await query.AnyAsync();
    }

    public Task<int> CountUsersAsync(int companyId) =>
        Context.Users.CountAsync(u => u.CompanyId == companyId);
}
