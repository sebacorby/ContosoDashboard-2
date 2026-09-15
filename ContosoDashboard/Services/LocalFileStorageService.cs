using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ContosoDashboard.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration, IWebHostEnvironment environment)
        : this(Path.Combine(
            environment.ContentRootPath,
            configuration["DocumentStorage:RootPath"] ?? "AppData/uploads"))
    {
    }

    public LocalFileStorageService(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new ArgumentException("Storage root is required.", nameof(rootPath));

        _rootPath = Path.GetFullPath(rootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> UploadAsync(
        Stream content,
        string originalFileName,
        int userId,
        int? projectId,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

        var extension = Path.GetExtension(originalFileName);
        extension = string.IsNullOrWhiteSpace(extension) ? string.Empty : extension.ToLowerInvariant();
        var projectSegment = projectId?.ToString() ?? "personal";
        var relativeDirectory = Path.Combine(userId.ToString(), projectSegment);
        var relativePath = Path.Combine(relativeDirectory, $"{Guid.NewGuid():N}{extension}");
        var finalPath = ResolvePath(relativePath);
        var directory = Path.GetDirectoryName(finalPath)!;
        Directory.CreateDirectory(directory);

        var tempPath = finalPath + ".tmp";
        try
        {
            await using (var output = new FileStream(
                tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await content.CopyToAsync(output, ct);
                await output.FlushAsync(ct);
            }

            File.Move(tempPath, finalPath);
            return relativePath.Replace(Path.DirectorySeparatorChar, '/');
        }
        catch
        {
            TryDelete(tempPath);
            TryDelete(finalPath);
            throw;
        }
    }

    public Task<Stream> DownloadAsync(string storagePath, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var fullPath = ResolvePath(storagePath);
        if (!File.Exists(fullPath)) throw new FileNotFoundException("Stored document not found.", storagePath);
        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var fullPath = ResolvePath(storagePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string storagePath, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(File.Exists(ResolvePath(storagePath)));
    }

    private string ResolvePath(string storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath)) throw new ArgumentException("Storage path is required.", nameof(storagePath));
        var normalized = storagePath.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, normalized));
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var rootPrefix = _rootPath.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootPrefix, comparison))
            throw new InvalidOperationException("Storage path escapes the configured root.");

        return fullPath;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch
        {
            // Preserve the original storage exception; cleanup is best-effort.
        }
    }
}
