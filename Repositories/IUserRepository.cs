using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    /// <summary>Fetches a user with Role, Department, and Designation eagerly loaded, by primary key.</summary>
    Task<User?> GetByIdWithDetailsAsync(int userId);

    /// <summary>Fetches a user with Role, Department, and Designation eagerly loaded, by email (case-insensitive).</summary>
    Task<User?> GetByEmailAsync(string email);

    Task<bool> ExistsByEmailAsync(string email, int? excludeUserId = null);

    Task<bool> ExistsByEmployeeNoAsync(string employeeNo, int? excludeUserId = null);

    /// <summary>Returns all users, with Role/Department/Designation eagerly loaded, for listing/search.</summary>
    IQueryable<User> QueryWithDetails();

    Task<List<User>> GetDirectReportsAsync(int managerUserId);

    Task<List<User>> GetByRoleIdAsync(int roleId);

    Task<List<User>> GetByDepartmentIdAsync(int departmentId);

    Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash);

    /// <summary>Returns all active users whose Role.RoleName is in the given set (e.g. management roles), with Role/Department/Designation eagerly loaded.</summary>
    Task<List<User>> GetByRoleNamesAsync(IEnumerable<string> roleNames);
}