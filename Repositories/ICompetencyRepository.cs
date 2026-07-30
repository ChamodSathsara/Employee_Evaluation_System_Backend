using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public interface ICompetencyRepository : IGenericRepository<Competency>
{
    Task<bool> ExistsByNameAsync(string name, int? excludeCompetencyId = null);
    Task<bool> IsInUseAsync(int competencyId);
}
