using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IKPIAssignmentRepository : IGenericRepository<KPIAssignment>
{
    Task<KPIAssignment?> GetByIdWithDetailsAsync(int assignmentId);

    Task<KPIAssignment?> GetByEmployeeAndPeriodAsync(int employeeId, int periodId);

    Task<List<KPIAssignment>> GetByEmployeeAsync(int employeeId);

    Task<List<KPIAssignment>> GetByPeriodAsync(int periodId, IReadOnlySet<int>? restrictToEmployeeIds);

    Task<List<KPIAssignment>> GetMyPublishedAsync(int employeeId);

    /// <summary>Removes all existing detail rows for the assignment and inserts the replacement set (used by Update).</summary>
    Task ReplaceDetailsAsync(int assignmentId, List<KPIAssignmentDetail> newDetails);

    /// <summary>True if the user appears as Employee or CreatedBy on any KPI assignment (used to block hard-deleting Users with dependent records).</summary>
    Task<bool> ExistsForUserAsync(int userId);

    Task<bool> HasEvaluationsAsync(int assignmentId);
}
