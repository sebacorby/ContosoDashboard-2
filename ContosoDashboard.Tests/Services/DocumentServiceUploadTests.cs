using ContosoDashboard.Services;
using Xunit;

namespace ContosoDashboard.Tests.Services;

public class DocumentServiceUploadTests
{
    [Theory]
    [InlineData("", "Reports")]
    [InlineData("Valid title", "")]
    [InlineData("Valid title", "Unknown")]
    public async Task UploadAsync_InvalidRequiredMetadata_IsRejected(string title, string category)
    {
        using var db = new TestDatabase();
        var storage = new FakeStorageService();
        var scan = new FakeScanService();
        var service = new DocumentService(db.Context, storage, scan, TestConfiguration.Create());
        var request = Request(title, category, "report.pdf", "application/pdf", 10);
        await using var content = new MemoryStream("ok"u8.ToArray());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UploadAsync(request, content, 4, CancellationToken.None));

        Assert.Equal(0, storage.UploadCalls);
    }

    [Theory]
    [InlineData("a.pdf", "application/pdf")]
    [InlineData("a.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("a.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData("a.pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    [InlineData("a.txt", "text/plain")]
    [InlineData("a.jpg", "image/jpeg")]
    [InlineData("a.png", "image/png")]
    public async Task UploadAsync_SupportedTypes_AreAccepted(string fileName, string contentType)
    {
        using var db = new TestDatabase();
        var storage = new FakeStorageService();
        var service = new DocumentService(db.Context, storage, new FakeScanService(), TestConfiguration.Create());
        var request = Request("Doc", "Reports", fileName, contentType, 10);
        await using var content = new MemoryStream("ok"u8.ToArray());

        var document = await service.UploadAsync(request, content, 4, CancellationToken.None);

        Assert.True(document.DocumentId > 0);
        Assert.Equal(fileName, document.OriginalFileName);
    }

    [Fact]
    public async Task UploadAsync_UnsupportedExtension_IsRejected()
    {
        using var db = new TestDatabase();
        var storage = new FakeStorageService();
        var service = new DocumentService(db.Context, storage, new FakeScanService(), TestConfiguration.Create());
        await using var content = new MemoryStream("no"u8.ToArray());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UploadAsync(Request("Bad", "Reports", "payload.exe", "application/octet-stream", 2), content, 4, CancellationToken.None));
    }

    [Fact]
    public async Task UploadAsync_Over25Mb_IsRejectedBeforeStorage()
    {
        using var db = new TestDatabase();
        var storage = new FakeStorageService();
        var service = new DocumentService(db.Context, storage, new FakeScanService(), TestConfiguration.Create());
        await using var content = new MemoryStream("tiny"u8.ToArray());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UploadAsync(Request("Large", "Reports", "large.pdf", "application/pdf", 26214401), content, 4, CancellationToken.None));

        Assert.Equal(0, storage.UploadCalls);
    }

    private static DocumentUploadRequest Request(
        string title,
        string category,
        string fileName,
        string contentType,
        long size) =>
        new(title, null, category, null, new[] { "finance", "quarterly" }, fileName, contentType, size);
}
