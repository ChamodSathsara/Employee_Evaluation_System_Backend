using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.Models;

namespace EmployeeEvaluationSystem.Repositories;

public class SupervisorEvaluationRepository : GenericRepository<SupervisorEvaluation>, ISupervisorEvaluationRepository
{
    public SupervisorEvaluationRepository(ApplicationDbContext context) : base(context)
    {
    }

    // TODO: implement SupervisorEvaluation-specific query methods declared on ISupervisorEvaluationRepository
}
