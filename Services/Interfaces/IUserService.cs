using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services.Interfaces;

public interface IUserService
{
    Task<PagedResultDto<UserDto>> GetAllAsync(UserSearchDto search);

    Task<UserDto> GetByIdAsync(int userId);

    /// <summary>Creates a user. Caller must hold the Admin role (enforced at controller and re-validated here). Validates the ReportsTo hierarchy for the target role.</summary>
    Task<UserDto> CreateUserAsync(CreateUserDto request);

    Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto request);

    /// <summary>Deletes a user permanently. Blocked (ConflictException) if the user has dependent records (KPIs created/assigned, direct reports) — deactivate instead in that case.</summary>
    Task DeleteUserAsync(int userId);

    Task<UserDto> UpdateStatusAsync(int userId, UpdateUserStatusDto request);

    Task<List<UserDto>> GetDirectReportsAsync(int managerUserId);

    Task<List<UserDto>> GetByRoleAsync(int roleId);

    Task<List<UserDto>> GetByDepartmentAsync(int departmentId);
}
