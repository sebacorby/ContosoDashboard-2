using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ContosoDashboard.Services;

public sealed class DocumentService : IDocumentService
{
    private static readonly HashSet<string> Categories = new(StringComparer.OrdinalIgnoreCase)
    {
        "Project Documents", "Team Resources", "Personal Files",
        "Reports", "Presentations", "Other"
    };

    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".jpg", ".jpeg", ".png"
    };

    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _storage;
    private readonly IFileScanService _scanner;
    private readonly long _maxFileSize;

    public DocumentService(
        ApplicationDbContext db,
        IFileStorageService storage,
        IFileScanService scanner,
        IConfiguration configuration)
    {
        _db = db;
        _storage = storage;
        _scanner = scanner;
        _maxFileSize = configuration.GetValue<long?>("DocumentStorage:MaxFileSizeBytes") ?? 26214400;
    }

    public async Task<Document> UploadAsync(
        DocumentUploadRequest request,
        Stream content,
        int userId,
        CancellationToken ct)
    {
        ValidateRequest(request, userId);
        if (!await _db.Users.AnyAsync(u => u.UserId == userId, ct))
            throw new UnauthorizedAccessException("Unknown user.");

        if (request.ProjectId.HasValue && !await IsProjectAuthorizedAsync(request.ProjectId.Value, userId, ct))
            throw new UnauthorizedAccessException("User is not authorized for this project.");

        Reset(content);
        var scan = await _scanner.ScanAsync(content, request.OriginalFileName, request.ContentType, ct);
        if (!scan.IsSafe)
            throw new InvalidOperationException(scan.RejectionReason ?? "File was rejected by the training scanner.");

        Reset(content);
        string? storagePath = null;
        try
        {
            storagePath = await _storage.UploadAsync(
                content, request.OriginalFileName, userId, request.ProjectId, ct);

            var document = new Document
            {
                Title = request.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Category = request.Category.Trim(),
                OriginalFileName = Path.GetFileName(request.OriginalFileName),
                StoragePath = storagePath,
                FileSize = request.FileSize,
                FileType = request.ContentType,
                UploadedByUserId = userId,
                ProjectId = request.ProjectId,
                UploadDate = DateTime.UtcNow
            };

            foreach (var tag in request.Tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                document.Tags.Add(new DocumentTag { Value = tag });
            }

            document.Activities.Add(new DocumentActivity
            {
                DocumentTitleSnapshot = document.Title,
                UserId = userId,
                Action = "Upload",
                OccurredAt = DateTime.UtcNow
            });

            _db.Documents.Add(document);
            await _db.SaveChangesAsync(ct);
            return document;
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(storagePath))
            {
                try { await _storage.DeleteAsync(storagePath, ct); }
                catch { /* preserve original persistence/storage exception */ }
            }

            foreach (var entry in _db.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added &&
                    (e.Entity is Document || e.Entity is DocumentTag || e.Entity is DocumentActivity)))
            {
                entry.State = EntityState.Detached;
            }
            throw;
        }
    }

    public async Task<IReadOnlyList<Document>> GetAccessibleDocumentsAsync(int userId, CancellationToken ct)
    {
        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserId == userId, ct)
            ?? throw new UnauthorizedAccessException("Unknown user.");

        var query = _db.Documents
            .AsNoTracking()
            .Include(d => d.Tags)
            .Include(d => d.Project)
            .Include(d => d.Shares)
            .AsQueryable();

        if (user.Role == UserRole.Administrator)
            return await query.OrderByDescending(d => d.UploadDate).ToListAsync(ct);

        var projectIds = await AuthorizedProjectIdsAsync(userId, ct);
        var department = user.Department;

        return await query
            .Where(d =>
                (d.ProjectId == null && d.UploadedByUserId == userId) ||
                (d.ProjectId != null && projectIds.Contains(d.ProjectId.Value)) ||
                (d.ProjectId == null && d.Shares.Any(s =>
                    s.SharedWithUserId == userId ||
                    (department != null && s.SharedWithDepartment == department))))
            .OrderByDescending(d => d.UploadDate)
            .ToListAsync(ct);
    }

    public Task<Document?> GetDocumentAsync(int documentId, CancellationToken ct) =>
        _db.Documents
            .AsNoTracking()
            .Include(d => d.Project)
            .Include(d => d.Shares)
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, ct);

    public async Task<bool> CanAccessAsync(Document document, int userId, CancellationToken ct)
    {
        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserId == userId, ct);
        if (user is null) return false;
        if (user.Role == UserRole.Administrator) return true;

        if (document.ProjectId.HasValue)
            return await IsProjectAuthorizedAsync(document.ProjectId.Value, userId, ct);

        if (document.UploadedByUserId == userId) return true;

        return await _db.DocumentShares.AsNoTracking().AnyAsync(s =>
            s.DocumentId == document.DocumentId &&
            (s.SharedWithUserId == userId ||
             (user.Department != null && s.SharedWithDepartment == user.Department)), ct);
    }

    public Task<Stream> OpenContentAsync(Document document, CancellationToken ct) =>
        _storage.DownloadAsync(document.StoragePath, ct);

    private void ValidateRequest(DocumentUploadRequest request, int userId)
    {
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Document title is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Category) || !Categories.Contains(request.Category.Trim()))
            throw new ArgumentException("A valid document category is required.", nameof(request));
        if (request.FileSize <= 0 || request.FileSize > _maxFileSize)
            throw new ArgumentException($"File size must be between 1 and {_maxFileSize} bytes.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.OriginalFileName))
            throw new ArgumentException("Original file name is required.", nameof(request));
        if (!Extensions.Contains(Path.GetExtension(request.OriginalFileName)))
            throw new ArgumentException("Unsupported file type.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.ContentType) || request.ContentType.Length > 255)
            throw new ArgumentException("A valid content type is required.", nameof(request));
    }

    private async Task<bool> IsProjectAuthorizedAsync(int projectId, int userId, CancellationToken ct)
    {
        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.UserId == userId, ct);
        if (user is null) return false;
        if (user.Role == UserRole.Administrator) return true;

        return await _db.Projects.AsNoTracking().AnyAsync(p =>
                p.ProjectId == projectId && p.ProjectManagerId == userId, ct)
            || await _db.ProjectMembers.AsNoTracking().AnyAsync(pm =>
                pm.ProjectId == projectId && pm.UserId == userId, ct);
    }

    private async Task<List<int>> AuthorizedProjectIdsAsync(int userId, CancellationToken ct)
    {
        var managed = await _db.Projects.AsNoTracking()
            .Where(p => p.ProjectManagerId == userId)
            .Select(p => p.ProjectId)
            .ToListAsync(ct);
        var member = await _db.ProjectMembers.AsNoTracking()
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.ProjectId)
            .ToListAsync(ct);
        return managed.Concat(member).Distinct().ToList();
    }

    private static void Reset(Stream stream)
    {
        if (stream.CanSeek) stream.Position = 0;
    }
}
