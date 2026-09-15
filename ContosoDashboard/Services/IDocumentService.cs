using ContosoDashboard.Models;

namespace ContosoDashboard.Services;

public sealed record DocumentUploadRequest(
    string Title,
    string? Description,
    string Category,
    int? ProjectId,
    IReadOnlyCollection<string> Tags,
    string OriginalFileName,
    string ContentType,
    long FileSize);

public interface IDocumentService
{
    Task<Document> UploadAsync(
        DocumentUploadRequest request,
        Stream content,
        int userId,
        CancellationToken ct);

    Task<IReadOnlyList<Document>> GetAccessibleDocumentsAsync(int userId, CancellationToken ct);
    Task<Document?> GetDocumentAsync(int documentId, CancellationToken ct);
    Task<bool> CanAccessAsync(Document document, int userId, CancellationToken ct);
    Task<Stream> OpenContentAsync(Document document, CancellationToken ct);
}

