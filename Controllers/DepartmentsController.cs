using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>
/// Department management. Read endpoints (list, single, employees, progress) are available
/// to any authenticated user so Manager/CEO/HOD dashboards can resolve department names and
/// stats; write endpoints (create/update/delete) remain Admin only.
/// </summary>
[Route("api/departments")]
[Authorize]
public class DepartmentsController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public DepartmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var departments = await _context.Departments
            .Select(d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                UserCount = d.Users.Count
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(ApiResponse<List<DepartmentDto>>.SuccessResponse(departments));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await _context.Departments
            .Where(d => d.DepartmentId == id)
            .Select(d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                UserCount = d.Users.Count
            })
            .AsNoTracking()
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException("Department", id);

        return Ok(ApiResponse<DepartmentDto>.SuccessResponse(department));
    }

    /// <summary>Lists every user assigned to this department.</summary>
    [HttpGet("{id:int}/employees")]
    public async Task<IActionResult> GetEmployees(int id)
    {
        if (!await _context.Departments.AnyAsync(d => d.DepartmentId == id))
        {
            throw new NotFoundException("Department", id);
        }

        var employees = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Designation)
            .Where(u => u.DepartmentId == id)
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
                DesignationId = u.DesignationId,
                DesignationName = u.Designation != null ? u.Designation.DesignationName : null,
                IsActive = u.IsActive,
                HasKpis = _context.KpiAssignments.Any(k => k.EmployeeId == u.UserId)
            })
            .AsNoTracking()
            .ToListAsync();

        return Ok(ApiResponse<List<TeamMemberDto>>.SuccessResponse(employees));
    }

    /// <summary>
    /// Department card stats for a given evaluation period: employee count, HOD/Supervisor
    /// names, and completion percentages for grading and attendance. Powers the Manager/CEO
    /// "Departments" screen progress bars.
    /// </summary>
    [HttpGet("{id:int}/progress")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.CEO},{RoleNames.Manager},{RoleNames.HOD}")]
    public async Task<IActionResult> GetProgress(int id, [FromQuery] int periodId)
    {
        if (!await _context.Departments.AnyAsync(d => d.DepartmentId == id))
        {
            throw new NotFoundException("Department", id);
        }

        if (!await _context.EvaluationPeriods.AnyAsync(p => p.PeriodId == periodId))
        {
            throw new NotFoundException("Evaluation Period", periodId);
        }

        var progress = await BuildDepartmentProgressAsync(id, periodId);
        return Ok(ApiResponse<DepartmentProgressDto>.SuccessResponse(progress));
    }

    /// <summary>Progress cards for every department at once, for a given period (avoids N calls from the Departments screen).</summary>
    [HttpGet("progress")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.CEO},{RoleNames.Manager},{RoleNames.HOD}")]
    public async Task<IActionResult> GetAllProgress([FromQuery] int periodId)
    {
        if (!await _context.EvaluationPeriods.AnyAsync(p => p.PeriodId == periodId))
        {
            throw new NotFoundException("Evaluation Period", periodId);
        }

        var departmentIds = await _context.Departments.Select(d => d.DepartmentId).ToListAsync();
        var results = new List<DepartmentProgressDto>();

        foreach (var deptId in departmentIds)
        {
            results.Add(await BuildDepartmentProgressAsync(deptId, periodId));
        }

        return Ok(ApiResponse<List<DepartmentProgressDto>>.SuccessResponse(results));
    }

    /// <summary>Shared computation behind GetProgress and GetAllProgress; assumes department and period existence have already been validated by the caller.</summary>
    private async Task<DepartmentProgressDto> BuildDepartmentProgressAsync(int departmentId, int periodId)
    {
        var department = await _context.Departments.FirstAsync(d => d.DepartmentId == departmentId);

        var employees = await _context.Users
            .Where(u => u.DepartmentId == departmentId && u.IsActive)
            .ToListAsync();

        var employeeIds = employees.Select(u => u.UserId).ToList();
        var employeeCount = employeeIds.Count;

        var hodRoleId = await _context.Roles.Where(r => r.RoleName == RoleNames.HOD).Select(r => r.RoleId).FirstOrDefaultAsync();
        var supervisorRoleId = await _context.Roles.Where(r => r.RoleName == RoleNames.Supervisor).Select(r => r.RoleId).FirstOrDefaultAsync();

        var hodNames = employees.Where(u => u.RoleId == hodRoleId).Select(u => $"{u.FirstName} {u.LastName}").ToList();
        var supervisorNames = employees.Where(u => u.RoleId == supervisorRoleId).Select(u => $"{u.FirstName} {u.LastName}").ToList();

        var gradedCount = employeeCount == 0
            ? 0
            : await _context.EvaluationResults
                .CountAsync(r =>
                    employeeIds.Contains(r.HODEvaluation.SelfEvaluation.EmployeeId) &&
                    r.HODEvaluation.SelfEvaluation.Assignment.PeriodId == periodId);

        var attendanceCount = employeeCount == 0
            ? 0
            : await _context.AttendanceSummaries
                .CountAsync(a => employeeIds.Contains(a.EmployeeId) && a.PeriodId == periodId);

        return new DepartmentProgressDto
        {
            DepartmentId = department.DepartmentId,
            DepartmentName = department.DepartmentName,
            EmployeeCount = employeeCount,
            HodNames = hodNames,
            SupervisorNames = supervisorNames,
            GradeCompletionPercentage = employeeCount == 0 ? 0 : Math.Round(gradedCount * 100m / employeeCount, 1),
            AttendanceCompletionPercentage = employeeCount == 0 ? 0 : Math.Round(attendanceCount * 100m / employeeCount, 1)
        };
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var exists = await _context.Departments.AnyAsync(d => d.DepartmentName.ToLower() == request.DepartmentName.ToLower());
        if (exists)
        {
            throw new ConflictException($"A department named '{request.DepartmentName}' already exists.");
        }

        var department = new Department { DepartmentName = request.DepartmentName, Description = request.Description };
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        var dto = new DepartmentDto { DepartmentId = department.DepartmentId, DepartmentName = department.DepartmentName, Description = department.Description, UserCount = 0 };
        return CreatedAtAction(nameof(GetById), new { id = department.DepartmentId },
            ApiResponse<DepartmentDto>.SuccessResponse(dto, "Department created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id)
            ?? throw new NotFoundException("Department", id);

        var duplicate = await _context.Departments
            .AnyAsync(d => d.DepartmentId != id && d.DepartmentName.ToLower() == request.DepartmentName.ToLower());
        if (duplicate)
        {
            throw new ConflictException($"A department named '{request.DepartmentName}' already exists.");
        }

        department.DepartmentName = request.DepartmentName;
        department.Description = request.Description;
        await _context.SaveChangesAsync();

        var userCount = await _context.Users.CountAsync(u => u.DepartmentId == id);
        return Ok(ApiResponse<DepartmentDto>.SuccessResponse(
            new DepartmentDto { DepartmentId = department.DepartmentId, DepartmentName = department.DepartmentName, Description = department.Description, UserCount = userCount },
            "Department updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id)
            ?? throw new NotFoundException("Department", id);

        var userCount = await _context.Users.CountAsync(u => u.DepartmentId == id);
        if (userCount > 0)
        {
            throw new ConflictException($"This department has {userCount} user(s) assigned and cannot be deleted.");
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.SuccessResponse("Department deleted successfully."));
    }
}
