using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Implements HOD Review (SRS Step 8): the HOD views the employee's self-evaluation marks
/// and (where applicable) the Supervisor's marks, enters their own marks/comments per KPI,
/// and submits. Supports both hierarchy shapes: Employee -> HOD directly (ReportsTo == HOD),
/// and Employee -> Supervisor -> HOD (ReportsTo chain), in which case the Supervisor review
/// must already be Submitted before HOD review may begin.
/// </summary>
public class HodEvaluationService : IHodEvaluationService
{
    private const string StatusDraft = "Draft";
    private const string StatusSubmitted = "Submitted";

    private readonly IHODEvaluationRepository _hodEvaluationRepository;
    private readonly IEmployeeEvaluationRepository _employeeEvaluationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public HodEvaluationService(
        IHODEvaluationRepository hodEvaluationRepository,
        IEmployeeEvaluationRepository employeeEvaluationRepository,
        IAuditLogRepository auditLogRepository,
        IMapper mapper)
    {
        _hodEvaluationRepository = hodEvaluationRepository;
        _employeeEvaluationRepository = employeeEvaluationRepository;
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<List<PendingReviewDto>> GetPendingAsync(int hodId)
    {
        var pending = await _employeeEvaluationRepository.GetPendingForHodAsync(hodId);

        return pending.Select(e => new PendingReviewDto
        {
            SelfEvaluationId = e.SelfEvaluationId,
            AssignmentId = e.AssignmentId,
            EmployeeId = e.EmployeeId,
            EmployeeName = $"{e.Employee.FirstName} {e.Employee.LastName}",
            PeriodName = e.Assignment.Period.Name,
            SubmittedDate = e.SubmittedDate
        }).ToList();
    }

    public async Task<HodEvaluationDto> GetByIdAsync(int callerId, string callerRole, int hodEvaluationId)
    {
        var evaluation = await _hodEvaluationRepository.GetByIdWithDetailsAsync(hodEvaluationId)
            ?? throw new NotFoundException("HOD Evaluation", hodEvaluationId);

        var allowed = callerRole == RoleNames.Admin
            || evaluation.HODId == callerId
            || evaluation.SelfEvaluation.EmployeeId == callerId;

        if (!allowed)
        {
            throw new ForbiddenException("You are not authorized to view this HOD evaluation.");
        }

        return MapWithMarks(evaluation);
    }

    public async Task<HodEvaluationDto> CreateAsync(int hodId, CreateHodEvaluationDto request)
    {
        var selfEvaluation = await _employeeEvaluationRepository.GetByIdWithDetailsAsync(request.SelfEvaluationId)
            ?? throw new NotFoundException("Employee Evaluation", request.SelfEvaluationId);

        if (selfEvaluation.Status != StatusSubmitted)
        {
            throw new ValidationAppException("The employee's self-evaluation must be Submitted before HOD review can begin.");
        }

        await ValidateHodOwnsEmployeeAsync(hodId, selfEvaluation);

        var existing = await _hodEvaluationRepository.GetBySelfEvaluationAsync(request.SelfEvaluationId);
        if (existing is not null)
        {
            throw new ConflictException("An HOD evaluation already exists for this self-evaluation.");
        }

        var validDetailIds = selfEvaluation.Details.Select(d => d.KPIAssignmentDetailId).ToHashSet();
        ValidateDetailsAgainstSelfEvaluation(validDetailIds, request.Details);

        var evaluation = new HODEvaluation
        {
            SelfEvaluationId = request.SelfEvaluationId,
            HODId = hodId,
            Status = StatusDraft,
            Details = request.Details.Select(d => new HODEvaluationDetail
            {
                KPIAssignmentDetailId = d.KPIAssignmentDetailId,
                Marks = d.Marks,
                Comments = d.Comments
            }).ToList()
        };

        await _hodEvaluationRepository.AddAsync(evaluation);
        await LogAuditAsync(hodId, "Create", "HODEvaluations", evaluation.HODEvaluationId);
        await _hodEvaluationRepository.SaveChangesAsync();

        var created = await _hodEvaluationRepository.GetByIdWithDetailsAsync(evaluation.HODEvaluationId);
        return MapWithMarks(created!);
    }

    public async Task<HodEvaluationDto> UpdateAsync(int hodId, int hodEvaluationId, UpdateHodEvaluationDto request)
    {
        var evaluation = await _hodEvaluationRepository.GetByIdWithDetailsAsync(hodEvaluationId)
            ?? throw new NotFoundException("HOD Evaluation", hodEvaluationId);

        if (evaluation.HODId != hodId)
        {
            throw new ForbiddenException("You can only edit your own HOD evaluation.");
        }

        if (evaluation.Status != StatusDraft)
        {
            throw new ConflictException("This HOD evaluation has already been submitted and can no longer be edited.");
        }

        var validDetailIds = evaluation.SelfEvaluation.Details.Select(d => d.KPIAssignmentDetailId).ToHashSet();
        ValidateDetailsAgainstSelfEvaluation(validDetailIds, request.Details);

        var newDetails = request.Details.Select(d => new HODEvaluationDetail
        {
            KPIAssignmentDetailId = d.KPIAssignmentDetailId,
            Marks = d.Marks,
            Comments = d.Comments
        }).ToList();

        await _hodEvaluationRepository.ReplaceDetailsAsync(hodEvaluationId, newDetails);
        await LogAuditAsync(hodId, "Update", "HODEvaluations", hodEvaluationId);
        await _hodEvaluationRepository.SaveChangesAsync();

        var updated = await _hodEvaluationRepository.GetByIdWithDetailsAsync(hodEvaluationId);
        return MapWithMarks(updated!);
    }

    public async Task<HodEvaluationDto> SubmitAsync(int hodId, int hodEvaluationId)
    {
        var evaluation = await _hodEvaluationRepository.GetByIdWithDetailsAsync(hodEvaluationId)
            ?? throw new NotFoundException("HOD Evaluation", hodEvaluationId);

        if (evaluation.HODId != hodId)
        {
            throw new ForbiddenException("You can only submit your own HOD evaluation.");
        }

        if (evaluation.Status != StatusDraft)
        {
            throw new ConflictException("This HOD evaluation has already been submitted.");
        }

        evaluation.Status = StatusSubmitted;
        evaluation.SubmittedDate = DateTime.UtcNow;

        await _hodEvaluationRepository.UpdateAsync(evaluation);
        await LogAuditAsync(hodId, "Evaluation Submit", "HODEvaluations", hodEvaluationId);
        await _hodEvaluationRepository.SaveChangesAsync();

        var result = await _hodEvaluationRepository.GetByIdWithDetailsAsync(hodEvaluationId);
        return MapWithMarks(result!);
    }

    // ---------- private helpers ----------

    /// <summary>
    /// Validates the HOD is entitled to review this employee: either the employee reports
    /// directly to the HOD (no Supervisor layer), or the employee's Supervisor reports to
    /// this HOD and has already submitted their Supervisor review.
    /// </summary>
    private async Task ValidateHodOwnsEmployeeAsync(int hodId, EmployeeEvaluation selfEvaluation)
    {
        var employee = selfEvaluation.Employee;

        if (employee.ReportsTo == hodId)
        {
            return;
        }

        if (employee.Manager?.ReportsTo == hodId)
        {
            var supervisorSubmitted = selfEvaluation.SupervisorEvaluations.Any(s => s.Status == "Submitted");
            if (!supervisorSubmitted)
            {
                throw new ValidationAppException("The Supervisor review must be Submitted before HOD review can begin for this employee.");
            }

            return;
        }

        throw new ForbiddenException("This employee is not within your reporting hierarchy.");
    }

    private static void ValidateDetailsAgainstSelfEvaluation(HashSet<int> validDetailIds, List<HodEvaluationDetailInputDto> submittedDetails)
    {
        foreach (var detail in submittedDetails)
        {
            if (!validDetailIds.Contains(detail.KPIAssignmentDetailId))
            {
                throw new ValidationAppException($"KPI line item {detail.KPIAssignmentDetailId} does not belong to this evaluation.");
            }
        }

        if (submittedDetails.Select(d => d.KPIAssignmentDetailId).Distinct().Count() != validDetailIds.Count)
        {
            throw new ValidationAppException("All KPI line items must be marked before saving.");
        }
    }

    private static HodEvaluationDto MapWithMarks(HODEvaluation evaluation)
    {
        var employeeMarksByDetail = evaluation.SelfEvaluation.Details.ToDictionary(d => d.KPIAssignmentDetailId, d => d.EmployeeMarks);

        var supervisorEvaluation = evaluation.SelfEvaluation.SupervisorEvaluations.FirstOrDefault(s => s.Status == "Submitted");
        var supervisorMarksByDetail = supervisorEvaluation?.Details.ToDictionary(d => d.KPIAssignmentDetailId, d => d.Marks)
            ?? new Dictionary<int, decimal>();

        return new HodEvaluationDto
        {
            HODEvaluationId = evaluation.HODEvaluationId,
            SelfEvaluationId = evaluation.SelfEvaluationId,
            EmployeeId = evaluation.SelfEvaluation.EmployeeId,
            EmployeeName = $"{evaluation.SelfEvaluation.Employee.FirstName} {evaluation.SelfEvaluation.Employee.LastName}",
            HODId = evaluation.HODId,
            HODName = $"{evaluation.HOD.FirstName} {evaluation.HOD.LastName}",
            SubmittedDate = evaluation.SubmittedDate,
            Status = evaluation.Status,
            Details = evaluation.Details.Select(d => new HodEvaluationDetailDto
            {
                DetailId = d.DetailId,
                KPIAssignmentDetailId = d.KPIAssignmentDetailId,
                KPIName = d.KpiAssignmentDetail.KPIName,
                WeightPercentage = d.KpiAssignmentDetail.WeightPercentage,
                EmployeeMarks = employeeMarksByDetail.GetValueOrDefault(d.KPIAssignmentDetailId),
                SupervisorMarks = supervisorMarksByDetail.TryGetValue(d.KPIAssignmentDetailId, out var sm) ? sm : null,
                Marks = d.Marks,
                Comments = d.Comments
            }).ToList()
        };
    }

    private async Task LogAuditAsync(int userId, string action, string tableName, int? recordId)
    {
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = action,
            TableName = tableName,
            RecordId = recordId,
            CreatedDate = DateTime.UtcNow
        });
    }
}
