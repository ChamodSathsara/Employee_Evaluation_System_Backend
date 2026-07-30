using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Reporting endpoints (SRS Module 10): individual, department, and organization-wide evaluation reports.</summary>
[Route("api/reports")]
[Authorize]
public class ReportsController : BaseApiController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>Returns the latest (or a specific period's) evaluation report for an employee.</summary>
    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetEmployeeReport(int employeeId, [FromQuery] int? periodId)
    {
        var result = await _reportService.GetEmployeeReportAsync(employeeId, periodId);
        return Ok(ApiResponse<EmployeeReportDto>.SuccessResponse(result));
    }

    /// <summary>Returns a department performance summary for a given period.</summary>
    [HttpGet("department/{departmentId:int}")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.CEO},{RoleNames.Manager},{RoleNames.HOD}")]
    public async Task<IActionResult> GetDepartmentReport(int departmentId, [FromQuery] int periodId)
    {
        var result = await _reportService.GetDepartmentReportAsync(departmentId, periodId);
        return Ok(ApiResponse<DepartmentReportDto>.SuccessResponse(result));
    }

    /// <summary>Returns an organization-wide summary (total evaluated, average score, grade distribution, per-department breakdown) for a period.</summary>
    [HttpGet("summary")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.CEO},{RoleNames.Manager}")]
    public async Task<IActionResult> GetSummary([FromQuery] int periodId)
    {
        var result = await _reportService.GetSummaryAsync(periodId);
        return Ok(ApiResponse<SummaryReportDto>.SuccessResponse(result));
    }

    /// <summary>Returns the top performers (by Final Score) for a period.</summary>
    [HttpGet("top-performers")]
    public async Task<IActionResult> GetTopPerformers([FromQuery] int periodId, [FromQuery] int top = 10)
    {
        var result = await _reportService.GetTopPerformersAsync(periodId, top);
        return Ok(ApiResponse<List<TopPerformerDto>>.SuccessResponse(result));
    }

    /// <summary>Returns employees with a "Promotion" HOD recommendation for a period.</summary>
    [HttpGet("promotion-candidates")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.CEO},{RoleNames.Manager},{RoleNames.HOD}")]
    public async Task<IActionResult> GetPromotionCandidates([FromQuery] int periodId)
    {
        var result = await _reportService.GetPromotionCandidatesAsync(periodId);
        return Ok(ApiResponse<List<RecommendationCandidateDto>>.SuccessResponse(result));
    }

    /// <summary>Returns employees with a "Salary Increment" HOD recommendation for a period.</summary>
    [HttpGet("salary-increment")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.CEO},{RoleNames.Manager},{RoleNames.HOD}")]
    public async Task<IActionResult> GetSalaryIncrementCandidates([FromQuery] int periodId)
    {
        var result = await _reportService.GetSalaryIncrementCandidatesAsync(periodId);
        return Ok(ApiResponse<List<RecommendationCandidateDto>>.SuccessResponse(result));
    }

    /// <summary>Downloads a single evaluation result as a formatted PDF report.</summary>
    [HttpGet("export/pdf/{resultId:int}")]
    public async Task<IActionResult> ExportPdf(int resultId)
    {
        var bytes = await _reportService.ExportEmployeeReportPdfAsync(resultId);
        return File(bytes, "application/pdf", $"EvaluationReport_{resultId}.pdf");
    }

    /// <summary>Downloads a single evaluation result as an Excel workbook.</summary>
    [HttpGet("export/excel/{resultId:int}")]
    public async Task<IActionResult> ExportExcel(int resultId)
    {
        var bytes = await _reportService.ExportEmployeeReportExcelAsync(resultId);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"EvaluationReport_{resultId}.xlsx");
    }
}
