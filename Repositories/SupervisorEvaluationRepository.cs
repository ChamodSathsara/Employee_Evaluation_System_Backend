using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class SupervisorEvaluationRepository : GenericRepository<SupervisorEvaluation>, ISupervisorEvaluationRepository
{
    public SupervisorEvaluationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<SupervisorEvaluation?> GetByIdWithDetailsAsync(int supervisorEvaluationId)
    {
        return await Context.SupervisorEvaluations
            .Include(s => s.Supervisor)
            .Include(s => s.SelfEvaluation).ThenInclude(e => e.Employee)
            .Include(s => s.SelfEvaluation).ThenInclude(e => e.Details)
            .Include(s => s.Details).ThenInclude(d => d.KpiAssignmentDetail)
            .FirstOrDefaultAsync(s => s.SupervisorEvaluationId == supervisorEvaluationId);
    }

    public async Task<SupervisorEvaluation?> GetBySelfEvaluationAsync(int selfEvaluationId)
    {
        return await Context.SupervisorEvaluations
            .Include(s => s.Details)
            .FirstOrDefaultAsync(s => s.SelfEvaluationId == selfEvaluationId);
    }

    public async Task ReplaceDetailsAsync(int supervisorEvaluationId, List<SupervisorEvaluationDetail> newDetails)
    {
        var existing = await Context.SupervisorEvaluationDetails
            .Where(d => d.SupervisorEvaluationId == supervisorEvaluationId)
            .ToListAsync();

        Context.SupervisorEvaluationDetails.RemoveRange(existing);

        foreach (var detail in newDetails)
        {
            detail.SupervisorEvaluationId = supervisorEvaluationId;
        }

        await Context.SupervisorEvaluationDetails.AddRangeAsync(newDetails);
    }
}
