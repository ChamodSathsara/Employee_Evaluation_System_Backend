using DocumentFormat.OpenXml.Bibliography;
using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByIdWithDetailsAsync(int userId)
    {
        return await Context.Users
            .Include(u => u.Role)
            .Include(u => u.Department)
            .Include(u => u.Designation)
            .Include(u => u.Manager)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await Context.Users
            .Include(u => u.Role)
            .Include(u => u.Department)
            .Include(u => u.Designation)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeUserId = null)
    {
        var query = Context.Users.Where(u => u.Email.ToLower() == email.ToLower());
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.UserId != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByEmployeeNoAsync(string employeeNo, int? excludeUserId = null)
    {
        var query = Context.Users.Where(u => u.EmployeeNo.ToLower() == employeeNo.ToLower());
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.UserId != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }

    public IQueryable<User> QueryWithDetails()
    {
        return Context.Users
            .Include(u => u.Role)
            .Include(u => u.Department)
            .Include(u => u.Designation)
            .AsNoTracking();
    }

    public async Task<List<User>> GetDirectReportsAsync(int managerUserId)
    {
        return await Context.Users
            .Include(u => u.Role)
            .Include(u => u.Department)
            .Where(u => u.ReportsTo == managerUserId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<User>> GetByRoleIdAsync(int roleId)
    {
        return await Context.Users
            .Include(u => u.Department)
            .Include(u => u.Designation)
            .Where(u => u.RoleId == roleId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<User>> GetByDepartmentIdAsync(int departmentId)
    {
        return await Context.Users
            .Include(u => u.Role)
            .Include(u => u.Designation)
            .Where(u => u.DepartmentId == departmentId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash)
    {
        return await Context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.RefreshTokenHash == refreshTokenHash);
    }

    public async Task<User?> GetTest()
    {
        try
        {
            Console.WriteLine("🔍 Starting GetTest...");
            User? name = await Context.Users.FirstOrDefaultAsync();
            Console.WriteLine($"✅ First user found: {name?.UserId}");

            var result = await Context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Include(u => u.Designation)
                .FirstOrDefaultAsync(u => u.UserId == 1);

            Console.WriteLine($"✅ User with ID 1 found: {result?.UserId}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            throw;
        }
    }
}
