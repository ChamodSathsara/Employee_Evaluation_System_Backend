using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
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
        try
        {
            return await Context.KpiAssignments
                .Include(a=>a.Employee)

                .Include(a => a.Period)
                .Include(a => a.CreatedByUser)
                .Include(a => a.Details)
                .Include(a => a.Period)
                .Include(a => a.Details)
                .Where(a => a.EmployeeId == employeeId && a.Status == "Published")
                //.OrderByDescending(a => a.CreatedDate)
                //.AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log the exception if you have a logger
            // _logger.LogError(ex, "An error occurred while retrieving published KPI assignments for EmployeeId: {EmployeeId}", employeeId);

            throw new Exception(
                $"An error occurred while retrieving published KPI assignments for Employee ID {employeeId}.",
                ex);
        }
    }

    public async Task<List<KPIAssignment>> GetByEmployeeIdsAsync(IEnumerable<int> employeeIds)
    {
        var idSet = employeeIds as ICollection<int> ?? employeeIds.ToList();

        if (idSet.Count == 0)
        {
            return new List<KPIAssignment>();
        }

        return await Context.KpiAssignments
            .Include(a => a.Employee)
            .Include(a => a.CreatedByUser)
            .Include(a => a.Period)
            .Include(a => a.Details)
            .Where(a => idSet.Contains(a.EmployeeId))
            .OrderBy(a => a.Employee.FirstName)
            .ThenByDescending(a => a.CreatedDate)
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

    public async Task DeleteWithRelatedDataAsync(KPIAssignment assignment)
    {
        var assignmentId = assignment.AssignmentId;

        var kpiDetailIds = await Context.KpiAssignmentDetails
            .Where(d => d.AssignmentId == assignmentId)
            .Select(d => d.DetailId)
            .ToListAsync();

        var selfEvaluationIds = await Context.EmployeeEvaluations
            .Where(e => e.AssignmentId == assignmentId)
            .Select(e => e.SelfEvaluationId)
            .ToListAsync();

        var supervisorEvaluationIds = await Context.SupervisorEvaluations
            .Where(e => selfEvaluationIds.Contains(e.SelfEvaluationId))
            .Select(e => e.SupervisorEvaluationId)
            .ToListAsync();

        var hodEvaluationIds = await Context.HodEvaluations
            .Where(e => selfEvaluationIds.Contains(e.SelfEvaluationId))
            .Select(e => e.HODEvaluationId)
            .ToListAsync();

        var resultIds = await Context.EvaluationResults
            .Where(r => hodEvaluationIds.Contains(r.HODEvaluationId))
            .Select(r => r.ResultId)
            .ToListAsync();

        var achievementIds = await Context.EmployeeAchievements
            .Where(a => hodEvaluationIds.Contains(a.HODEvaluationId))
            .Select(a => a.AchievementId)
            .ToListAsync();

        // Attachments use a polymorphic table/id reference instead of a foreign key.
        // Remove matching rows explicitly so they do not become orphaned.
        var attachments = await Context.Attachments
            .Where(a =>
                (a.RelatedTable == "KPIAssignments" && a.RelatedId == assignmentId) ||
                (a.RelatedTable == "KPIAssignmentDetails" && kpiDetailIds.Contains(a.RelatedId)) ||
                (a.RelatedTable == "EmployeeEvaluations" && selfEvaluationIds.Contains(a.RelatedId)) ||
                (a.RelatedTable == "SupervisorEvaluations" && supervisorEvaluationIds.Contains(a.RelatedId)) ||
                (a.RelatedTable == "HODEvaluations" && hodEvaluationIds.Contains(a.RelatedId)) ||
                (a.RelatedTable == "EmployeeAchievements" && achievementIds.Contains(a.RelatedId)) ||
                (a.RelatedTable == "EvaluationResults" && resultIds.Contains(a.RelatedId)))
            .ToListAsync();

        var recommendations = await Context.Recommendations
            .Where(r => resultIds.Contains(r.ResultId))
            .ToListAsync();
        var managerReviews = await Context.ManagerReviews
            .Where(r => resultIds.Contains(r.ResultId))
            .ToListAsync();
        var results = await Context.EvaluationResults
            .Where(r => resultIds.Contains(r.ResultId))
            .ToListAsync();
        var competencyEvaluations = await Context.CompetencyEvaluations
            .Where(e => hodEvaluationIds.Contains(e.HODEvaluationId))
            .ToListAsync();
        var achievements = await Context.EmployeeAchievements
            .Where(a => achievementIds.Contains(a.AchievementId))
            .ToListAsync();
        var hodDetails = await Context.HodEvaluationDetails
            .Where(d => hodEvaluationIds.Contains(d.HODEvaluationId))
            .ToListAsync();
        var supervisorDetails = await Context.SupervisorEvaluationDetails
            .Where(d => supervisorEvaluationIds.Contains(d.SupervisorEvaluationId))
            .ToListAsync();
        var employeeDetails = await Context.EmployeeEvaluationDetails
            .Where(d => selfEvaluationIds.Contains(d.SelfEvaluationId))
            .ToListAsync();
        var hodEvaluations = await Context.HodEvaluations
            .Where(e => hodEvaluationIds.Contains(e.HODEvaluationId))
            .ToListAsync();
        var supervisorEvaluations = await Context.SupervisorEvaluations
            .Where(e => supervisorEvaluationIds.Contains(e.SupervisorEvaluationId))
            .ToListAsync();
        var employeeEvaluations = await Context.EmployeeEvaluations
            .Where(e => selfEvaluationIds.Contains(e.SelfEvaluationId))
            .ToListAsync();
        var kpiDetails = await Context.KpiAssignmentDetails
            .Where(d => kpiDetailIds.Contains(d.DetailId))
            .ToListAsync();

        Context.Attachments.RemoveRange(attachments);
        Context.Recommendations.RemoveRange(recommendations);
        Context.ManagerReviews.RemoveRange(managerReviews);
        Context.EvaluationResults.RemoveRange(results);
        Context.CompetencyEvaluations.RemoveRange(competencyEvaluations);
        Context.EmployeeAchievements.RemoveRange(achievements);
        Context.HodEvaluationDetails.RemoveRange(hodDetails);
        Context.SupervisorEvaluationDetails.RemoveRange(supervisorDetails);
        Context.EmployeeEvaluationDetails.RemoveRange(employeeDetails);
        Context.HodEvaluations.RemoveRange(hodEvaluations);
        Context.SupervisorEvaluations.RemoveRange(supervisorEvaluations);
        Context.EmployeeEvaluations.RemoveRange(employeeEvaluations);
        Context.KpiAssignmentDetails.RemoveRange(kpiDetails);
        Context.KpiAssignments.Remove(assignment);
    }
}
