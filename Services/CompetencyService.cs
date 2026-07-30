using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Admin-only CRUD for the master list of the 10 standard competencies
/// (Customer Focus, Team Work, Decision Making, Initiative, Innovation, Planning,
/// Communication, Drive & Flexibility, Professionalism, Leadership).
/// </summary>
public class CompetencyService : ICompetencyService
{
    private readonly ICompetencyRepository _competencyRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CompetencyService(
        ICompetencyRepository competencyRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _competencyRepository = competencyRepository;
        _auditLogRepository = auditLogRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<List<CompetencyDto>> GetAllAsync()
    {
        var competencies = await _competencyRepository.ListAllAsync();
        return _mapper.Map<List<CompetencyDto>>(competencies);
    }

    public async Task<CompetencyDto> CreateAsync(CreateCompetencyDto request)
    {
        EnsureCallerIsAdmin();

        if (await _competencyRepository.ExistsByNameAsync(request.Name))
        {
            throw new ConflictException($"A competency named '{request.Name}' already exists.");
        }

        var competency = _mapper.Map<Competency>(request);
        await _competencyRepository.AddAsync(competency);
        await LogAuditAsync("Create", "Competencies", competency.CompetencyId);
        await _competencyRepository.SaveChangesAsync();

        return _mapper.Map<CompetencyDto>(competency);
    }

    public async Task<CompetencyDto> UpdateAsync(int competencyId, UpdateCompetencyDto request)
    {
        EnsureCallerIsAdmin();

        var competency = await _competencyRepository.GetByIdAsync(competencyId)
            ?? throw new NotFoundException("Competency", competencyId);

        if (await _competencyRepository.ExistsByNameAsync(request.Name, excludeCompetencyId: competencyId))
        {
            throw new ConflictException($"A competency named '{request.Name}' already exists.");
        }

        _mapper.Map(request, competency);
        await _competencyRepository.UpdateAsync(competency);
        await LogAuditAsync("Update", "Competencies", competencyId);
        await _competencyRepository.SaveChangesAsync();

        return _mapper.Map<CompetencyDto>(competency);
    }

    public async Task DeleteAsync(int competencyId)
    {
        EnsureCallerIsAdmin();

        var competency = await _competencyRepository.GetByIdAsync(competencyId)
            ?? throw new NotFoundException("Competency", competencyId);

        if (await _competencyRepository.IsInUseAsync(competencyId))
        {
            throw new ConflictException("This competency has evaluation history and cannot be deleted.");
        }

        await _competencyRepository.DeleteAsync(competency);
        await LogAuditAsync("Delete", "Competencies", competencyId);
        await _competencyRepository.SaveChangesAsync();
    }

    private void EnsureCallerIsAdmin()
    {
        if (!_currentUserService.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Only Admin can manage the competency list.");
        }
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
