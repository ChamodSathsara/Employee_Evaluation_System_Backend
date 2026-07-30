using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>Designation management endpoints. Admin only.</summary>
[Route("api/designations")]
[Authorize(Roles = RoleNames.Admin)]
public class DesignationsController : BaseApiController
{
    private readonly IDesignationService _designationService;

    public DesignationsController(IDesignationService designationService)
    {
        _designationService = designationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _designationService.GetAllAsync();
        return Ok(ApiResponse<List<DesignationDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _designationService.GetByIdAsync(id);
        return Ok(ApiResponse<DesignationDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDesignationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var result = await _designationService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.DesignationId },
            ApiResponse<DesignationDto>.SuccessResponse(result, "Designation created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDesignationDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var result = await _designationService.UpdateAsync(id, request);
        return Ok(ApiResponse<DesignationDto>.SuccessResponse(result, "Designation updated successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _designationService.DeleteAsync(id);
        return Ok(ApiResponse.SuccessResponse("Designation deleted successfully."));
    }
}
