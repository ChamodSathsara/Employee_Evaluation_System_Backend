using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IDesignationService
{
    Task<List<DesignationDto>> GetAllAsync();
    Task<DesignationDto> GetByIdAsync(int designationId);
    Task<DesignationDto> CreateAsync(CreateDesignationDto request);
    Task<DesignationDto> UpdateAsync(int designationId, UpdateDesignationDto request);
    Task DeleteAsync(int designationId);
}
