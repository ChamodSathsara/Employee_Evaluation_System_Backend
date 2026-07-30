namespace EmployeeEvaluationSystem.Services;

public interface IFileStorageService
{
    /// <summary>Saves a file stream under the configured upload path and returns its relative stored path.</summary>
    Task<string> UploadAsync(Stream content, string originalFileName);

    /// <summary>Deletes a previously stored file, given the relative path returned by UploadAsync.</summary>
    void Delete(string storedFilePath);
}
