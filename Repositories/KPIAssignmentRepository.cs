using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class KPIAssignmentRepository : GenericRepository<KPIAssignment>, IKPIAssignmentRepository
{
    public KPIAssignmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<KPIAssignment?> GetByIdWithDetailsAsync(int assignmentId)
    {
        return await Context.KpiAssignments
            .Include(a => a.Employee).ThenInclude(e => e.Role)
            .Include(a => a.CreatedByUser)
            .Include(a => a.Period)
            .Include(a => a.Details)
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
    }

    public async Task<KPIAssignment?> GetByEmployeeAndPeriodAsync(int employeeId, int periodId)
    {
        return await Context.KpiAssignments
            .Include(a => a.Details)
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.PeriodId == periodId);
    }

    public async Task<List<KPIAssignment>> GetByEmployeeAsync(int employeeId)
    {
        return await Context.KpiAssignments
            .Include(a => a.Employee)
            .Include(a => a.CreatedByUser)
            .Include(a => a.Period)
            .Include(a => a.Details)
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.CreatedDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<KPIAssignment>> GetByPeriodAsync(int periodId, IReadOnlySet<int>? restrictToEmployeeIds)
    {
        var query = Context.KpiAssignments
            .Include(a => a.Employee)
            .Include(a => a.CreatedByUser)
            .Include(a => a.Period)
            .Include(a => a.Details)
            .Where(a => a.PeriodId == periodId);

        // A null restriction set means "no restriction" (Admin, full visibility).
        if (restrictToEmployeeIds is not null)
        {
            query = query.Where(a => restrictToEmployeeIds.Contains(a.EmployeeId));
        }

        return await query
            .OrderBy(a => a.Employee.FirstName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<KPIAssignment>> GetMyPublishedAsync(int employeeId)
    {
        return await Context.KpiAssignments
            .Include(a => a.Period)
            .Include(a => a.Details)
            .Where(a => a.EmployeeId == employeeId && a.Status == "Published")
            .OrderByDescending(a => a.CreatedDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task ReplaceDetailsAsync(int assignmentId, List<KPIAssignmentDetail> newDetails)
    {
        var existing = await Context.KpiAssignmentDetails
            .Where(d => d.AssignmentId == assignmentId)
            .ToListAsync();

        Context.KpiAssignmentDetails.RemoveRange(existing);

        foreach (var detail in newDetails)
        {
            detail.AssignmentId = assignmentId;
        }

        await Context.KpiAssignmentDetails.AddRangeAsync(newDetails);
    }

    public async Task<bool> ExistsForUserAsync(int userId)
    {
        return await Context.KpiAssignments.AnyAsync(a => a.EmployeeId == userId || a.CreatedBy == userId);
    }

    public async Task<bool> HasEvaluationsAsync(int assignmentId)
    {
        return await Context.EmployeeEvaluations.AnyAsync(e => e.AssignmentId == assignmentId);
    }
}
