using EmployeeEvaluationSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;

    public GenericRepository(ApplicationDbContext context)
    {
        Context = context;
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            return await Context.Set<T>().FindAsync(id);
        }
        catch (Exception ex)
        {
            // Log the exception if you have a logger
            // _logger.LogError(ex, "Error occurred while getting entity by Id: {Id}", id);

            throw new Exception($"An error occurred while retrieving the entity with Id {id}.", ex);
        }
    }
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
        try
        {
            Context.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }
        catch (ArgumentNullException ex)
        {
            throw new ArgumentNullException( "The entity to delete cannot be null.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"An error occurred while deleting the {typeof(T).Name} entity.", ex);
        }
    }

    public Task<int> SaveChangesAsync() => Context.SaveChangesAsync();
}
