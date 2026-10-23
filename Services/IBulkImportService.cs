using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IBulkImportService
{
    Task<BulkImportResultDto> ImportAsync(Stream excelStream);
}
