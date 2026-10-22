using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Competency scoring endpoints (SRS Step 9). Restricted to the HOD role.</summary>
[Route("api/competency-evaluations")]
[Authorize(Roles = $"{RoleNames.HOD},{RoleNames.Admin}")]
public class CompetencyEvaluationsController : BaseApiController
{
    private readonly ICompetencyEvaluationService _competencyEvaluationService;
    private readonly ICurrentUserService _currentUserService;

    public CompetencyEvaluationsController(ICompetencyEvaluationService competencyEvaluationService, ICurrentUserService currentUserService)
    {
        _competencyEvaluationService = competencyEvaluationService;
        _currentUserService = currentUserService;
    }

    /// <summary>Lists all competency scores entered so far for the given HOD evaluation.</summary>
    [HttpGet("{hodEvaluationId:int}")]
    public async Task<IActionResult> GetByHodEvaluation(int hodEvaluationId)
    {
        var (callerId, callerRole) = RequireUserIdAndRole();
        var result = await _competencyEvaluationService.GetByHodEvaluationAsync(callerId, callerRole, hodEvaluationId);
        return Ok(ApiResponse<List<CompetencyEvaluationDto>>.SuccessResponse(result));
    }

    /// <summary>Submits (creates or updates) a 1-5 score for one competency on the HOD's own Draft evaluation.</summary>
    [HttpPost]
    public async Task<IActionResult> Submit([FromQuery] int hodEvaluationId, [FromBody] SubmitCompetencyEvaluationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var hodId = RequireUserId();
        var result = await _competencyEvaluationService.SubmitAsync(hodId, hodEvaluationId, request);
        return Ok(ApiResponse<CompetencyEvaluationDto>.SuccessResponse(result, "Competency score saved."));
    }

    /// <summary>Creates a custom competency and adds its 1-5 score to the HOD's own Draft evaluation.</summary>
    [HttpPost("custom")]
    [Authorize(Roles = RoleNames.HOD)]
    public async Task<IActionResult> CreateAndSubmit(
        [FromQuery] int hodEvaluationId,
        [FromBody] CreateCompetencyWithScoreDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var hodId = RequireUserId();
        var result = await _competencyEvaluationService.CreateAndSubmitAsync(hodId, hodEvaluationId, request);
        return Ok(ApiResponse<CompetencyEvaluationDto>.SuccessResponse(
            result,
            "Competency created and score saved."));
    }

    /// <summary>Updates an existing competency score by id.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCompetencyEvaluationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var hodId = RequireUserId();
        var result = await _competencyEvaluationService.UpdateAsync(hodId, id, request);
        return Ok(ApiResponse<CompetencyEvaluationDto>.SuccessResponse(result, "Competency score updated."));
    }

    private int RequireUserId() => _currentUserService.UserId ?? throw new UnauthorizedAppException();

    private (int UserId, string RoleName) RequireUserIdAndRole()
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAppException();
        var roleName = _currentUserService.RoleName ?? throw new UnauthorizedAppException();
        return (userId, roleName);
    }
}
