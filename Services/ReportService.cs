using ClosedXML.Excel;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Implements the Reports module (SRS Chapter 3, Module 10): individual employee reports,
/// department/organization summaries, top performers, and recommendation-driven candidate
/// lists (promotion, salary increment), plus PDF/Excel export of a single evaluation result.
/// </summary>
public class ReportService : IReportService
{
    private readonly IEvaluationResultRepository _evaluationResultRepository;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IManagerReviewRepository _managerReviewRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IKPIAssignmentRepository _kPIAssignmentRepository;

    public ReportService(
        IEvaluationResultRepository evaluationResultRepository,
        IRecommendationRepository recommendationRepository,
        IManagerReviewRepository managerReviewRepository,
        IDepartmentRepository departmentRepository,
        IUserRepository userRepository, 
        IKPIAssignmentRepository kPIAssignmentRepository)
    {
        _evaluationResultRepository = evaluationResultRepository;
        _recommendationRepository = recommendationRepository;
        _managerReviewRepository = managerReviewRepository;
        _departmentRepository = departmentRepository;
        _userRepository = userRepository;
        _kPIAssignmentRepository = kPIAssignmentRepository;


        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<EmployeeReportDto> GetEmployeeReportAsync(int employeeId, int? periodId = null)
    {
        var results = await _evaluationResultRepository.GetByEmployeeAsync(employeeId);
        var result = periodId.HasValue
            ? results.FirstOrDefault(r => r.HODEvaluation.SelfEvaluation.Assignment.PeriodId == periodId.Value)
            : results.OrderByDescending(r => r.ProcessedDate).FirstOrDefault();

        if (result is null)
        {
            throw new NotFoundException("No processed evaluation result was found for this employee" + (periodId.HasValue ? " in the specified period." : "."));
        }

        return await BuildEmployeeReportAsync(result.ResultId);
    }

    public async Task<DepartmentReportDto> GetDepartmentReportAsync(int departmentId, int periodId)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId)
            ?? throw new NotFoundException("Department", departmentId);

        var employeeIds = (await _userRepository.GetByDepartmentIdAsync(departmentId)).Select(u => u.UserId).ToHashSet();
        var periodResults = await _evaluationResultRepository.GetByPeriodAsync(periodId);
        var deptResults = periodResults.Where(r => employeeIds.Contains(r.HODEvaluation.SelfEvaluation.EmployeeId)).ToList();

        return new DepartmentReportDto
        {
            DepartmentId = departmentId,
            DepartmentName = department.DepartmentName,
            EmployeeCount = deptResults.Count,
            AverageFinalScore = deptResults.Count > 0 ? Math.Round(deptResults.Average(r => r.FinalScore), 2) : 0,
            GradeDistribution = deptResults.GroupBy(r => r.Grade).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<SummaryReportDto> GetSummaryAsync(int periodId)
    {
        var results = await _evaluationResultRepository.GetByPeriodAsync(periodId);
        var departments = await _departmentRepository.ListAllAsync();

        var byDepartment = new List<DepartmentReportDto>();
        foreach (var dept in departments)
        {
            byDepartment.Add(await GetDepartmentReportAsync(dept.DepartmentId, periodId));
        }

        return new SummaryReportDto
        {
            TotalEmployeesEvaluated = results.Count,
            AverageFinalScore = results.Count > 0 ? Math.Round(results.Average(r => r.FinalScore), 2) : 0,
            GradeDistribution = results.GroupBy(r => r.Grade).ToDictionary(g => g.Key, g => g.Count()),
            ByDepartment = byDepartment.Where(d => d.EmployeeCount > 0).ToList()
        };
    }

    public async Task<List<TopPerformerDto>> GetTopPerformersAsync(int periodId, int top = 10)
    {
        var results = await _evaluationResultRepository.GetByPeriodAsync(periodId);

        return results
            .OrderByDescending(r => r.FinalScore)
            .Take(top)
            .Select(r => new TopPerformerDto
            {
                EmployeeId = r.HODEvaluation.SelfEvaluation.EmployeeId,
                EmployeeName = $"{r.HODEvaluation.SelfEvaluation.Employee.FirstName} {r.HODEvaluation.SelfEvaluation.Employee.LastName}",
                DepartmentName = r.HODEvaluation.SelfEvaluation.Employee.Department?.DepartmentName ?? "-",
                FinalScore = r.FinalScore,
                Grade = r.Grade
            })
            .ToList();
    }

    public async Task<List<RecommendationCandidateDto>> GetPromotionCandidatesAsync(int periodId)
    {
        return await GetCandidatesByRecommendationTypeAsync(periodId, "Promotion");
    }

    public async Task<List<RecommendationCandidateDto>> GetSalaryIncrementCandidatesAsync(int periodId)
    {
        return await GetCandidatesByRecommendationTypeAsync(periodId, "Salary Increment");
    }

    public async Task<byte[]> ExportEmployeeReportPdfAsync(int resultId)
    {
        var report = await BuildEmployeeReportAsync(resultId);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text("Employee Evaluation Report").FontSize(18).Bold();

                page.Content().Column(col =>
                {
                    col.Spacing(6);
                    col.Item().Text($"Employee: {report.EmployeeName}");
                    col.Item().Text($"Department: {report.DepartmentName}");
                    col.Item().Text($"Designation: {report.DesignationName}");
                    col.Item().Text($"Evaluation Period: {report.PeriodName}");
                    col.Item().LineHorizontal(1);

                    col.Item().Text($"KPI Score (80%): {report.KPIScore}");
                    col.Item().Text($"Competency Score (10%): {report.CompetencyScore}");
                    col.Item().Text($"Achievement Score (10%): {report.AchievementScore}");
                    col.Item().Text($"Attendance Penalty: -{report.AttendancePenalty}");
                    col.Item().LineHorizontal(1);
                    col.Item().Text($"Final Score: {report.FinalScore}").Bold();
                    col.Item().Text($"Grade: {report.Grade}").Bold();

                    col.Item().PaddingTop(10).Text("Recommendations:").Bold();
                    if (report.Recommendations.Count == 0)
                    {
                        col.Item().Text("None recorded.");
                    }
                    else
                    {
                        foreach (var rec in report.Recommendations)
                        {
                            col.Item().Text($"- {rec}");
                        }
                    }

                    col.Item().PaddingTop(10).Text($"Manager Decision: {report.ManagerDecision ?? "Pending"}").Bold();
                    if (!string.IsNullOrWhiteSpace(report.ManagerComments))
                    {
                        col.Item().Text($"Manager Comments: {report.ManagerComments}");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Generated on ");
                    x.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'"));
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportEmployeeReportExcelAsync(int resultId)
    {
        var report = await BuildEmployeeReportAsync(resultId);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Evaluation Report");

        var rows = new (string Label, string Value)[]
        {
            ("Employee", report.EmployeeName),
            ("Department", report.DepartmentName),
            ("Designation", report.DesignationName),
            ("Evaluation Period", report.PeriodName),
            ("KPI Score (80%)", report.KPIScore.ToString()),
            ("Competency Score (10%)", report.CompetencyScore.ToString()),
            ("Achievement Score (10%)", report.AchievementScore.ToString()),
            ("Attendance Penalty", $"-{report.AttendancePenalty}"),
            ("Final Score", report.FinalScore.ToString()),
            ("Grade", report.Grade),
            ("Recommendations", string.Join("; ", report.Recommendations)),
            ("Manager Decision", report.ManagerDecision ?? "Pending"),
            ("Manager Comments", report.ManagerComments ?? "")
        };

        for (var i = 0; i < rows.Length; i++)
        {
            sheet.Cell(i + 1, 1).Value = rows[i].Label;
            sheet.Cell(i + 1, 1).Style.Font.Bold = true;
            sheet.Cell(i + 1, 2).Value = rows[i].Value;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    // ---------- private helpers ----------

    private async Task<EmployeeReportDto> BuildEmployeeReportAsync(int resultId)
    {
        var result = await _evaluationResultRepository.GetByIdWithDetailsAsync(resultId)
            ?? throw new NotFoundException("Evaluation Result", resultId);

        var employee = result.HODEvaluation.SelfEvaluation.Employee;
        var recommendations = await _recommendationRepository.GetByResultAsync(resultId);
        var managerReview = await _managerReviewRepository.GetByResultAsync(resultId);
        var department = await _departmentRepository.DepartmentNameById(Convert.ToInt32(employee.DepartmentId));
        var assignment = _kPIAssignmentRepository.GetByIdWithDetailsAsync(result.HODEvaluation.SelfEvaluation.AssignmentId);
        //var periodName = assignment?.Period.Name ?? "_";
        Console.WriteLine(assignment);

        return new EmployeeReportDto
        {
            EmployeeId = employee.UserId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            DepartmentName = department ?? "-",
            DesignationName = employee.Designation?.DesignationName ?? "-",






            PeriodName = result.HODEvaluation.SelfEvaluation.Assignment.Period.Name ?? "_",
            KPIScore = result.KPIScore,
            CompetencyScore = result.CompetencyScore,
            AchievementScore = result.AchievementScore,
            AttendancePenalty = result.AttendancePenalty,
            FinalScore = result.FinalScore,
            Grade = result.Grade,
            Recommendations = recommendations.Select(r => $"{r.RecommendationType}{(string.IsNullOrWhiteSpace(r.RecommendationNote) ? "" : $" - {r.RecommendationNote}")}").ToList(),
            ManagerDecision = managerReview?.Decision ?? "_",
            ManagerComments = managerReview?.Comments ?? "_"
        };
    }

    private async Task<List<RecommendationCandidateDto>> GetCandidatesByRecommendationTypeAsync(int periodId, string recommendationType)
    {
        var results = await _evaluationResultRepository.GetByPeriodAsync(periodId);
        var candidates = new List<RecommendationCandidateDto>();

        foreach (var result in results)
        {
            var recommendations = await _recommendationRepository.GetByResultAsync(result.ResultId);
            var match = recommendations.FirstOrDefault(r => string.Equals(r.RecommendationType, recommendationType, StringComparison.OrdinalIgnoreCase));

            if (match is not null)
            {
                var employee = result.HODEvaluation.SelfEvaluation.Employee;
                candidates.Add(new RecommendationCandidateDto
                {
                    EmployeeId = employee.UserId,
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    DepartmentName = employee.Department?.DepartmentName ?? "-",
                    FinalScore = result.FinalScore,
                    Grade = result.Grade,
                    RecommendationNote = match.RecommendationNote
                });
            }
        }

        return candidates.OrderByDescending(c => c.FinalScore).ToList();
    }
}
