using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Implements Employee Self Evaluation (SRS Step 6 / Module 2): an employee scores every
/// KPI line item on their Published KPI assignment (0-100), may save as Draft repeatedly,
/// and Submit locks the record and notifies the next reviewer (Supervisor if the employee
/// has one, otherwise the HOD directly).
/// </summary>
public class SelfEvaluationService : ISelfEvaluationService
{
    private const string StatusDraft = "Draft";
    private const string StatusSubmitted = "Submitted";

    private readonly IEmployeeEvaluationRepository _employeeEvaluationRepository;
    private readonly IKPIAssignmentRepository _kpiAssignmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public SelfEvaluationService(
        IEmployeeEvaluationRepository employeeEvaluationRepository,
        IKPIAssignmentRepository kpiAssignmentRepository,
        IUserRepository userRepository,
        INotificationRepository notificationRepository,
        IAuditLogRepository auditLogRepository,
        IMapper mapper)
    {
        _employeeEvaluationRepository = employeeEvaluationRepository;
        _kpiAssignmentRepository = kpiAssignmentRepository;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<EmployeeEvaluationDto> GetMyAsync(int employeeId, int assignmentId)
    {
        var evaluation = await _employeeEvaluationRepository.GetByAssignmentAndEmployeeAsync(assignmentId, employeeId);
        if (evaluation is null)
        {
            throw new NotFoundException("A self-evaluation for this KPI assignment has not been started yet.");
        }

        var full = await _employeeEvaluationRepository.GetByIdWithDetailsAsync(evaluation.SelfEvaluationId);
        return _mapper.Map<EmployeeEvaluationDto>(full!);
    }

    public async Task<List<EmployeeEvaluationDto>> GetMyHistoryAsync(int employeeId)
    {
        var history = await _employeeEvaluationRepository.GetHistoryForEmployeeAsync(employeeId);
        return _mapper.Map<List<EmployeeEvaluationDto>>(history);
    }

    public async Task<EmployeeEvaluationDto?> GetByAssignmentAsync(int callerId, string callerRole, int assignmentId)
    {
        var assignment = await _kpiAssignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("KPI Assignment", assignmentId);

        var allowed = callerRole == RoleNames.Admin || assignment.EmployeeId == callerId || assignment.CreatedBy == callerId ;
        //if (!allowed)
        //{
        //    throw new ForbiddenException("You are not authorized to view this self-evaluation.");
        //}

        var evaluation = await _employeeEvaluationRepository.GetByAssignmentAndEmployeeAsync(assignmentId, assignment.EmployeeId);
        if (evaluation is null)
        {
            return null;
        }

        var full = await _employeeEvaluationRepository.GetByIdWithDetailsAsync(evaluation.SelfEvaluationId);
        return _mapper.Map<EmployeeEvaluationDto>(full!);
    }

    public async Task<EmployeeEvaluationDto> SaveDraftAsync(int employeeId, SaveEmployeeEvaluationDto request)
    {
        var assignment = await ValidateAssignmentForSelfEvaluationAsync(employeeId, request.AssignmentId);

        var evaluation = await _employeeEvaluationRepository.GetByAssignmentAndEmployeeAsync(request.AssignmentId, employeeId);

        if (evaluation is not null && evaluation.Status != StatusDraft)
        {
            throw new ConflictException("This self-evaluation has already been submitted and can no longer be edited.");
        }

        ValidateDetailsAgainstAssignment(assignment.Details.Select(d => d.DetailId).ToHashSet(), request.Details);

        if (evaluation is null)
        {
            evaluation = new EmployeeEvaluation
            {
                AssignmentId = request.AssignmentId,
                EmployeeId = employeeId,
                Status = StatusDraft,
                Details = request.Details.Select(d => new EmployeeEvaluationDetail
                {
                    KPIAssignmentDetailId = d.KPIAssignmentDetailId,
                    EmployeeMarks = d.EmployeeMarks,
                    Comments = d.Comments
                }).ToList()
            };
            await _employeeEvaluationRepository.AddAsync(evaluation);
        }
        else
        {
            foreach (var detail in request.Details)
            {
                var existingDetail = evaluation.Details.FirstOrDefault(d => d.KPIAssignmentDetailId == detail.KPIAssignmentDetailId);
                if (existingDetail is not null)
                {
                    existingDetail.EmployeeMarks = detail.EmployeeMarks;
                    existingDetail.Comments = detail.Comments;
                }
                else
                {
                    evaluation.Details.Add(new EmployeeEvaluationDetail
                    {
                        SelfEvaluationId = evaluation.SelfEvaluationId,
                        KPIAssignmentDetailId = detail.KPIAssignmentDetailId,
                        EmployeeMarks = detail.EmployeeMarks,
                        Comments = detail.Comments
                    });
                }
            }

            await _employeeEvaluationRepository.UpdateAsync(evaluation);
        }

        await LogAuditAsync(employeeId, "Update", "EmployeeEvaluations", evaluation.SelfEvaluationId);
        await _employeeEvaluationRepository.SaveChangesAsync();

        var saved = await _employeeEvaluationRepository.GetByIdWithDetailsAsync(evaluation.SelfEvaluationId);
        return _mapper.Map<EmployeeEvaluationDto>(saved!);
    }

    public async Task<EmployeeEvaluationDto> SubmitAsync(int employeeId, SaveEmployeeEvaluationDto request)
    {
        var saved = await SaveDraftAsync(employeeId, request);

        var evaluation = await _employeeEvaluationRepository.GetByIdAsync(saved.SelfEvaluationId)
            ?? throw new NotFoundException("Employee Evaluation", saved.SelfEvaluationId);

        evaluation.Status = StatusSubmitted;
        evaluation.SubmittedDate = DateTime.UtcNow;
        await _employeeEvaluationRepository.UpdateAsync(evaluation);

        var employee = await _userRepository.GetByIdWithDetailsAsync(employeeId);
        var nextReviewerId = employee?.ReportedSupervisorId ?? employee?.ReportedHodId;

        if (nextReviewerId.HasValue)
        {
            await _notificationRepository.AddAsync(new Notification
            {
                UserId = nextReviewerId.Value,
                Title = "Self-Evaluation Submitted",
                Message = $"{employee!.FirstName} {employee.LastName} has submitted their self-evaluation and it is awaiting your review.",
                IsRead = false
            });
        }

        await LogAuditAsync(employeeId, "Evaluation Submit", "EmployeeEvaluations", evaluation.SelfEvaluationId);
        await _employeeEvaluationRepository.SaveChangesAsync();

        var result = await _employeeEvaluationRepository.GetByIdWithDetailsAsync(evaluation.SelfEvaluationId);
        return _mapper.Map<EmployeeEvaluationDto>(result!);
    }

    public async Task<EmployeeEvaluationDto> UpdateDraftAsync(int employeeId, int selfEvaluationId, SaveEmployeeEvaluationDto request)
    {
        var evaluation = await _employeeEvaluationRepository.GetByIdAsync(selfEvaluationId)
            ?? throw new NotFoundException("Employee Evaluation", selfEvaluationId);

        if (evaluation.EmployeeId != employeeId)
        {
            throw new ForbiddenException("You can only edit your own self-evaluation.");
        }

        if (evaluation.Status != StatusDraft)
        {
            throw new ConflictException("This self-evaluation has already been submitted and can no longer be edited.");
        }

        return await SaveDraftAsync(employeeId, request);
    }

    // ---------- private helpers ----------

    private async Task<KPIAssignment> ValidateAssignmentForSelfEvaluationAsync(int employeeId, int assignmentId)
    {
        var assignment = await _kpiAssignmentRepository.GetByIdWithDetailsAsync(assignmentId)
            ?? throw new NotFoundException("KPI Assignment", assignmentId);

        if (assignment.EmployeeId != employeeId)
        {
            throw new ForbiddenException("This KPI assignment does not belong to you.");
        }

        if (assignment.Status != "Published")
        {
            throw new ValidationAppException("Self-evaluation can only be performed on a Published KPI assignment.");
        }

        return assignment;
    }

    private static void ValidateDetailsAgainstAssignment(HashSet<int> validDetailIds, List<SaveEmployeeEvaluationDetailDto> submittedDetails)
    {
        foreach (var detail in submittedDetails)
        {
            if (!validDetailIds.Contains(detail.KPIAssignmentDetailId))
            {
                throw new ValidationAppException($"KPI line item {detail.KPIAssignmentDetailId} does not belong to this KPI assignment.");
            }
        }

        if (submittedDetails.Select(d => d.KPIAssignmentDetailId).Distinct().Count() != validDetailIds.Count)
        {
            throw new ValidationAppException("All KPI line items on the assignment must be marked before saving.");
        }
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
