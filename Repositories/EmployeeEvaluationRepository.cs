using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class EmployeeEvaluationRepository : GenericRepository<EmployeeEvaluation>, IEmployeeEvaluationRepository
{
    public EmployeeEvaluationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<EmployeeEvaluation?> GetByAssignmentAndEmployeeAsync(int assignmentId, int employeeId)
    {
        return await Context.EmployeeEvaluations
            .Include(e => e.Details)
            .FirstOrDefaultAsync(e => e.AssignmentId == assignmentId && e.EmployeeId == employeeId);
    }

    public async Task<EmployeeEvaluation?> GetByIdWithDetailsAsync(int selfEvaluationId)
    {
        return await Context.EmployeeEvaluations
            .Include(e => e.Employee)
            .Include(e => e.Assignment).ThenInclude(a => a.Period)
            .Include(e => e.Details).ThenInclude(d => d.KpiAssignmentDetail)
            .FirstOrDefaultAsync(e => e.SelfEvaluationId == selfEvaluationId);
    }

    public async Task<List<EmployeeEvaluation>> GetHistoryForEmployeeAsync(int employeeId)
    {
        return await Context.EmployeeEvaluations
            .Include(e => e.Employee)
            .Include(e => e.Assignment).ThenInclude(a => a.Period)
            .Include(e => e.Details).ThenInclude(d => d.KpiAssignmentDetail)
            .Where(e => e.EmployeeId == employeeId)
            .OrderByDescending(e => e.Assignment.Period.StartDate)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<List<EmployeeEvaluation>> GetPendingForSupervisorAsync(int supervisorId)
    {
        return await Context.EmployeeEvaluations
            .Include(e => e.Employee)
            .Include(e => e.Assignment).ThenInclude(a => a.Period)
            .Where(e => e.Status == "Submitted" && e.Employee.ReportedSupervisorId == supervisorId)
            .Where(e => !e.SupervisorEvaluations.Any(se => se.Status == "Submitted"))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<List<EmployeeEvaluation>> GetPendingForHodAsync(int hodId)
    {
        return await Context.EmployeeEvaluations
            .Include(e => e.Employee).ThenInclude(emp => emp.ReportedSupervisor)
            .Include(e => e.Assignment).ThenInclude(a => a.Period)
            .Where(e => e.Status == "Submitted")
            .Where(e =>
                e.Employee.ReportedHodId == hodId ||
                (e.Employee.ReportedSupervisor != null && e.Employee.ReportedSupervisor.ReportedHodId == hodId &&
                 e.SupervisorEvaluations.Any(se => se.Status == "Submitted")))
            .Where(e => !e.HodEvaluations.Any(he => he.Status == "Submitted"))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }
}
