using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IEmployeeEvaluationRepository : IGenericRepository<EmployeeEvaluation>
{
    Task<EmployeeEvaluation?> GetByAssignmentAndEmployeeAsync(int assignmentId, int employeeId);
    Task<EmployeeEvaluation?> GetByIdWithDetailsAsync(int selfEvaluationId);
    Task<List<EmployeeEvaluation>> GetHistoryForEmployeeAsync(int employeeId);

    /// <summary>Self-evaluations submitted by this supervisor's direct reports and not yet fully supervisor-reviewed.</summary>
    Task<List<EmployeeEvaluation>> GetPendingForSupervisorAsync(int supervisorId);

    /// <summary>
    /// Self-evaluations ready for HOD review: either the employee reports directly to this HOD
    /// (no Supervisor layer), or the employee's Supervisor reports to this HOD and has already
    /// submitted their review. Excludes evaluations the HOD has already submitted.
    /// </summary>
    Task<List<EmployeeEvaluation>> GetPendingForHodAsync(int hodId);
}
