using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    // TODO: add AuditLog-specific query methods beyond basic CRUD
}
