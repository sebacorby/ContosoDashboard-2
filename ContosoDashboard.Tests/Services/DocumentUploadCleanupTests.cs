using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ContosoDashboard.Tests.Services;

public class DocumentUploadCleanupTests
{
    [Fact]
    public async Task UploadAsync_UnsafeScan_PersistsNothing()
    {
        using var db = new TestDatabase();
        var storage = new FakeStorageService();
        var service = new DocumentService(db.Context, storage, new FakeScanService(false), TestConfiguration.Create());
        await using var content = new MemoryStream("unsafe"u8.ToArray());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadAsync(Request(), content, 4, CancellationToken.None));

        Assert.Empty(await db.Context.Documents.ToListAsync());
        Assert.Equal(0, storage.UploadCalls);
    }

    [Fact]
    public async Task UploadAsync_StorageFailure_PersistsNothing()
    {
        using var db = new TestDatabase();
        var storage = new FakeStorageService { ThrowOnUpload = true };
        var service = new DocumentService(db.Context, storage, new FakeScanService(), TestConfiguration.Create());
        await using var content = new MemoryStream("ok"u8.ToArray());

        await Assert.ThrowsAsync<IOException>(() =>
            service.UploadAsync(Request(), content, 4, CancellationToken.None));

        Assert.Empty(await db.Context.Documents.ToListAsync());
    }

    [Fact]
    public async Task UploadAsync_MetadataFailure_DeletesStoredFile()
    {
        using var db = new TestDatabase();
        const string duplicatePath = "4/personal/fixed.pdf";
        db.Context.Documents.Add(new Document
        {
            Title = "Existing",
            Category = "Reports",
            OriginalFileName = "existing.pdf",
            StoragePath = duplicatePath,
            FileSize = 2,
            FileType = "application/pdf",
            UploadedByUserId = 4,
            UploadDate = DateTime.UtcNow
        });
        await db.Context.SaveChangesAsync();

        var storage = new FakeStorageService { FixedPath = duplicatePath };
        var service = new DocumentService(db.Context, storage, new FakeScanService(), TestConfiguration.Create());
        await using var content = new MemoryStream("ok"u8.ToArray());

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            service.UploadAsync(Request(), content, 4, CancellationToken.None));

        Assert.Equal(1, storage.DeleteCalls);
    }

    private static DocumentUploadRequest Request() =>
        new(
            "Quarterly report",
            null,
            "Reports",
            null,
            Array.Empty<string>(),
            "report.pdf",
            "application/pdf",
            2);
}
