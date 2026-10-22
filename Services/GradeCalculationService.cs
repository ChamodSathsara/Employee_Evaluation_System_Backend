using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Implements the Grade Calculation Engine (SRS Step 12-13):
///   Final Score = KPI Score (80%) + Competency Score (10%) + Achievement Score (10%) - Attendance Penalty
/// where:
///   KPI Score        = weighted average of HOD marks across all KPI line items (0-100) x 0.80
///   Competency Score = (average of 1-5 competency scores / 5) x 10
///   Achievement Score = sum of awarded achievement marks, already capped at 10 by AchievementService
///   Attendance Penalty = AttendanceSummary.PenaltyMarks for the employee/period
/// Grade is then resolved from the Final Score via GradeBandHelper. Processing requires the
/// HOD evaluation to be Submitted, all 10 competencies scored, and an attendance record to
/// exist for the period; it is idempotent per HOD evaluation (one EvaluationResult each).
/// </summary>
public class GradeCalculationService : IGradeCalculationService
{
    private const int RequiredCompetencyCount = 4;

    private readonly IHODEvaluationRepository _hodEvaluationRepository;
    private readonly IEvaluationResultRepository _evaluationResultRepository;
    private readonly ICompetencyEvaluationRepository _competencyEvaluationRepository;
    private readonly IEmployeeAchievementRepository _employeeAchievementRepository;
    private readonly IAttendanceSummaryRepository _attendanceSummaryRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GradeCalculationService(
        IHODEvaluationRepository hodEvaluationRepository,
        IEvaluationResultRepository evaluationResultRepository,
        ICompetencyEvaluationRepository competencyEvaluationRepository,
        IEmployeeAchievementRepository employeeAchievementRepository,
        IAttendanceSummaryRepository attendanceSummaryRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _hodEvaluationRepository = hodEvaluationRepository;
        _evaluationResultRepository = evaluationResultRepository;
        _competencyEvaluationRepository = competencyEvaluationRepository;
        _employeeAchievementRepository = employeeAchievementRepository;
        _attendanceSummaryRepository = attendanceSummaryRepository;
        _auditLogRepository = auditLogRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<EvaluationResultDto> ProcessEvaluationAsync(int hodEvaluationId)
    {
        var hodEvaluation = await _hodEvaluationRepository.GetByIdWithDetailsAsync(hodEvaluationId)
            ?? throw new NotFoundException("HOD Evaluation", hodEvaluationId);

        if (hodEvaluation.HODId != _currentUserService.UserId && !_currentUserService.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("You can only process your own HOD evaluation.");
        }

        if (hodEvaluation.Status != "Submitted")
        {
            throw new ValidationAppException("The HOD evaluation must be Submitted before it can be processed.");
        }

        if (await _evaluationResultRepository.GetByHodEvaluationAsync(hodEvaluationId) is not null)
        {
            throw new ConflictException("This HOD evaluation has already been processed.");
        }

        var competencyScores = await _competencyEvaluationRepository.GetByHodEvaluationAsync(hodEvaluationId);
        if (competencyScores.Count < RequiredCompetencyCount)
        {
            throw new ValidationAppException(
                $"All {RequiredCompetencyCount} competencies must be scored before processing. " +
                $"Currently {competencyScores.Count} of {RequiredCompetencyCount} are scored.");
        }

        // Employee and period must come from the KPI assignment that owns this exact
        // self/HOD evaluation. Do not look them up using unrelated numeric IDs.
        var assignment = hodEvaluation.SelfEvaluation.Assignment;
        var employeeId = assignment.EmployeeId;
        var periodId = assignment.PeriodId;

        var attendance = await _attendanceSummaryRepository.GetByEmployeeAndPeriodAsync(employeeId, periodId)
            ?? throw new ValidationAppException("An attendance summary must be recorded for this employee/period before processing.");

        // ---- KPI Score: weighted average of HOD marks (0-100), scaled to the 80% component ----
        var totalWeight = hodEvaluation.Details.Sum(d => d.KpiAssignmentDetail.WeightPercentage);
        if (totalWeight != 100)
        {
            throw new ValidationAppException($"KPI weights for this evaluation must total 100%; found {totalWeight}%.");
        }

        var weightedKpiAverage = hodEvaluation.Details.Sum(d => d.Marks * (d.KpiAssignmentDetail.WeightPercentage / 100m));
        var kpiScore = Math.Round(weightedKpiAverage * 0.80m, 2);

        // ---- Competency Score: (average 1-5 score / 5) x 10 ----
        var averageCompetencyRating = (decimal)competencyScores.Average(c => c.Score);
        var competencyScore = Math.Round(averageCompetencyRating / 5m * 10m, 2);

        // ---- Achievement Score: sum of awarded marks (already capped at 10 by AchievementService) ----
        var achievementScore = await _employeeAchievementRepository.GetTotalMarksAsync(hodEvaluationId);

        // ---- Attendance Penalty ----
        var attendancePenalty = attendance.PenaltyMarks;

        var finalScore = Math.Round(kpiScore + competencyScore + achievementScore - attendancePenalty, 2);
        finalScore = Math.Clamp(finalScore, 0, 100);

        var grade = GradeBandHelper.ResolveGrade(finalScore);

        var result = new EvaluationResult
        {
            HODEvaluationId = hodEvaluationId,
            KPIScore = kpiScore,
            CompetencyScore = competencyScore,
            AchievementScore = achievementScore,
            AttendancePenalty = attendancePenalty,
            FinalScore = finalScore,
            Grade = grade,
            ProcessedDate = DateTime.UtcNow
        };

        await _evaluationResultRepository.AddAsync(result);
        await LogAuditAsync("Update", "EvaluationResults", result.ResultId);
        await _evaluationResultRepository.SaveChangesAsync();

        var created = await _evaluationResultRepository.GetByIdWithDetailsAsync(result.ResultId);
        return _mapper.Map<EvaluationResultDto>(created!);
    }

    public async Task<EvaluationResultDto> GetByIdAsync(int resultId)
    {
        var result = await _evaluationResultRepository.GetByIdWithDetailsAsync(resultId)
            ?? throw new NotFoundException("Evaluation Result", resultId);

        return _mapper.Map<EvaluationResultDto>(result);
    }

    public async Task<List<EvaluationResultDto>> GetByEmployeeAsync(int employeeId)
    {
        var results = await _evaluationResultRepository.GetByEmployeeAsync(employeeId);
        return _mapper.Map<List<EvaluationResultDto>>(results);
    }

    public async Task<List<EvaluationResultDto>> GetByPeriodAsync(int periodId)
    {
        var results = await _evaluationResultRepository.GetByPeriodAsync(periodId);
        return _mapper.Map<List<EvaluationResultDto>>(results);
    }

    private async Task LogAuditAsync(string action, string tableName, int? recordId)
    {
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = _currentUserService.UserId ?? 0,
            Action = action,
            TableName = tableName,
            RecordId = recordId,
            CreatedDate = DateTime.UtcNow
        });
    }
}
