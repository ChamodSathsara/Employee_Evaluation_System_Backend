using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IReportService
{
    Task<EmployeeReportDto> GetEmployeeReportAsync(int employeeId, int? periodId = null);
    Task<DepartmentReportDto> GetDepartmentReportAsync(int departmentId, int periodId);
    Task<SummaryReportDto> GetSummaryAsync(int periodId);
    Task<List<TopPerformerDto>> GetTopPerformersAsync(int periodId, int top = 10);
    Task<List<RecommendationCandidateDto>> GetPromotionCandidatesAsync(int periodId);
    Task<List<RecommendationCandidateDto>> GetSalaryIncrementCandidatesAsync(int periodId);

    Task<byte[]> ExportEmployeeReportPdfAsync(int resultId);
    Task<byte[]> ExportEmployeeReportExcelAsync(int resultId);
}
