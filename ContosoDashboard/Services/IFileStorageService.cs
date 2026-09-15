namespace ContosoDashboard.Services;

public interface IFileStorageService
{
    Task<string> UploadAsync(
        Stream content,
        string originalFileName,
        int userId,
        int? projectId,
        CancellationToken ct);

    Task<Stream> DownloadAsync(string storagePath, CancellationToken ct);
    Task DeleteAsync(string storagePath, CancellationToken ct);
    Task<bool> ExistsAsync(string storagePath, CancellationToken ct);
}
