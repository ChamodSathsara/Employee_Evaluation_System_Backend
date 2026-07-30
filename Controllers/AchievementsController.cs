using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Special Achievements endpoints (SRS Step 10). Restricted to the HOD role.</summary>
[Route("api/achievements")]
[Authorize(Roles = $"{RoleNames.HOD},{RoleNames.Admin}")]
public class AchievementsController : BaseApiController
{
    private readonly IAchievementService _achievementService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;

    public AchievementsController(
        IAchievementService achievementService,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService)
    {
        _achievementService = achievementService;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
    }

    /// <summary>Lists all achievement entries for the given HOD evaluation.</summary>
    [HttpGet("{hodEvaluationId:int}")]
    public async Task<IActionResult> GetByHodEvaluation(int hodEvaluationId)
    {
        var (callerId, callerRole) = RequireUserIdAndRole();
        var result = await _achievementService.GetByHodEvaluationAsync(callerId, callerRole, hodEvaluationId);
        return Ok(ApiResponse<List<EmployeeAchievementDto>>.SuccessResponse(result));
    }

    /// <summary>Adds a new achievement entry, enforcing the total marks cap for the evaluation.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeAchievementDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var hodId = RequireUserId();
        var result = await _achievementService.CreateAsync(hodId, request);
        return Ok(ApiResponse<EmployeeAchievementDto>.SuccessResponse(result, "Achievement added successfully."));
    }

    /// <summary>Updates an existing achievement entry.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeAchievementDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var hodId = RequireUserId();
        var result = await _achievementService.UpdateAsync(hodId, id, request);
        return Ok(ApiResponse<EmployeeAchievementDto>.SuccessResponse(result, "Achievement updated successfully."));
    }

    /// <summary>Removes an achievement entry.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var hodId = RequireUserId();
        await _achievementService.DeleteAsync(hodId, id);
        return Ok(ApiResponse.SuccessResponse("Achievement deleted successfully."));
    }

    /// <summary>Uploads a supporting document for an achievement entry (stored under wwwroot/uploads).</summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadAttachment([FromForm] UploadAchievementAttachmentDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var hodId = RequireUserId();

        await using var stream = request.File.OpenReadStream();
        var storedPath = await _fileStorageService.UploadAsync(stream, request.File.FileName);

        var result = await _achievementService.UploadAttachmentAsync(hodId, request.AchievementId, storedPath);
        return Ok(ApiResponse<EmployeeAchievementDto>.SuccessResponse(result, "Attachment uploaded successfully."));
    }

    private int RequireUserId() => _currentUserService.UserId ?? throw new UnauthorizedAppException();

    private (int UserId, string RoleName) RequireUserIdAndRole()
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAppException();
        var roleName = _currentUserService.RoleName ?? throw new UnauthorizedAppException();
        return (userId, roleName);
    }
}
