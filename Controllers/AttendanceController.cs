using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Attendance & Leave Summary endpoints (SRS Step 11). Read: Admin, HOD, or the employee themself. Write: Admin/HOD.</summary>
[Route("api/attendance")]
[Authorize]
public class AttendanceController : BaseApiController
{
    private readonly IAttendanceService _attendanceService;
    private readonly ICurrentUserService _currentUserService;

    public AttendanceController(IAttendanceService attendanceService, ICurrentUserService currentUserService)
    {
        _attendanceService = attendanceService;
        _currentUserService = currentUserService;
    }

    /// <summary>Lists attendance summary records for the given employee, across all periods.</summary>
    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        var (callerId, callerRole) = RequireUserIdAndRole();
        var result = await _attendanceService.GetByEmployeeAsync(callerId, callerRole, employeeId);
        return Ok(ApiResponse<List<AttendanceSummaryDto>>.SuccessResponse(result));
    }

    /// <summary>Creates or updates the attendance summary for an employee/period, recalculating the penalty.</summary>
    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.HOD},{RoleNames.Manager}")]
    public async Task<IActionResult> Upsert([FromBody] UpsertAttendanceSummaryDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var callerId = RequireUserId();
        var result = await _attendanceService.UpsertAsync(callerId, request);
        return Ok(ApiResponse<AttendanceSummaryDto>.SuccessResponse(result, "Attendance summary saved."));
    }

    /// <summary>Updates an existing attendance summary record by id.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.HOD}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertAttendanceSummaryDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var callerId = RequireUserId();
        var result = await _attendanceService.UpdateAsync(callerId, id, request);
        return Ok(ApiResponse<AttendanceSummaryDto>.SuccessResponse(result, "Attendance summary updated."));
    }

    /// <summary>Bulk-imports attendance rows for a period from an external file/source.</summary>
    [HttpPost("import")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.HOD}")]
    public async Task<IActionResult> Import([FromBody] ImportAttendanceDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var callerId = RequireUserId();
        var count = await _attendanceService.ImportAsync(callerId, request);
        return Ok(ApiResponse<int>.SuccessResponse(count, $"{count} attendance record(s) imported successfully."));
    }

    /// <summary>Lists attendance summaries for every employee in the given evaluation period.</summary>
    [HttpGet("summary/{periodId:int}")]
    public async Task<IActionResult> GetSummaryByPeriod(int periodId)
    {
        var result = await _attendanceService.GetSummaryByPeriodAsync(periodId);
        return Ok(ApiResponse<List<AttendanceSummaryDto>>.SuccessResponse(result));
    }

    private int RequireUserId() => _currentUserService.UserId ?? throw new UnauthorizedAppException();

    private (int UserId, string RoleName) RequireUserIdAndRole()
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAppException();
        var roleName = _currentUserService.RoleName ?? throw new UnauthorizedAppException();
        return (userId, roleName);
    }
}
