using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IGradeCalculationService
{
    /// <summary>
    /// Runs the grade calculation engine for a Submitted HOD evaluation:
    /// Final Score = KPI Score (80%) + Competency Score (10%) + Achievement Score (10%) - Attendance Penalty.
    /// </summary>
    Task<EvaluationResultDto> ProcessEvaluationAsync(int hodEvaluationId);

    Task<EvaluationResultDto> GetByIdAsync(int resultId);
    Task<List<EvaluationResultDto>> GetByEmployeeAsync(int employeeId);
    Task<List<EvaluationResultDto>> GetByPeriodAsync(int periodId);
}
