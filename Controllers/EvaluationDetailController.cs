using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>
/// Combined "evaluation detail" endpoint: everything the Employee Results tab and the
/// Manager/HOD employee evaluation page need in a single call — KPI lines with
/// Employee/Supervisor/HOD marks side by side, competency scores, achievements, attendance
/// penalty, final score/grade (if processed), HOD recommendation, and Manager decision.
/// Access: Admin, the employee themself, or anyone within the employee's reporting chain
/// (Supervisor/HOD/Manager/CEO — checked via ReportsTo walk).
/// </summary>
[Route("api/evaluation-detail")]
[Authorize]
public class EvaluationDetailController : BaseApiController
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public EvaluationDetailController(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>Returns the combined evaluation detail for a given KPI assignment.</summary>
    [HttpGet("{assignmentId:int}")]
    public async Task<IActionResult> GetByAssignment(int assignmentId)
    {
        var callerId = _currentUserService.UserId ?? throw new UnauthorizedAppException();
        var callerRole = _currentUserService.RoleName ?? throw new UnauthorizedAppException();

        var assignment = await _context.KpiAssignments
            .Include(a => a.Employee).ThenInclude(e => e.Department)
            .Include(a => a.Employee).ThenInclude(e => e.Designation)
            .Include(a => a.Period)
            .Include(a => a.Details)
            .FirstOrDefaultAsync(a => a.AssignmentId == assignmentId)
            ?? throw new NotFoundException("KPI Assignment", assignmentId);

        if (!await IsAuthorizedAsync(callerId, callerRole, assignment.EmployeeId))
        {
            throw new ForbiddenException("You are not authorized to view this employee's evaluation.");
        }

        var selfEvaluation = await _context.EmployeeEvaluations
            .Include(e => e.Details)
            .FirstOrDefaultAsync(e => e.AssignmentId == assignmentId);

        var supervisorEvaluation = selfEvaluation is null ? null : await _context.SupervisorEvaluations
            .Include(s => s.Details)
            .FirstOrDefaultAsync(s => s.SelfEvaluationId == selfEvaluation.SelfEvaluationId);

        var hodEvaluation = selfEvaluation is null ? null : await _context.HodEvaluations
            .Include(h => h.Details)
            .Include(h => h.CompetencyEvaluations).ThenInclude(c => c.Competency)
            .Include(h => h.Achievements).ThenInclude(a => a.AchievementType)
            .Include(h => h.Result)
            .FirstOrDefaultAsync(h => h.SelfEvaluationId == selfEvaluation.SelfEvaluationId);

        var attendance = await _context.AttendanceSummaries
            .FirstOrDefaultAsync(a => a.EmployeeId == assignment.EmployeeId && a.PeriodId == assignment.PeriodId);

        List<Recommendation> recommendations = new();
        ManagerReview? managerReview = null;
        if (hodEvaluation?.Result is not null)
        {
            recommendations = await _context.Recommendations
                .Where(r => r.ResultId == hodEvaluation.Result.ResultId)
                .ToListAsync();

            managerReview = await _context.ManagerReviews
                .FirstOrDefaultAsync(m => m.ResultId == hodEvaluation.Result.ResultId);
        }

        var employeeDetailsByKpi = selfEvaluation?.Details.ToDictionary(d => d.KPIAssignmentDetailId) ?? new();
        var supervisorDetailsByKpi = supervisorEvaluation?.Details.ToDictionary(d => d.KPIAssignmentDetailId) ?? new();
        var hodDetailsByKpi = hodEvaluation?.Details.ToDictionary(d => d.KPIAssignmentDetailId) ?? new();

        var kpiLines = assignment.Details.Select(kpi => new EvaluationKpiLineDto
        {
            KPIAssignmentDetailId = kpi.DetailId,
            KPIName = kpi.KPIName,
            Description = kpi.Description,
            WeightPercentage = kpi.WeightPercentage,
            EmployeeMarks = employeeDetailsByKpi.TryGetValue(kpi.DetailId, out var ed) ? ed.EmployeeMarks : null,
            EmployeeComments = employeeDetailsByKpi.TryGetValue(kpi.DetailId, out var ed2) ? ed2.Comments : null,
            SupervisorMarks = supervisorDetailsByKpi.TryGetValue(kpi.DetailId, out var sd) ? sd.Marks : null,
            SupervisorComments = supervisorDetailsByKpi.TryGetValue(kpi.DetailId, out var sd2) ? sd2.Comments : null,
            HodMarks = hodDetailsByKpi.TryGetValue(kpi.DetailId, out var hd) ? hd.Marks : null,
            HodComments = hodDetailsByKpi.TryGetValue(kpi.DetailId, out var hd2) ? hd2.Comments : null
        }).ToList();

        var competencies = hodEvaluation?.CompetencyEvaluations.Select(c => new EvaluationCompetencyLineDto
        {
            CompetencyId = c.CompetencyId,
            CompetencyName = c.Competency.Name,
            Score = c.Score,
            Remarks = c.Remarks
        }).ToList() ?? new List<EvaluationCompetencyLineDto>();

        var achievements = hodEvaluation?.Achievements.Select(a => new EvaluationAchievementLineDto
        {
            AchievementId = a.AchievementId,
            AchievementTypeName = a.AchievementType.Name,
            Title = a.Title,
            Marks = a.Marks,
            Attachment = a.Attachment,
            Remarks = a.Remarks
        }).ToList() ?? new List<EvaluationAchievementLineDto>();

        var result = new EvaluationDetailDto
        {
            AssignmentId = assignment.AssignmentId,
            EmployeeId = assignment.EmployeeId,
            EmployeeName = $"{assignment.Employee.FirstName} {assignment.Employee.LastName}",
            DepartmentName = assignment.Employee.Department?.DepartmentName,
            DesignationName = assignment.Employee.Designation?.DesignationName,
            PeriodId = assignment.PeriodId,
            PeriodName = assignment.Period.Name,

            SelfEvaluationId = selfEvaluation?.SelfEvaluationId,
            SelfEvaluationStatus = selfEvaluation?.Status ?? "Not Started",

            SupervisorEvaluationId = supervisorEvaluation?.SupervisorEvaluationId,
            SupervisorEvaluationStatus = supervisorEvaluation?.Status,

            HodEvaluationId = hodEvaluation?.HODEvaluationId,
            HodEvaluationStatus = hodEvaluation?.Status,

            KpiLines = kpiLines,
            Competencies = competencies,
            Achievements = achievements,

            AttendancePenalty = attendance?.PenaltyMarks,

            IsProcessed = hodEvaluation?.Result is not null,
            ResultId = hodEvaluation?.Result?.ResultId,
            KPIScore = hodEvaluation?.Result?.KPIScore,
            CompetencyScore = hodEvaluation?.Result?.CompetencyScore,
            AchievementScore = hodEvaluation?.Result?.AchievementScore,
            FinalScore = hodEvaluation?.Result?.FinalScore,
            Grade = hodEvaluation?.Result?.Grade,

            RecommendationTypes = recommendations.Select(r => r.RecommendationType).ToList(),
            RecommendationNote = recommendations.FirstOrDefault()?.RecommendationNote,

            ManagerDecision = managerReview?.Decision,
            ManagerComments = managerReview?.Comments
        };

        return Ok(ApiResponse<EvaluationDetailDto>.SuccessResponse(result));
    }

    /// <summary>Convenience lookup: resolve the latest KPI assignment for an employee/period, then return its evaluation detail.</summary>
    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployeeAndPeriod(int employeeId, [FromQuery] int periodId)
    {
        var assignment = await _context.KpiAssignments
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.PeriodId == periodId)
            ?? throw new NotFoundException("No KPI assignment was found for this employee in the specified period.");

        return await GetByAssignment(assignment.AssignmentId);
    }

    /// <summary>
    /// Authorization: Admin always allowed; the employee may view their own record;
    /// anyone in the employee's upward ReportsTo chain (direct or indirect manager) is allowed.
    /// </summary>
    private async Task<bool> IsAuthorizedAsync(int callerId, string callerRole, int employeeId)
    {
        if (callerRole == RoleNames.Admin || callerId == employeeId || callerRole == RoleNames.Manager)
        {
            return true;
        }

        var currentId = (int?)employeeId;
        var visited = new HashSet<int>();

        while (currentId.HasValue && visited.Add(currentId.Value))
        {
            var managerId = await _context.Users
                .Where(u => u.UserId == currentId.Value)
                .Select(u => u.ReportedSupervisorId ?? u.ReportedHodId)
                .FirstOrDefaultAsync();

            if (managerId == callerId)
            {
                return true;
            }

            currentId = managerId;
        }

        return false;
    }
}
