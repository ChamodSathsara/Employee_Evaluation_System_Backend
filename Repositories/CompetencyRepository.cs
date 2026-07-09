using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories.Interfaces;

namespace EmployeeEvaluationSystem.Repositories;

public class CompetencyRepository : GenericRepository<Competency>, ICompetencyRepository
{
    public CompetencyRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement Competency-specific query methods declared on ICompetencyRepository
}
