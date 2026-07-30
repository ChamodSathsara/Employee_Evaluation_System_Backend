using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>Admin-only CRUD for designations.</summary>
public class DesignationService : IDesignationService
{
    private readonly IDesignationRepository _designationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public DesignationService(
        IDesignationRepository designationRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _designationRepository = designationRepository;
        _auditLogRepository = auditLogRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<List<DesignationDto>> GetAllAsync()
    {
        var designations = await _designationRepository.ListAllAsync();
        var dtos = _mapper.Map<List<DesignationDto>>(designations);

        foreach (var dto in dtos)
        {
            dto.UserCount = await _designationRepository.CountUsersAsync(dto.DesignationId);
        }

        return dtos;
    }

    public async Task<DesignationDto> GetByIdAsync(int designationId)
    {
        var designation = await _designationRepository.GetByIdAsync(designationId)
            ?? throw new NotFoundException("Designation", designationId);

        var dto = _mapper.Map<DesignationDto>(designation);
        dto.UserCount = await _designationRepository.CountUsersAsync(designationId);
        return dto;
    }

    public async Task<DesignationDto> CreateAsync(CreateDesignationDto request)
    {
        EnsureCallerIsAdmin();

        if (await _designationRepository.ExistsByNameAsync(request.DesignationName))
        {
            throw new ConflictException($"A designation named '{request.DesignationName}' already exists.");
        }

        var designation = _mapper.Map<Designation>(request);
        await _designationRepository.AddAsync(designation);
        await LogAuditAsync("Create", "Designations", designation.DesignationId);
        await _designationRepository.SaveChangesAsync();

        return _mapper.Map<DesignationDto>(designation);
    }

    public async Task<DesignationDto> UpdateAsync(int designationId, UpdateDesignationDto request)
    {
        EnsureCallerIsAdmin();

        var designation = await _designationRepository.GetByIdAsync(designationId)
            ?? throw new NotFoundException("Designation", designationId);

        if (await _designationRepository.ExistsByNameAsync(request.DesignationName, excludeDesignationId: designationId))
        {
            throw new ConflictException($"A designation named '{request.DesignationName}' already exists.");
        }

        _mapper.Map(request, designation);
        await _designationRepository.UpdateAsync(designation);
        await LogAuditAsync("Update", "Designations", designationId);
        await _designationRepository.SaveChangesAsync();

        return _mapper.Map<DesignationDto>(designation);
    }

    public async Task DeleteAsync(int designationId)
    {
        EnsureCallerIsAdmin();

        var designation = await _designationRepository.GetByIdAsync(designationId)
            ?? throw new NotFoundException("Designation", designationId);

        var userCount = await _designationRepository.CountUsersAsync(designationId);
        if (userCount > 0)
        {
            throw new ConflictException($"This designation has {userCount} user(s) assigned and cannot be deleted.");
        }

        await _designationRepository.DeleteAsync(designation);
        await LogAuditAsync("Delete", "Designations", designationId);
        await _designationRepository.SaveChangesAsync();
    }

    private void EnsureCallerIsAdmin()
    {
        if (!_currentUserService.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Only Admin can manage designations.");
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
