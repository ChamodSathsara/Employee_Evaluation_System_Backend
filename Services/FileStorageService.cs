using Microsoft.Extensions.Configuration;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Local-disk file storage under the configured "FileStorage:UploadPath" (default:
/// wwwroot/uploads), used for achievement supporting documents. Files are renamed with a
/// GUID prefix to avoid collisions/overwrites while preserving the original extension.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _uploadRoot;

    public FileStorageService(IConfiguration configuration)
    {
        var configuredPath = configuration["FileStorage:UploadPath"] ?? "wwwroot/uploads";
        _uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), configuredPath);

        if (!Directory.Exists(_uploadRoot))
        {
            Directory.CreateDirectory(_uploadRoot);
        }
    }

    public async Task<string> UploadAsync(Stream content, string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_uploadRoot, storedFileName);

        await using (var fileStream = new FileStream(fullPath, FileMode.Create))
        {
            await content.CopyToAsync(fileStream);
        }

        // Relative path is what gets persisted (e.g. on EmployeeAchievement.Attachment),
        // so it stays valid regardless of the deployment's absolute file-system root.
        return Path.Combine("uploads", storedFileName).Replace('\\', '/');
    }

    public void Delete(string storedFilePath)
    {
        var fileName = Path.GetFileName(storedFilePath);
        var fullPath = Path.Combine(_uploadRoot, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
