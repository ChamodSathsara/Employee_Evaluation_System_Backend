using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories.Interfaces;
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
}
