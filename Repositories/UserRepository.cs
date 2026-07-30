using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeEvaluationSystem.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger) : base(context)
    {
        _logger = logger;
    }

    public async Task<User?> GetByIdWithDetailsAsync(int userId)
    {
        try
        {
            return await Context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.Designation)
                .Include(u => u.Manager)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in {Method} for UserId: {UserId}", nameof(GetByIdWithDetailsAsync), userId);
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            return await Context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.Designation)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in {Method} for Email: {Email}", nameof(GetByEmailAsync), email);
            throw;
        }
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeUserId = null)
    {
        try
        {
            var query = Context.Users.Where(u => u.Email.ToLower() == email.ToLower());
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.UserId != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in {Method} for Email: {Email}", nameof(ExistsByEmailAsync), email);
            throw;
        }
    }

    public async Task<bool> ExistsByEmployeeNoAsync(string employeeNo, int? excludeUserId = null)
    {
        try
        {
            var query = Context.Users.Where(u => u.EmployeeNo.ToLower() == employeeNo.ToLower());
            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.UserId != excludeUserId.Value);
            }

            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in {Method} for EmployeeNo: {EmployeeNo}", nameof(ExistsByEmployeeNoAsync), employeeNo);
            throw;
        }
    }

    public IQueryable<User> QueryWithDetails()
    {
        try
        {
            var users = Context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.Designation)
                .Include(u => u.Manager)
                .AsNoTracking();

            

            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in {Method}", nameof(QueryWithDetails));
            throw;
        }
    }

    public async Task<List<User>> GetDirectReportsAsync(int managerUserId)
    {
        try
        {
            return await Context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Where(u => u.ReportsTo == managerUserId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in {Method} for ManagerUserId: {ManagerUserId}", nameof(GetDirectReportsAsync), managerUserId);
            throw;
        }
    }

    public async Task<List<User>> GetByRoleIdAsync(int roleId)
    {
        try
        {
            return await Context.Users
                .Include(u => u.Department)
                .Include(u => u.Designation)
                .Where(u => u.RoleId == roleId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in {Method} for RoleId: {RoleId}", nameof(GetByRoleIdAsync), roleId);
            throw;
        }
    }

    public async Task<List<User>> GetByDepartmentIdAsync(int departmentId)
    {
        try
        {
            return await Context.Users
                .Include(u => u.Role)
                .Include(u => u.Designation)
                .Where(u => u.DepartmentId == departmentId)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in {Method} for DepartmentId: {DepartmentId}", nameof(GetByDepartmentIdAsync), departmentId);
            throw;
        }
    }

    public async Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash)
    {
        try
        {
            return await Context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.RefreshTokenHash == refreshTokenHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in {Method}", nameof(GetByRefreshTokenHashAsync));
            throw;
        }
    }

    public async Task<List<User>> GetByRoleNamesAsync(IEnumerable<string> roleNames)
    {
        try
        {
            var roleNameSet = roleNames.Select(r => r.ToLower()).ToHashSet();

            return await Context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.Designation)
                .Where(u => u.IsActive && roleNameSet.Contains(u.Role.RoleName.ToLower()))
                .OrderBy(u => u.Role.RoleName)
                .ThenBy(u => u.FirstName)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in {Method} for RoleNames: {RoleNames}", nameof(GetByRoleNamesAsync), string.Join(",", roleNames));
            throw;
        }
    }
}