using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>
/// "My Team" endpoints for HOD, Supervisor, Manager, and CEO — a non-Admin-gated view of
/// direct reports, used to populate the Employees / Departments screens. Reuses the same
/// ReportsTo relationship as UsersController.GetReportingTo, but is accessible to the
/// reporting-line roles themselves rather than only Admin.
/// </summary>
[Route("api/team")]
[Authorize]
public class TeamController : BaseApiController
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public TeamController(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>Returns the caller's direct reports (ReportsTo = caller's UserId).</summary>
    [HttpGet("my-team")]
    [Authorize(Roles = $"{RoleNames.CEO},{RoleNames.Manager},{RoleNames.HOD},{RoleNames.Supervisor},{RoleNames.Admin}")]
    public async Task<IActionResult> GetMyTeam()
    {
        var callerId = _currentUserService.UserId ?? throw new UnauthorizedAppException();

        var members = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Department)
            .Include(u => u.Designation)
            .Where(u => u.ReportedSupervisorId == callerId || u.ReportedHodId == callerId)
            .OrderBy(u => u.FirstName)
            .Select(u => new TeamMemberDto
            {
                UserId = u.UserId,
                EmployeeNo = u.EmployeeNo,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                RoleName = u.Role.RoleName,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department != null ? u.Department.DepartmentName : null,
                DesignationId = u.DesignationId,
                DesignationName = u.Designation != null ? u.Designation.DesignationName : null,
                IsActive = u.IsActive,
                HasKpis = _context.KpiAssignments.Any(k => k.EmployeeId == u.UserId)
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(ApiResponse<List<TeamMemberDto>>.SuccessResponse(members));
    }

    /// <summary>
    /// Returns every employee within the caller's organizational subtree (their direct and
    /// indirect reports), computed by walking ReportsTo downward. Used by Manager/CEO
    /// Department screens where a whole department's employees are shown, not just direct reports.
    /// </summary>
    [HttpGet("my-subtree")]
    [Authorize(Roles = $"{RoleNames.CEO},{RoleNames.Manager},{RoleNames.HOD},{RoleNames.Supervisor},{RoleNames.Admin}")]
    public async Task<IActionResult> GetMySubtree()
    {
        var callerId = _currentUserService.UserId ?? throw new UnauthorizedAppException();

        var allUsers = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Department)
            .Include(u => u.Designation)
            .AsNoTracking()
            .ToListAsync();

        var visited = new HashSet<int> { callerId };
        var queue = new Queue<int>();
        queue.Enqueue(callerId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            foreach (var report in allUsers.Where(u =>
                         u.ReportedSupervisorId == currentId || u.ReportedHodId == currentId))
            {
                if (visited.Add(report.UserId))
                {
                    queue.Enqueue(report.UserId);
                }
            }
        }

        visited.Remove(callerId);

        var kpiEmployeeIds = await _context.KpiAssignments.Select(k => k.EmployeeId).Distinct().ToListAsync();

        var members = allUsers
            .Where(u => visited.Contains(u.UserId))
            .OrderBy(u => u.FirstName)
            .Select(u => new TeamMemberDto
            {
                UserId = u.UserId,
                EmployeeNo = u.EmployeeNo,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                RoleName = u.Role.RoleName,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department?.DepartmentName,
                DesignationId = u.DesignationId,
                DesignationName = u.Designation?.DesignationName,
                IsActive = u.IsActive,
                HasKpis = kpiEmployeeIds.Contains(u.UserId)
            })
            .ToList();

        return Ok(ApiResponse<List<TeamMemberDto>>.SuccessResponse(members));
    }
}
