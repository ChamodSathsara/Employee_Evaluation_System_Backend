using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Master competency list management. Read for any authenticated user, write restricted to Admin.</summary>
[Route("api/competencies")]
[Authorize]
public class CompetenciesController : BaseApiController
{
    private readonly ICompetencyService _competencyService;

    public CompetenciesController(ICompetencyService competencyService)
    {
        _competencyService = competencyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _competencyService.GetAllAsync();
        return Ok(ApiResponse<List<CompetencyDto>>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateCompetencyDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var result = await _competencyService.CreateAsync(request);
        return Ok(ApiResponse<CompetencyDto>.SuccessResponse(result, "Competency created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCompetencyDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var result = await _competencyService.UpdateAsync(id, request);
        return Ok(ApiResponse<CompetencyDto>.SuccessResponse(result, "Competency updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _competencyService.DeleteAsync(id);
        return Ok(ApiResponse.SuccessResponse("Competency deleted successfully."));
    }
}
