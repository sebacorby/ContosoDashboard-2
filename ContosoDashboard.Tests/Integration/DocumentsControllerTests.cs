using System.Security.Claims;
using ContosoDashboard.Controllers;
using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ContosoDashboard.Tests.Integration;

public class DocumentsControllerTests
{
    [Fact]
    public async Task Download_MissingDocument_Returns404()
    {
        using var db = new TestDatabase();
        var controller = Controller(db, new FakeStorageService(), 4);

        var result = await controller.Download(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Download_UnauthorizedProjectDocument_Returns403()
    {
        using var db = new TestDatabase();
        var storage = new FakeStorageService { FixedPath = "4/1/project.pdf" };
        await using var source = new MemoryStream("secret"u8.ToArray());
        var path = await storage.UploadAsync(source, "project.pdf", 4, 1, CancellationToken.None);
        db.Context.Documents.Add(new Document
        {
            Title = "Project secret",
            Category = "Project Documents",
            OriginalFileName = "project.pdf",
            StoragePath = path,
            FileSize = 6,
            FileType = "application/pdf",
            UploadedByUserId = 4,
            ProjectId = 1,
            UploadDate = DateTime.UtcNow
        });
        await db.Context.SaveChangesAsync();
        var controller = Controller(db, storage, 5);

        var result = await controller.Download(1, CancellationToken.None);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Download_AuthorizedOwner_ReturnsFileWithOriginalName()
    {
        using var db = new TestDatabase();
        var storage = new FakeStorageService();
        var service = new DocumentService(db.Context, storage, new FakeScanService(), TestConfiguration.Create());
        await using var content = new MemoryStream("hello"u8.ToArray());
        var document = await service.UploadAsync(
            new DocumentUploadRequest("Report", null, "Reports", null, Array.Empty<string>(), "report.pdf", "application/pdf", 5),
            content, 4, CancellationToken.None);
        var controller = Controller(db, storage, 4);

        var result = await controller.Download(document.DocumentId, CancellationToken.None);

        var file = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("report.pdf", file.FileDownloadName);
        Assert.Equal("application/pdf", file.ContentType);
    }

    private static DocumentsController Controller(TestDatabase db, FakeStorageService storage, int userId)
    {
        var service = new DocumentService(
            db.Context,
            storage,
            new FakeScanService(),
            TestConfiguration.Create());
        var controller = new DocumentsController(service)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                "test"));
        return controller;
    }
}
