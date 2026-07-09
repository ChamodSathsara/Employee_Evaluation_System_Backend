using EmployeeEvaluationSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EmployeeEvaluationSystem.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;

    public GenericRepository(ApplicationDbContext context)
    {
        Context = context;
    }

    public async Task<T?> GetByIdAsync(int id) => await Context.Set<T>().FindAsync(id);
    public async Task<IReadOnlyList<T>> ListAllAsync() => await Context.Set<T>().ToListAsync();

    public async Task<T> AddAsync(T entity)
    {
        await Context.Set<T>().AddAsync(entity);
        return entity;
    }

    public Task UpdateAsync(T entity)
    {
        Context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        Context.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await Context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"❌ Database Update Error: {ex.Message}");
            Console.WriteLine($"❌ Inner Exception: {ex.InnerException?.Message}");
            throw;
        }
        
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Unexpected Error in SaveChangesAsync: {ex.GetType().Name}");
            Console.WriteLine($"❌ Message: {ex.Message}");
            Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            throw;
        }
    }
}
