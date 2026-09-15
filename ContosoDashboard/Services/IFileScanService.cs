namespace ContosoDashboard.Services;

public sealed record FileScanResult(bool IsSafe, string? RejectionReason = null)
{
    public static FileScanResult Safe() => new(true);
    public static FileScanResult Unsafe(string reason) => new(false, reason);
}

public interface IFileScanService
{
    Task<FileScanResult> ScanAsync(
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken ct);
}
