using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Xunit;

namespace ContosoDashboard.Tests.Authorization;

public class DocumentAuthorizationTests
{
    [Fact]
    public async Task PersonalDocument_IsVisibleOnlyToOwnerOrAdministrator()
    {
        using var db = new TestDatabase();
        var service = CreateService(db);
        var document = new Document
        {
            Title = "Personal",
            Category = "Personal Files",
            OriginalFileName = "personal.pdf",
            StoragePath = "4/personal/a.pdf",
            FileSize = 2,
            FileType = "application/pdf",
            UploadedByUserId = 4,
            UploadDate = DateTime.UtcNow
        };

        Assert.True(await service.CanAccessAsync(document, 4, CancellationToken.None));
        Assert.True(await service.CanAccessAsync(document, 1, CancellationToken.None));
        Assert.False(await service.CanAccessAsync(document, 5, CancellationToken.None));
    }

    [Fact]
    public async Task ProjectDocument_RequiresManagerOrCurrentMember()
    {
        using var db = new TestDatabase();
        var service = CreateService(db);
        var document = new Document
        {
            Title = "Project",
            Category = "Project Documents",
            OriginalFileName = "project.pdf",
            StoragePath = "4/1/b.pdf",
            FileSize = 2,
            FileType = "application/pdf",
            UploadedByUserId = 4,
            ProjectId = 1,
            UploadDate = DateTime.UtcNow
        };

        Assert.True(await service.CanAccessAsync(document, 2, CancellationToken.None));
        Assert.True(await service.CanAccessAsync(document, 3, CancellationToken.None));
        Assert.True(await service.CanAccessAsync(document, 4, CancellationToken.None));
        Assert.False(await service.CanAccessAsync(document, 5, CancellationToken.None));
    }

    private static DocumentService CreateService(TestDatabase db) =>
        new(db.Context, new FakeStorageService(), new FakeScanService(), TestConfiguration.Create());
}

