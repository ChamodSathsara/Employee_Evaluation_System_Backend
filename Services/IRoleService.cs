using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllAsync();
    Task<RoleDto> GetByIdAsync(int roleId);
    Task<RoleDto> CreateAsync(CreateRoleDto request);
    Task<RoleDto> UpdateAsync(int roleId, UpdateRoleDto request);
    Task DeleteAsync(int roleId);
}
