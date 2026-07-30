using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class DesignationRepository : GenericRepository<Designation>, IDesignationRepository
{
    public DesignationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsAsync(int designationId)
    {
        return await Context.Designations.AnyAsync(d => d.DesignationId == designationId);
    }

    public async Task<bool> ExistsByNameAsync(string designationName, int? excludeDesignationId = null)
    {
        var query = Context.Designations.Where(d => d.DesignationName.ToLower() == designationName.ToLower());
        if (excludeDesignationId.HasValue)
        {
            query = query.Where(d => d.DesignationId != excludeDesignationId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<int> CountUsersAsync(int designationId)
    {
        return await Context.Users.CountAsync(u => u.DesignationId == designationId);
    }
}
