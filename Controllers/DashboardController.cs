using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Role-specific landing dashboard endpoints (SRS API section 20). Each endpoint is restricted to its corresponding role.</summary>
[Route("api/dashboard")]
[Authorize]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;
    private readonly ICurrentUserService _currentUserService;

    public DashboardController(IDashboardService dashboardService, ICurrentUserService currentUserService)
    {
        _dashboardService = dashboardService;
        _currentUserService = currentUserService;
    }

    /// <summary>Returns My KPIs, pending self-evaluation flag, latest grade, and notifications.</summary>
    [HttpGet("employee")]
    [Authorize(Roles = $"{RoleNames.Employee},{RoleNames.Admin}")]
    public async Task<IActionResult> GetEmployeeDashboard()
    {
        var userId = RequireUserId();
        var result = await _dashboardService.GetEmployeeDashboardAsync(userId);
        return Ok(ApiResponse<EmployeeDashboardDto>.SuccessResponse(result));
    }

    /// <summary>Returns the Supervisor's employees, pending reviews, and notifications.</summary>
    [HttpGet("supervisor")]
    [Authorize(Roles = $"{RoleNames.Supervisor},{RoleNames.Admin}")]
    public async Task<IActionResult> GetSupervisorDashboard()
    {
        var userId = RequireUserId();
        var result = await _dashboardService.GetSupervisorDashboardAsync(userId);
        return Ok(ApiResponse<SupervisorDashboardDto>.SuccessResponse(result));
    }

    /// <summary>Returns the HOD's department employees, pending reviews, KPI status, and notifications.</summary>
    [HttpGet("hod")]
    [Authorize(Roles = $"{RoleNames.HOD},{RoleNames.Admin}")]
    public async Task<IActionResult> GetHodDashboard()
    {
        var userId = RequireUserId();
        var result = await _dashboardService.GetHodDashboardAsync(userId);
        return Ok(ApiResponse<HodDashboardDto>.SuccessResponse(result));
    }

    /// <summary>Returns the Manager's pending approvals, HOD evaluations, and grade distribution.</summary>
    [HttpGet("manager")]
    [Authorize(Roles = $"{RoleNames.Manager},{RoleNames.Admin}")]
    public async Task<IActionResult> GetManagerDashboard()
    {
        var userId = RequireUserId();
        var result = await _dashboardService.GetManagerDashboardAsync(userId);
        return Ok(ApiResponse<ManagerDashboardDto>.SuccessResponse(result));
    }

    /// <summary>Returns Manager performance, promotion summary, and company statistics.</summary>
    [HttpGet("ceo")]
    [Authorize(Roles = $"{RoleNames.CEO},{RoleNames.Admin}")]
    public async Task<IActionResult> GetCeoDashboard()
    {
        var userId = RequireUserId();
        var result = await _dashboardService.GetCeoDashboardAsync(userId);
        return Ok(ApiResponse<CeoDashboardDto>.SuccessResponse(result));
    }

    /// <summary>Returns user/department counts, the active evaluation period, and recent audit log activity.</summary>
    [HttpGet("admin")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var result = await _dashboardService.GetAdminDashboardAsync();
        return Ok(ApiResponse<AdminDashboardDto>.SuccessResponse(result));
    }

    private int RequireUserId() => _currentUserService.UserId ?? throw new UnauthorizedAppException();
}
