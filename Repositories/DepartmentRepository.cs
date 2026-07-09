using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories.Interfaces;
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
}
