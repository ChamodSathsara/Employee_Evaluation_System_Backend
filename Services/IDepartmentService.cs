using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto> GetByIdAsync(int departmentId);
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto request);
    Task<DepartmentDto> UpdateAsync(int departmentId, UpdateDepartmentDto request);
    Task DeleteAsync(int departmentId);
}
