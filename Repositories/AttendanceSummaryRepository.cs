using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class AttendanceSummaryRepository : GenericRepository<AttendanceSummary>, IAttendanceSummaryRepository
{
    public AttendanceSummaryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<AttendanceSummary?> GetByEmployeeAndPeriodAsync(int employeeId, int periodId)
    {
        return await Context.AttendanceSummaries
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.PeriodId == periodId);
    }

    public async Task<List<AttendanceSummary>> GetByEmployeeAsync(int employeeId)
    {
        return await Context.AttendanceSummaries
            .Include(a => a.Period)
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.Period.StartDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<AttendanceSummary>> GetByPeriodAsync(int periodId)
    {
        return await Context.AttendanceSummaries
            .Include(a => a.Employee)
            .Where(a => a.PeriodId == periodId)
            .OrderBy(a => a.Employee.FirstName)
            .AsNoTracking()
            .ToListAsync();
    }
}
