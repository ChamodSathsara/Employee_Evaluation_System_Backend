using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Controllers;

/// <summary>
/// Master list of achievement types (Degree, Masters, Certifications, Awards, Innovation,
/// Training, ...) used as the dropdown source when HOD/Supervisor add a Special Achievement.
/// Read access for any authenticated user; write restricted to Admin.
/// </summary>
[Route("api/achievement-types")]
[Authorize]
public class AchievementTypesController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public AchievementTypesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Lists every achievement type.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var types = await _context.AchievementTypes
            .OrderBy(a => a.Name)
            .Select(a => new AchievementTypeDto { AchievementTypeId = a.AchievementTypeId, Name = a.Name })
            .AsNoTracking()
            .ToListAsync();

        return Ok(ApiResponse<List<AchievementTypeDto>>.SuccessResponse(types));
    }

    /// <summary>Returns a single achievement type by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var type = await _context.AchievementTypes.AsNoTracking().FirstOrDefaultAsync(a => a.AchievementTypeId == id)
            ?? throw new NotFoundException("Achievement Type", id);

        return Ok(ApiResponse<AchievementTypeDto>.SuccessResponse(
            new AchievementTypeDto { AchievementTypeId = type.AchievementTypeId, Name = type.Name }));
    }

    /// <summary>Creates a new achievement type. Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateAchievementTypeDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var exists = await _context.AchievementTypes.AnyAsync(a => a.Name.ToLower() == request.Name.ToLower());
        if (exists)
        {
            throw new ConflictException($"An achievement type named '{request.Name}' already exists.");
        }

        var type = new AchievementType { Name = request.Name };
        _context.AchievementTypes.Add(type);
        await _context.SaveChangesAsync();

        var dto = new AchievementTypeDto { AchievementTypeId = type.AchievementTypeId, Name = type.Name };
        return CreatedAtAction(nameof(GetById), new { id = type.AchievementTypeId },
            ApiResponse<AchievementTypeDto>.SuccessResponse(dto, "Achievement type created successfully."));
    }

    /// <summary>Updates an achievement type. Admin only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAchievementTypeDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.FailResponse("Invalid request payload."));
        }

        var type = await _context.AchievementTypes.FirstOrDefaultAsync(a => a.AchievementTypeId == id)
            ?? throw new NotFoundException("Achievement Type", id);

        var duplicate = await _context.AchievementTypes
            .AnyAsync(a => a.AchievementTypeId != id && a.Name.ToLower() == request.Name.ToLower());
        if (duplicate)
        {
            throw new ConflictException($"An achievement type named '{request.Name}' already exists.");
        }

        type.Name = request.Name;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<AchievementTypeDto>.SuccessResponse(
            new AchievementTypeDto { AchievementTypeId = type.AchievementTypeId, Name = type.Name },
            "Achievement type updated successfully."));
    }

    /// <summary>Deletes an achievement type. Admin only. Blocked if it is already referenced by an achievement entry.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var type = await _context.AchievementTypes.FirstOrDefaultAsync(a => a.AchievementTypeId == id)
            ?? throw new NotFoundException("Achievement Type", id);

        var inUse = await _context.EmployeeAchievements.AnyAsync(a => a.AchievementTypeId == id);
        if (inUse)
        {
            throw new ConflictException("This achievement type is already in use and cannot be deleted.");
        }

        _context.AchievementTypes.Remove(type);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.SuccessResponse("Achievement type deleted successfully."));
    }
}
