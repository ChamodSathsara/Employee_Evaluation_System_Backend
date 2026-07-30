using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Composes data already exposed by the other module services into a single payload per
/// role-specific landing dashboard (SRS API section 20). This is an aggregator on top of
/// the Service layer (not the Repository layer directly), so each individual data point
/// still goes through its owning module's business rules and authorization checks.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IKpiAssignmentService _kpiAssignmentService;
    private readonly ISelfEvaluationService _selfEvaluationService;
    private readonly IGradeCalculationService _gradeCalculationService;
    private readonly ISupervisorEvaluationService _supervisorEvaluationService;
    private readonly IHodEvaluationService _hodEvaluationService;
    private readonly IManagerReviewService _managerReviewService;
    private readonly IReportService _reportService;
    private readonly IUserService _userService;
    private readonly IDepartmentService _departmentService;
    private readonly IEvaluationPeriodService _evaluationPeriodService;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;
    private readonly IEvaluationPeriodRepository _evaluationPeriodRepository;
    private readonly IUserRepository _userRepository;

    public DashboardService(
        IKpiAssignmentService kpiAssignmentService,
        ISelfEvaluationService selfEvaluationService,
        IGradeCalculationService gradeCalculationService,
        ISupervisorEvaluationService supervisorEvaluationService,
        IHodEvaluationService hodEvaluationService,
        IManagerReviewService managerReviewService,
        IReportService reportService,
        IUserService userService,
        IDepartmentService departmentService,
        IEvaluationPeriodService evaluationPeriodService,
        INotificationService notificationService,
        IAuditLogService auditLogService,
        IEvaluationPeriodRepository evaluationPeriodRepository,
        IUserRepository userRepository)
    {
        _kpiAssignmentService = kpiAssignmentService;
        _selfEvaluationService = selfEvaluationService;
        _gradeCalculationService = gradeCalculationService;
        _supervisorEvaluationService = supervisorEvaluationService;
        _hodEvaluationService = hodEvaluationService;
        _managerReviewService = managerReviewService;
        _reportService = reportService;
        _userService = userService;
        _departmentService = departmentService;
        _evaluationPeriodService = evaluationPeriodService;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
        _evaluationPeriodRepository = evaluationPeriodRepository;
        _userRepository = userRepository;
    }

    public async Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int employeeId)
    {
        var myKpis = await _kpiAssignmentService.GetMyKpisAsync(employeeId);
        var notifications = await _notificationService.GetUnreadAsync(employeeId);

        var hasPendingSelfEvaluation = false;
        foreach (var kpi in myKpis)
        {
            var evaluation = await _selfEvaluationService.GetByAssignmentAsync(employeeId, "Employee", kpi.AssignmentId);
            if (evaluation is null || evaluation.Status == "Draft")
            {
                hasPendingSelfEvaluation = true;
                break;
            }
        }

        EvaluationResultDto? latestResult = null;
        var results = await _gradeCalculationService.GetByEmployeeAsync(employeeId);
        latestResult = results.OrderByDescending(r => r.ProcessedDate).FirstOrDefault();

        return new EmployeeDashboardDto
        {
            MyKpis = myKpis,
            HasPendingSelfEvaluation = hasPendingSelfEvaluation,
            LatestResult = latestResult,
            Notifications = notifications
        };
    }

    public async Task<SupervisorDashboardDto> GetSupervisorDashboardAsync(int supervisorId)
    {
        var employees = await _userService.GetDirectReportsAsync(supervisorId);
        var pendingReviews = await _supervisorEvaluationService.GetPendingAsync(supervisorId);
        var notifications = await _notificationService.GetUnreadAsync(supervisorId);

        return new SupervisorDashboardDto
        {
            Employees = employees,
            PendingReviews = pendingReviews,
            Notifications = notifications
        };
    }

    public async Task<HodDashboardDto> GetHodDashboardAsync(int hodId)
    {
        var departmentEmployees = await _userService.GetDirectReportsAsync(hodId);
        var pendingReviews = await _hodEvaluationService.GetPendingAsync(hodId);
        var notifications = await _notificationService.GetUnreadAsync(hodId);

        var activePeriod = await _evaluationPeriodRepository.GetActiveAsync();
        var kpiStatus = activePeriod is not null
            ? await _kpiAssignmentService.GetByPeriodAsync(hodId, "HOD", activePeriod.PeriodId)
            : new List<KpiAssignmentDto>();

        return new HodDashboardDto
        {
            DepartmentEmployees = departmentEmployees,
            PendingReviews = pendingReviews,
            KpiStatus = kpiStatus,
            Notifications = notifications
        };
    }

    public async Task<ManagerDashboardDto> GetManagerDashboardAsync(int managerId)
    {
        var pendingApprovals = await _managerReviewService.GetPendingAsync(managerId);
        var notifications = await _notificationService.GetUnreadAsync(managerId);

        var activePeriod = await _evaluationPeriodRepository.GetActiveAsync();
        var gradeDistribution = new Dictionary<string, int>();
        if (activePeriod is not null)
        {
            var summary = await _reportService.GetSummaryAsync(activePeriod.PeriodId);
            gradeDistribution = summary.GradeDistribution;
        }

        return new ManagerDashboardDto
        {
            PendingApprovals = pendingApprovals,
            HodEvaluations = new List<HodEvaluationDto>(),
            GradeDistribution = gradeDistribution,
            Notifications = notifications
        };
    }

    public async Task<CeoDashboardDto> GetCeoDashboardAsync(int callerId)
    {
        var activePeriod = await _evaluationPeriodRepository.GetActiveAsync();

        var managerPerformance = new List<TopPerformerDto>();
        var promotionSummary = new List<RecommendationCandidateDto>();
        if (activePeriod is not null)
        {
            managerPerformance = await _reportService.GetTopPerformersAsync(activePeriod.PeriodId, 10);
            promotionSummary = await _reportService.GetPromotionCandidatesAsync(activePeriod.PeriodId);
        }

        var allUsers = await _userRepository.ListAllAsync();
        var departments = await _departmentService.GetAllAsync();
        var notifications = await _notificationService.GetUnreadAsync(callerId);

        return new CeoDashboardDto
        {
            ManagerPerformance = managerPerformance,
            PromotionSummary = promotionSummary,
            TotalActiveUsers = allUsers.Count(u => u.IsActive),
            TotalDepartments = departments.Count,
            Notifications = notifications
        };
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var allUsers = await _userRepository.ListAllAsync();
        var departments = await _departmentService.GetAllAsync();
        var periods = await _evaluationPeriodService.GetAllAsync();
        var activePeriod = periods.FirstOrDefault(p => p.Status == "Active");

        var recentLogs = await _auditLogService.SearchAsync(new AuditLogSearchDto { PageNumber = 1, PageSize = 20 });

        return new AdminDashboardDto
        {
            TotalUsers = allUsers.Count,
            ActiveUsers = allUsers.Count(u => u.IsActive),
            TotalDepartments = departments.Count,
            ActiveEvaluationPeriod = activePeriod,
            RecentAuditLogs = recentLogs.Items
        };
    }
}
