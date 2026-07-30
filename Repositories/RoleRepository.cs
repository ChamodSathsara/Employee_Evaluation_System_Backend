using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await Context.Roles.FirstOrDefaultAsync(r => r.RoleName.ToLower() == roleName.ToLower());
    }

    public async Task<bool> ExistsByNameAsync(string roleName, int? excludeRoleId = null)
    {
        var query = Context.Roles.Where(r => r.RoleName.ToLower() == roleName.ToLower());
        if (excludeRoleId.HasValue)
        {
            query = query.Where(r => r.RoleId != excludeRoleId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<int> CountUsersAsync(int roleId)
    {
        return await Context.Users.CountAsync(u => u.RoleId == roleId);
    }
}
