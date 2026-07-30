using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsAsync(int departmentId)
    {
        return await Context.Departments.AnyAsync(d => d.DepartmentId == departmentId);
    }

    public async Task<bool> ExistsByNameAsync(string departmentName, int? excludeDepartmentId = null)
    {
        var query = Context.Departments.Where(d => d.DepartmentName.ToLower() == departmentName.ToLower());
        if (excludeDepartmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId != excludeDepartmentId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<int> CountUsersAsync(int departmentId)
    {
        return await Context.Users.CountAsync(u => u.DepartmentId == departmentId);
    }

    public async Task<string> DepartmentNameById(int departmentId)
    {
        var department = await Context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
        if (department == null)
        {
            throw new KeyNotFoundException($"Department with ID {departmentId} not found.");
        }
        return department.DepartmentName;
    }
}
