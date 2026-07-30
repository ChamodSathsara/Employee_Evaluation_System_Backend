using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IAuditLogService
{
    Task<PagedResultDto<AuditLogDto>> SearchAsync(AuditLogSearchDto search);
    Task<List<AuditLogDto>> GetByUserAsync(int userId);
}
