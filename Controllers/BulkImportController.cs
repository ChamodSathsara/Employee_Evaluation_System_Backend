using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeEvaluationSystem.Controllers;

[Route("api/bulk-import")]
[Authorize(Roles = RoleNames.Admin)]
public class BulkImportController : BaseApiController
{
    private const long MaxFileSize = 10 * 1024 * 1024;
    private readonly IBulkImportService _bulkImportService;

    public BulkImportController(IBulkImportService bulkImportService)
    {
        _bulkImportService = bulkImportService;
    }

    /// <summary>Imports Departments, Designations, and Users from a three-sheet Excel workbook.</summary>
    [HttpPost("excel")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportExcel(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.FailResponse("An Excel file is required."));
        }
        if (file.Length > MaxFileSize)
        {
            return BadRequest(ApiResponse<object>.FailResponse("The Excel file must not exceed 10 MB."));
        }
        if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<object>.FailResponse("Only .xlsx files are supported."));
        }

        await using var stream = file.OpenReadStream();
        var result = await _bulkImportService.ImportAsync(stream);
        return Ok(ApiResponse<BulkImportResultDto>.SuccessResponse(result, "Excel data imported successfully."));
    }
}
