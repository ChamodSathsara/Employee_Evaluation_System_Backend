using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<(List<AuditLog> Items, int TotalCount)> SearchAsync(
        int? userId, string? tableName, string? action, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize);

    Task<List<AuditLog>> GetByUserAsync(int userId);
}
