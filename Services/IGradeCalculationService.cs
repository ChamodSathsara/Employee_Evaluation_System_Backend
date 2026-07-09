using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IGradeCalculationService
{
    // Final Score = KPI(80%) + Competency(10%) + Achievement(10%) - Attendance Penalty
    // Grade bands: see Helpers/GradeBandHelper.cs (SRS Chapter 3, Module 7)
    Task<EvaluationResultDto> ProcessEvaluationAsync(int hodEvaluationId);
}
