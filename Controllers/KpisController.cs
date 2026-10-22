using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>
/// KPI Assignment endpoints. Creation is restricted to CEO, Manager, HOD, and Supervisor,
/// each of whom may only assign KPIs to their own direct reports (see IKpiAssignmentService
/// for the exact role/hierarchy matrix). Admin and Employee never create KPI assignments.
/// </summary>
[Route("api/kpis")]
[Authorize]
public class KpisController : BaseApiController
{
    private readonly IKpiAssignmentService _kpiAssignmentService;
    private readonly ICurrentUserService _currentUserService;

    public KpisController(IKpiAssignmentService kpiAssignmentService, ICurrentUserService currentUserService)
    {
        _kpiAssignmentService = kpiAssignmentService;
        _currentUserService = currentUserService;
    }

    /// <summary>Creates a new Draft KPI assignment for a direct report.</summary>
    [HttpPost]
    [Authorize(Roles = $"{RoleNames.CEO},{RoleNames.Manager},{RoleNames.HOD},{RoleNames.Supervisor}")]
    public async Task<IActionResult> Create([FromBody] CreateKpiAssignmentDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var callerId = RequireUserId();
        var result = await _kpiAssignmentService.CreateAsync(callerId, request);
        return CreatedAtAction(nameof(GetById), new { assignmentId = result.AssignmentId },
            ApiResponse<KpiAssignmentDto>.SuccessResponse(result, "KPI assignment created as Draft."));
    }

    /// <summary>Replaces the line items of a Draft KPI assignment (creator only).</summary>
    [HttpPut("{assignmentId:int}")]
    [Authorize(Roles = $"{RoleNames.CEO},{RoleNames.Manager},{RoleNames.HOD},{RoleNames.Supervisor}")]
    public async Task<IActionResult> Update(int assignmentId, [FromBody] UpdateKpiAssignmentDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var callerId = RequireUserId();
        var result = await _kpiAssignmentService.UpdateAsync(callerId, assignmentId, request);
        return Ok(ApiResponse<KpiAssignmentDto>.SuccessResponse(result, "KPI assignment updated."));
    }

    /// <summary>Deletes a Draft or Published KPI assignment (creator only, with no evaluation activity).</summary>
    [HttpDelete("{assignmentId:int}")]
    [Authorize(Roles = $"{RoleNames.CEO},{RoleNames.Manager},{RoleNames.HOD},{RoleNames.Supervisor}")]
    public async Task<IActionResult> Delete(int assignmentId)
    {
        var callerId = RequireUserId();
        await _kpiAssignmentService.DeleteAsync(callerId, assignmentId);
        return Ok(ApiResponse.SuccessResponse("KPI assignment deleted."));
    }

    /// <summary>Publishes a Draft KPI assignment, making it visible to the employee.</summary>
    [HttpPost("{assignmentId:int}/publish")]
    [Authorize(Roles = $"{RoleNames.CEO},{RoleNames.Manager},{RoleNames.HOD},{RoleNames.Supervisor}")]
    public async Task<IActionResult> Publish(int assignmentId)
    {
        var callerId = RequireUserId();
        var result = await _kpiAssignmentService.PublishAsync(callerId, assignmentId);
        return Ok(ApiResponse<KpiAssignmentDto>.SuccessResponse(result, "KPI assignment published."));
    }

    /// <summary>Reverts a Published KPI assignment back to Draft for correction.</summary>
    [HttpPost("{assignmentId:int}/draft")]
    [Authorize(Roles = $"{RoleNames.CEO},{RoleNames.Manager},{RoleNames.HOD},{RoleNames.Supervisor}")]
    public async Task<IActionResult> RevertToDraft(int assignmentId)
    {
        var callerId = RequireUserId();
        var result = await _kpiAssignmentService.RevertToDraftAsync(callerId, assignmentId);
        return Ok(ApiResponse<KpiAssignmentDto>.SuccessResponse(result, "KPI assignment reverted to Draft."));
    }

    /// <summary>Returns the caller's own published KPI assignments.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMy()
    {
        var callerId = RequireUserId();
        var result = await _kpiAssignmentService.GetMyKpisAsync(callerId);
        return Ok(ApiResponse<List<KpiAssignmentDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Returns every KPI assignment (all periods) belonging to the caller's direct reports.
    /// Available to CEO, Manager, HOD, and Supervisor for their "Employees"/team screens.
    /// </summary>
    [HttpGet("my-team")]
    [Authorize(Roles = $"{RoleNames.CEO},{RoleNames.Manager},{RoleNames.HOD},{RoleNames.Supervisor},{RoleNames.Admin}")]
    public async Task<IActionResult> GetMyTeamKpis()
    {
        var callerId = RequireUserId();
        var result = await _kpiAssignmentService.GetMyTeamKpisAsync(callerId);
        return Ok(ApiResponse<List<KpiAssignmentDto>>.SuccessResponse(result));
    }

    /// <summary>Returns a single KPI assignment by id (Admin, the employee, the creator, or the employee's direct manager).</summary>
    [HttpGet("{assignmentId:int}")]
    public async Task<IActionResult> GetById(int assignmentId)
    {
        var (callerId, callerRole) = RequireUserIdAndRole();
        var result = await _kpiAssignmentService.GetByIdAsync(callerId, callerRole, assignmentId);
        return Ok(ApiResponse<KpiAssignmentDto>.SuccessResponse(result));
    }

    /// <summary>Returns every KPI assignment for a given employee across all periods.</summary>
    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        var (callerId, callerRole) = RequireUserIdAndRole();
        var result = await _kpiAssignmentService.GetByEmployeeAsync(callerId, callerRole, employeeId);
        return Ok(ApiResponse<List<KpiAssignmentDto>>.SuccessResponse(result));
    }

    /// <summary>Returns every KPI assignment for a given evaluation period, scoped to the caller's organizational subtree (Admin sees all).</summary>
    [HttpGet("period/{periodId:int}")]
    public async Task<IActionResult> GetByPeriod(int periodId)
    {
        var (callerId, callerRole) = RequireUserIdAndRole();
        var result = await _kpiAssignmentService.GetByPeriodAsync(callerId, callerRole, periodId);
        return Ok(ApiResponse<List<KpiAssignmentDto>>.SuccessResponse(result));
    }

    // ---------- private helpers ----------

    private int RequireUserId() => _currentUserService.UserId ?? throw new UnauthorizedAppException();

    private (int UserId, string RoleName) RequireUserIdAndRole()
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAppException();
        var roleName = _currentUserService.RoleName ?? throw new UnauthorizedAppException();
        return (userId, roleName);
    }
}
