using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Grade Calculation endpoints (SRS Step 12-13).</summary>
[Route("api/evaluation-results")]
[Authorize]
public class EvaluationResultsController : BaseApiController
{
    private readonly IGradeCalculationService _gradeCalculationService;

    public EvaluationResultsController(IGradeCalculationService gradeCalculationService)
    {
        _gradeCalculationService = gradeCalculationService;
    }

    /// <summary>Processes a Submitted HOD evaluation and computes the Final Score and Grade.</summary>
    [HttpPost("process/{hodEvaluationId:int}")]
    [Authorize(Roles = $"{RoleNames.HOD},{RoleNames.Admin}")]
    public async Task<IActionResult> Process(int hodEvaluationId)
    {
        var result = await _gradeCalculationService.ProcessEvaluationAsync(hodEvaluationId);
        return Ok(ApiResponse<EvaluationResultDto>.SuccessResponse(result, "Evaluation processed successfully."));
    }

    /// <summary>Returns a single evaluation result by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _gradeCalculationService.GetByIdAsync(id);
        return Ok(ApiResponse<EvaluationResultDto>.SuccessResponse(result));
    }

    /// <summary>Returns every evaluation result for the given employee, across all periods.</summary>
    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployee(int employeeId)
    {
        var result = await _gradeCalculationService.GetByEmployeeAsync(employeeId);
        return Ok(ApiResponse<List<EvaluationResultDto>>.SuccessResponse(result));
    }

    /// <summary>Returns every evaluation result for the given evaluation period.</summary>
    [HttpGet("period/{periodId:int}")]
    public async Task<IActionResult> GetByPeriod(int periodId)
    {
        var result = await _gradeCalculationService.GetByPeriodAsync(periodId);
        return Ok(ApiResponse<List<EvaluationResultDto>>.SuccessResponse(result));
    }
}
