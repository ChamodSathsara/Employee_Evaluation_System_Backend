using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Repositories.Interfaces;
using EmployeeEvaluationSystem.Services.Interfaces;

namespace EmployeeEvaluationSystem.Services;

public class GradeCalculationService : IGradeCalculationService
{
    private readonly IHODEvaluationRepository _hodEvaluationRepository;
    private readonly IEvaluationResultRepository _evaluationResultRepository;

    public GradeCalculationService(
        IHODEvaluationRepository hodEvaluationRepository,
        IEvaluationResultRepository evaluationResultRepository)
    {
        _hodEvaluationRepository = hodEvaluationRepository;
        _evaluationResultRepository = evaluationResultRepository;
    }

    // TODO: implement per SRS FR-49 - FR-53:
    // 1. Validate KPI, Competency, Achievement, Attendance sections are complete.
    // 2. Calculate KPIScore, CompetencyScore, AchievementScore, AttendancePenalty.
    // 3. FinalScore = KPIScore + CompetencyScore + AchievementScore - AttendancePenalty.
    // 4. Grade = GradeBandHelper.ResolveGrade(FinalScore).
    // 5. Persist an EvaluationResult and lock the evaluation from further edits.
    public Task<EvaluationResultDto> ProcessEvaluationAsync(int hodEvaluationId) => throw new NotImplementedException();
}
