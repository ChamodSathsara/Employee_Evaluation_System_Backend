using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Read access to the immutable audit trail (Login, Create, Update, Delete, Evaluation
/// Submit, Manager Approval) written by every other module in the system. Admin-only,
/// enforced at the controller level.
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public AuditLogService(IAuditLogRepository auditLogRepository, IMapper mapper)
    {
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<AuditLogDto>> SearchAsync(AuditLogSearchDto search)
    {
        var pageNumber = search.PageNumber < 1 ? 1 : search.PageNumber;
        var pageSize = search.PageSize is < 1 or > 200 ? 50 : search.PageSize;

        var (items, totalCount) = await _auditLogRepository.SearchAsync(
            search.UserId, search.TableName, search.Action, search.FromDate, search.ToDate, pageNumber, pageSize);

        return new PagedResultDto<AuditLogDto>
        {
            Items = _mapper.Map<List<AuditLogDto>>(items),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<List<AuditLogDto>> GetByUserAsync(int userId)
    {
        var logs = await _auditLogRepository.GetByUserAsync(userId);
        return _mapper.Map<List<AuditLogDto>>(logs);
    }
}
