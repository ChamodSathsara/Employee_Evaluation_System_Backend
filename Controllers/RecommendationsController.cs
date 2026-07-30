using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>HOD Recommendation endpoints (SRS Step 14). Write restricted to HOD.</summary>
[Route("api/recommendations")]
[Authorize]
public class RecommendationsController : BaseApiController
{
    private readonly IRecommendationService _recommendationService;
    private readonly ICurrentUserService _currentUserService;

    public RecommendationsController(IRecommendationService recommendationService, ICurrentUserService currentUserService)
    {
        _recommendationService = recommendationService;
        _currentUserService = currentUserService;
    }

    /// <summary>Adds a recommendation (Promotion, Salary Increment, Training, Extend Probation, No Increment) to a processed result.</summary>
    [HttpPost]
    [Authorize(Roles = $"{RoleNames.HOD},{RoleNames.Admin}")]
    public async Task<IActionResult> Create([FromQuery] int resultId, [FromBody] CreateRecommendationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var hodId = RequireUserId();
        var result = await _recommendationService.CreateAsync(hodId, resultId, request);
        return Ok(ApiResponse<RecommendationDto>.SuccessResponse(result, "Recommendation added successfully."));
    }

    /// <summary>Updates an existing recommendation.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{RoleNames.HOD},{RoleNames.Admin}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRecommendationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var hodId = RequireUserId();
        var result = await _recommendationService.UpdateAsync(hodId, id, request);
        return Ok(ApiResponse<RecommendationDto>.SuccessResponse(result, "Recommendation updated successfully."));
    }

    /// <summary>Lists all recommendations attached to the given evaluation result.</summary>
    [HttpGet("{resultId:int}")]
    public async Task<IActionResult> GetByResult(int resultId)
    {
        var (callerId, callerRole) = RequireUserIdAndRole();
        var result = await _recommendationService.GetByResultAsync(callerId, callerRole, resultId);
        return Ok(ApiResponse<List<RecommendationDto>>.SuccessResponse(result));
    }

    private int RequireUserId() => _currentUserService.UserId ?? throw new UnauthorizedAppException();

    private (int UserId, string RoleName) RequireUserIdAndRole()
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAppException();
        var roleName = _currentUserService.RoleName ?? throw new UnauthorizedAppException();
        return (userId, roleName);
    }
}
