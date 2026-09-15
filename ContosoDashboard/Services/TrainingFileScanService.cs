using System.Text;

namespace ContosoDashboard.Services;

public sealed class TrainingFileScanService : IFileScanService
{
    private const string TrainingSignature = "EICAR-STANDARD-ANTIVIRUS-TEST-FILE";

    public async Task<FileScanResult> ScanAsync(
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(content);
        var originalPosition = content.CanSeek ? content.Position : 0;

        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        if (content.CanSeek) content.Position = originalPosition;

        var text = Encoding.UTF8.GetString(buffer.ToArray());
        var rejected = originalFileName.Contains("eicar", StringComparison.OrdinalIgnoreCase)
            || text.Contains(TrainingSignature, StringComparison.OrdinalIgnoreCase);

        return rejected
            ? FileScanResult.Unsafe("Training scanner rejected the test malware signature.")
            : FileScanResult.Safe();
    }
}

