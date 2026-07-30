using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Audit log endpoints. Admin only.</summary>
[Route("api/audit-log")]
[Authorize(Roles = RoleNames.Admin)]
public class AuditLogsController : BaseApiController
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>Searches the global audit log with optional filters and paging.</summary>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] AuditLogSearchDto search)
    {
        var result = await _auditLogService.SearchAsync(search);
        return Ok(ApiResponse<PagedResultDto<AuditLogDto>>.SuccessResponse(result));
    }

    /// <summary>Returns the full audit history for a single user.</summary>
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var result = await _auditLogService.GetByUserAsync(userId);
        return Ok(ApiResponse<List<AuditLogDto>>.SuccessResponse(result));
    }
}
