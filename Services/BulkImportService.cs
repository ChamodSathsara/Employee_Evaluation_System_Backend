using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
using EmployeeEvaluationSystem.Data;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Services;

public class BulkImportService : IBulkImportService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;

    public BulkImportService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public async Task<BulkImportResultDto> ImportAsync(Stream excelStream)
    {
        if (!_currentUserService.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Only Admin can bulk import master data and users.");
        }

        using var workbook = new XLWorkbook(excelStream);
        var departmentSheet = RequireSheet(workbook, "Departments");
        var designationSheet = RequireSheet(workbook, "Designations");
        var userSheet = RequireSheet(workbook, "Users");

        var departmentRows = ReadRows(departmentSheet,
            "DepartmentName", "Description");
        var designationRows = ReadRows(designationSheet,
            "DesignationName");
        var userRows = ReadRows(userSheet,
            "EmployeeNo", "FirstName", "LastName", "Email", "Password", "RoleName",
            "DepartmentName", "DesignationName", "ReportsToEmployeeNo", "IsActive");

        var errors = new List<string>();
        var existingDepartments = await _context.Departments.ToListAsync();
        var existingDesignations = await _context.Designations.ToListAsync();
        var existingUsers = await _context.Users.Include(u => u.Role).ToListAsync();
        var roles = await _context.Roles.ToListAsync();

        var departments = existingDepartments.ToDictionary(
            d => d.DepartmentName, StringComparer.OrdinalIgnoreCase);
        var designations = existingDesignations.ToDictionary(
            d => d.DesignationName, StringComparer.OrdinalIgnoreCase);
        var rolesByName = roles.ToDictionary(r => r.RoleName, StringComparer.OrdinalIgnoreCase);
        var usersByEmployeeNo = existingUsers.ToDictionary(
            u => u.EmployeeNo, StringComparer.OrdinalIgnoreCase);
        var existingEmails = existingUsers.Select(u => u.Email)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newDepartments = new List<Department>();
        foreach (var row in departmentRows)
        {
            var name = row.Value("DepartmentName").Trim();
            var description = NullIfWhiteSpace(row.Value("Description"));
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add($"Departments row {row.RowNumber}: DepartmentName is required.");
                continue;
            }
            if (name.Length > 100 || (description?.Length ?? 0) > 255)
            {
                errors.Add($"Departments row {row.RowNumber}: field length exceeds the allowed limit.");
                continue;
            }
            if (departments.ContainsKey(name)) continue;

            var department = new Department { DepartmentName = name, Description = description };
            departments[name] = department;
            newDepartments.Add(department);
        }

        var newDesignations = new List<Designation>();
        foreach (var row in designationRows)
        {
            var name = row.Value("DesignationName").Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add($"Designations row {row.RowNumber}: DesignationName is required.");
                continue;
            }
            if (name.Length > 100)
            {
                errors.Add($"Designations row {row.RowNumber}: DesignationName exceeds 100 characters.");
                continue;
            }
            if (designations.ContainsKey(name)) continue;

            var designation = new Designation { DesignationName = name };
            designations[name] = designation;
            newDesignations.Add(designation);
        }

        var importedUserRows = new List<(ExcelRow Row, User User, string RoleName, string? ReportsToEmployeeNo)>();
        var importedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in userRows)
        {
            var employeeNo = row.Value("EmployeeNo").Trim();
            var firstName = row.Value("FirstName").Trim();
            var lastName = row.Value("LastName").Trim();
            var email = row.Value("Email").Trim();
            var password = row.Value("Password");
            var roleName = row.Value("RoleName").Trim();
            var departmentName = NullIfWhiteSpace(row.Value("DepartmentName"));
            var designationName = NullIfWhiteSpace(row.Value("DesignationName"));
            var reportsToEmployeeNo = NullIfWhiteSpace(row.Value("ReportsToEmployeeNo"));
            var isActiveText = NullIfWhiteSpace(row.Value("IsActive"));

            if (string.IsNullOrWhiteSpace(employeeNo) || string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(roleName))
            {
                errors.Add($"Users row {row.RowNumber}: EmployeeNo, FirstName, LastName, Email, Password and RoleName are required.");
                continue;
            }
            if (employeeNo.Length > 50 || firstName.Length > 100 || lastName.Length > 100 || email.Length > 150)
            {
                errors.Add($"Users row {row.RowNumber}: one or more fields exceed their allowed length.");
                continue;
            }
            if (!new EmailAddressAttribute().IsValid(email))
            {
                errors.Add($"Users row {row.RowNumber}: '{email}' is not a valid email address.");
                continue;
            }
            if (password.Length < 8)
            {
                errors.Add($"Users row {row.RowNumber}: Password must contain at least 8 characters.");
                continue;
            }
            if (!rolesByName.TryGetValue(roleName, out var role))
            {
                errors.Add($"Users row {row.RowNumber}: role '{roleName}' does not exist.");
                continue;
            }
            if (usersByEmployeeNo.ContainsKey(employeeNo))
            {
                errors.Add($"Users row {row.RowNumber}: employee number '{employeeNo}' already exists or is duplicated in the workbook.");
                continue;
            }
            if (existingEmails.Contains(email) || !importedEmails.Add(email))
            {
                errors.Add($"Users row {row.RowNumber}: email '{email}' already exists or is duplicated in the workbook.");
                continue;
            }

            Department? department = null;
            if (departmentName is not null && !departments.TryGetValue(departmentName, out department))
            {
                errors.Add($"Users row {row.RowNumber}: department '{departmentName}' was not found in the database or Departments sheet.");
                continue;
            }

            Designation? designation = null;
            if (designationName is not null && !designations.TryGetValue(designationName, out designation))
            {
                errors.Add($"Users row {row.RowNumber}: designation '{designationName}' was not found in the database or Designations sheet.");
                continue;
            }

            var isActive = true;
            if (isActiveText is not null && !bool.TryParse(isActiveText, out isActive))
            {
                errors.Add($"Users row {row.RowNumber}: IsActive must be TRUE or FALSE.");
                continue;
            }

            var user = new User
            {
                EmployeeNo = employeeNo,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = _passwordHasher.Hash(password),
                Role = role,
                Department = department,
                Designation = designation,
                IsActive = isActive,
                CreatedDate = DateTime.UtcNow
            };

            usersByEmployeeNo[employeeNo] = user;
            importedUserRows.Add((row, user, role.RoleName, reportsToEmployeeNo));
        }

        foreach (var item in importedUserRows)
        {
            User? manager = null;
            if (item.ReportsToEmployeeNo is not null &&
                !usersByEmployeeNo.TryGetValue(item.ReportsToEmployeeNo, out manager))
            {
                errors.Add($"Users row {item.Row.RowNumber}: ReportsTo employee '{item.ReportsToEmployeeNo}' was not found.");
                continue;
            }

            var managerRole = manager?.Role.RoleName;
            if (!IsValidHierarchy(item.RoleName, managerRole))
            {
                errors.Add($"Users row {item.Row.RowNumber}: role '{item.RoleName}' cannot report to role '{managerRole ?? "none"}'.");
                continue;
            }

            item.User.Manager = manager;
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException(string.Join(" | ", errors.Take(20)));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Departments.AddRangeAsync(newDepartments);
            await _context.Designations.AddRangeAsync(newDesignations);
            await _context.Users.AddRangeAsync(importedUserRows.Select(x => x.User));
            await _context.SaveChangesAsync();

            var adminId = _currentUserService.UserId ?? 0;
            var auditLogs = newDepartments.Select(d => new AuditLog
                { UserId = adminId, Action = "Create", TableName = "Departments", RecordId = d.DepartmentId, CreatedDate = DateTime.UtcNow })
                .Concat(newDesignations.Select(d => new AuditLog
                { UserId = adminId, Action = "Create", TableName = "Designations", RecordId = d.DesignationId, CreatedDate = DateTime.UtcNow }))
                .Concat(importedUserRows.Select(x => new AuditLog
                { UserId = adminId, Action = "Create", TableName = "Users", RecordId = x.User.UserId, CreatedDate = DateTime.UtcNow }));
            await _context.AuditLogs.AddRangeAsync(auditLogs);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return new BulkImportResultDto
        {
            DepartmentsCreated = newDepartments.Count,
            DesignationsCreated = newDesignations.Count,
            UsersCreated = importedUserRows.Count
        };
    }

    private static IXLWorksheet RequireSheet(XLWorkbook workbook, string name)
    {
        if (!workbook.TryGetWorksheet(name, out var worksheet))
        {
            throw new ValidationAppException($"Required worksheet '{name}' was not found.");
        }
        return worksheet;
    }

    private static List<ExcelRow> ReadRows(IXLWorksheet sheet, params string[] requiredHeaders)
    {
        var headerRow = sheet.FirstRowUsed()
            ?? throw new ValidationAppException($"Worksheet '{sheet.Name}' is empty.");
        var headers = headerRow.CellsUsed().ToDictionary(
            c => c.GetString().Trim(), c => c.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);

        var missing = requiredHeaders.Where(h => !headers.ContainsKey(h)).ToList();
        if (missing.Count > 0)
        {
            throw new ValidationAppException(
                $"Worksheet '{sheet.Name}' is missing columns: {string.Join(", ", missing)}.");
        }

        return sheet.RowsUsed()
            .Where(r => r.RowNumber() > headerRow.RowNumber() && !r.IsEmpty())
            .Select(r => new ExcelRow(r.RowNumber(), r, headers))
            .ToList();
    }

    private static bool IsValidHierarchy(string roleName, string? managerRole) => roleName switch
    {
        RoleNames.Admin or RoleNames.CEO => managerRole is null,
        RoleNames.Manager or RoleNames.HOD => managerRole == RoleNames.CEO,
        RoleNames.Supervisor => managerRole == RoleNames.HOD,
        RoleNames.Employee => managerRole is RoleNames.Supervisor or RoleNames.HOD,
        _ => false
    };

    private static string? NullIfWhiteSpace(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record ExcelRow(
        int RowNumber,
        IXLRow Row,
        IReadOnlyDictionary<string, int> Headers)
    {
        public string Value(string header) => Row.Cell(Headers[header]).GetString();
    }
}
