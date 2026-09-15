using ContosoDashboard.Services;
using Xunit;

namespace ContosoDashboard.Tests.Services;

public class TrainingFileScanServiceTests
{
    [Fact]
    public async Task ScanAsync_NormalContent_IsSafe()
    {
        var service = new TrainingFileScanService();
        await using var stream = new MemoryStream("quarterly report"u8.ToArray());

        var result = await service.ScanAsync(stream, "report.pdf", "application/pdf", CancellationToken.None);

        Assert.True(result.IsSafe);
        Assert.Null(result.RejectionReason);
    }

    [Fact]
    public async Task ScanAsync_EicarTrainingSignature_IsRejected()
    {
        var service = new TrainingFileScanService();
        await using var stream = new MemoryStream("X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!"u8.ToArray());

        var result = await service.ScanAsync(stream, "eicar.txt", "text/plain", CancellationToken.None);

        Assert.False(result.IsSafe);
        Assert.Contains("training", result.RejectionReason!, StringComparison.OrdinalIgnoreCase);
    }
}
