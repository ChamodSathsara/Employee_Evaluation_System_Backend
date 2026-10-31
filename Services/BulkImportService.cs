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
        var userSheet = GetImportSheet(workbook);
        var userRows = ReadRows(userSheet,
            "EmployeeNo", "FirstName", "LastName", "Email", "Password", "RoleName",
            "CompanyName", "DepartmentName", "DesignationName", "ReportedSupervisorEmployeeNo",
            "ReportedHodEmployeeNo", "IsActive");

        var errors = new List<string>();
        var existingCompanies = await _context.Companies.ToListAsync();
        var existingDepartments = await _context.Departments.ToListAsync();
        var existingDesignations = await _context.Designations.ToListAsync();
        var existingUsers = await _context.Users.Include(u => u.Role).ToListAsync();
        var roles = await _context.Roles.ToListAsync();

        var companies = existingCompanies.ToDictionary(
            c => c.CompanyName, StringComparer.OrdinalIgnoreCase);
        var departments = existingDepartments.ToDictionary(
            d => d.DepartmentName, StringComparer.OrdinalIgnoreCase);
        var designations = existingDesignations.ToDictionary(
            d => d.DesignationName, StringComparer.OrdinalIgnoreCase);
        var rolesByName = roles.ToDictionary(r => r.RoleName, StringComparer.OrdinalIgnoreCase);
        var usersByEmployeeNo = existingUsers.ToDictionary(
            u => u.EmployeeNo, StringComparer.OrdinalIgnoreCase);
        var existingEmails = existingUsers.Select(u => u.Email)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newCompanies = new List<Company>();
        var newDepartments = new List<Department>();
        var newDesignations = new List<Designation>();

        // The single-sheet template is the source of master data. Existing records are
        // reused case-insensitively; missing companies/departments/designations are created.
        foreach (var row in userRows)
        {
            var companyName = row.Value("CompanyName").Trim();
            var departmentName = NullIfWhiteSpace(row.Value("DepartmentName"));
            var designationName = NullIfWhiteSpace(row.Value("DesignationName"));

            if (!string.IsNullOrWhiteSpace(companyName) && !companies.ContainsKey(companyName))
            {
                if (companyName.Length > 150)
                {
                    errors.Add($"Employee Import row {row.RowNumber}: CompanyName exceeds 150 characters.");
                }
                else
                {
                    var company = new Company { CompanyName = companyName };
                    companies[companyName] = company;
                    newCompanies.Add(company);
                }
            }

            if (departmentName is not null && !departments.ContainsKey(departmentName))
            {
                if (departmentName.Length > 100)
                {
                    errors.Add($"Employee Import row {row.RowNumber}: DepartmentName exceeds 100 characters.");
                }
                else
                {
                    var department = new Department { DepartmentName = departmentName };
                    departments[departmentName] = department;
                    newDepartments.Add(department);
                }
            }

            if (designationName is not null && !designations.ContainsKey(designationName))
            {
                if (designationName.Length > 100)
                {
                    errors.Add($"Employee Import row {row.RowNumber}: DesignationName exceeds 100 characters.");
                }
                else
                {
                    var designation = new Designation { DesignationName = designationName };
                    designations[designationName] = designation;
                    newDesignations.Add(designation);
                }
            }
        }

        var importedUserRows = new List<(ExcelRow Row, User User, string RoleName,
            string? SupervisorEmployeeNo, string? HodEmployeeNo)>();
        var importedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in userRows)
        {
            var employeeNo = row.Value("EmployeeNo").Trim();
            var firstName = row.Value("FirstName").Trim();
            var lastName = row.Value("LastName").Trim();
            var email = row.Value("Email").Trim();
            var password = row.Value("Password");
            var roleName = row.Value("RoleName").Trim();
            var companyName = row.Value("CompanyName").Trim();
            var departmentName = NullIfWhiteSpace(row.Value("DepartmentName"));
            var designationName = NullIfWhiteSpace(row.Value("DesignationName"));
            var supervisorEmployeeNo = NullIfWhiteSpace(row.Value("ReportedSupervisorEmployeeNo"));
            var hodEmployeeNo = NullIfWhiteSpace(row.Value("ReportedHodEmployeeNo"));
            var isActiveText = NullIfWhiteSpace(row.Value("IsActive"));

            if (string.IsNullOrWhiteSpace(employeeNo) || string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(roleName) ||
                string.IsNullOrWhiteSpace(companyName))
            {
                errors.Add($"Users row {row.RowNumber}: EmployeeNo, FirstName, LastName, Email, Password, RoleName and CompanyName are required.");
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

            if (!companies.TryGetValue(companyName, out var company))
            {
                errors.Add($"Users row {row.RowNumber}: company '{companyName}' was not found in the database.");
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

            if (!TryParseIsActive(isActiveText, out var isActive))
            {
                errors.Add($"Users row {row.RowNumber}: IsActive must be TRUE, FALSE, 1, or 0.");
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
                Company = company,
                Department = department,
                Designation = designation,
                IsActive = isActive,
                CreatedDate = DateTime.UtcNow
            };

            usersByEmployeeNo[employeeNo] = user;
            importedUserRows.Add((row, user, role.RoleName, supervisorEmployeeNo, hodEmployeeNo));
        }

        foreach (var item in importedUserRows)
        {
            User? firstReference = null;
            User? secondReference = null;

            if (item.SupervisorEmployeeNo is not null &&
                !usersByEmployeeNo.TryGetValue(item.SupervisorEmployeeNo, out firstReference))
            {
                errors.Add($"Users row {item.Row.RowNumber}: reported supervisor '{item.SupervisorEmployeeNo}' was not found.");
                continue;
            }

            if (item.HodEmployeeNo is not null &&
                !usersByEmployeeNo.TryGetValue(item.HodEmployeeNo, out secondReference))
            {
                errors.Add($"Users row {item.Row.RowNumber}: reported HOD '{item.HodEmployeeNo}' was not found.");
                continue;
            }

            // Dependency-sorted source files sometimes repeat a HOD/CEO in both reporting
            // columns. Use the referenced user's actual role to normalize the two links.
            var references = new[] { firstReference, secondReference }
                .Where(u => u is not null)
                .DistinctBy(u => u!.EmployeeNo)
                .ToList();
            var supervisor = references.FirstOrDefault(u => u!.Role.RoleName == RoleNames.Supervisor);
            var hod = references.FirstOrDefault(u => u!.Role.RoleName == RoleNames.HOD);

            if (item.RoleName == RoleNames.CEO)
            {
                supervisor = null;
                hod = null;
            }
            else if (item.RoleName == RoleNames.HOD)
            {
                // The new model intentionally has no CEO reporting field for HOD users.
                supervisor = null;
                hod = null;
            }

            if (item.RoleName == RoleNames.Employee && supervisor is null && hod is null)
            {
                errors.Add($"Users row {item.Row.RowNumber}: an Employee must have at least one reported Supervisor or HOD.");
                continue;
            }
            item.User.ReportedSupervisor = supervisor;
            item.User.ReportedHod = hod;
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException(string.Join(" | ", errors.Take(20)));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.Companies.AddRangeAsync(newCompanies);
            await _context.Departments.AddRangeAsync(newDepartments);
            await _context.Designations.AddRangeAsync(newDesignations);
            await _context.Users.AddRangeAsync(importedUserRows.Select(x => x.User));
            await _context.SaveChangesAsync();

            var adminId = _currentUserService.UserId ?? 0;
            var auditLogs = newCompanies.Select(c => new AuditLog
                { UserId = adminId, Action = "Create", TableName = "Companies", RecordId = c.CompanyId, CreatedDate = DateTime.UtcNow })
                .Concat(newDepartments.Select(d => new AuditLog
                { UserId = adminId, Action = "Create", TableName = "Departments", RecordId = d.DepartmentId, CreatedDate = DateTime.UtcNow })
                .Concat(newDesignations.Select(d => new AuditLog
                { UserId = adminId, Action = "Create", TableName = "Designations", RecordId = d.DesignationId, CreatedDate = DateTime.UtcNow }))
                .Concat(importedUserRows.Select(x => new AuditLog
                { UserId = adminId, Action = "Create", TableName = "Users", RecordId = x.User.UserId, CreatedDate = DateTime.UtcNow })));
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
            CompaniesCreated = newCompanies.Count,
            DepartmentsCreated = newDepartments.Count,
            DesignationsCreated = newDesignations.Count,
            UsersCreated = importedUserRows.Count
        };
    }

    private static IXLWorksheet GetImportSheet(XLWorkbook workbook)
    {
        if (workbook.TryGetWorksheet("Employee Import", out var worksheet) ||
            workbook.TryGetWorksheet("Users", out worksheet))
        {
            return worksheet;
        }

        if (workbook.Worksheets.Count == 1)
        {
            return workbook.Worksheet(1);
        }

        throw new ValidationAppException(
            "A worksheet named 'Employee Import' or 'Users' is required.");
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

    private static bool TryParseIsActive(string? value, out bool isActive)
    {
        if (value is null)
        {
            isActive = true;
            return true;
        }

        if (value == "1")
        {
            isActive = true;
            return true;
        }

        if (value == "0")
        {
            isActive = false;
            return true;
        }

        return bool.TryParse(value, out isActive);
    }

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
