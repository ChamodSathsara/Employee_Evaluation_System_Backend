using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Evaluation period management. Read access for any authenticated user; mutation restricted to Admin.</summary>
[Route("api/evaluation-periods")]
[Authorize]
public class EvaluationPeriodsController : BaseApiController
{
    private readonly IEvaluationPeriodService _evaluationPeriodService;

    public EvaluationPeriodsController(IEvaluationPeriodService evaluationPeriodService)
    {
        _evaluationPeriodService = evaluationPeriodService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _evaluationPeriodService.GetAllAsync();
        return Ok(ApiResponse<List<EvaluationPeriodDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _evaluationPeriodService.GetByIdAsync(id);
        return Ok(ApiResponse<EvaluationPeriodDto>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateEvaluationPeriodDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var result = await _evaluationPeriodService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.PeriodId },
            ApiResponse<EvaluationPeriodDto>.SuccessResponse(result, "Evaluation period created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEvaluationPeriodDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var result = await _evaluationPeriodService.UpdateAsync(id, request);
        return Ok(ApiResponse<EvaluationPeriodDto>.SuccessResponse(result, "Evaluation period updated successfully."));
    }

    [HttpPatch("{id:int}/activate")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Activate(int id)
    {
        var result = await _evaluationPeriodService.ActivateAsync(id);
        return Ok(ApiResponse<EvaluationPeriodDto>.SuccessResponse(result, "Evaluation period activated."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _evaluationPeriodService.DeleteAsync(id);
        return Ok(ApiResponse.SuccessResponse("Evaluation period deleted successfully."));
    }
}
