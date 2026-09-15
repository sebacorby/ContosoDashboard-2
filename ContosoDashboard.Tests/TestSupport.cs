using ContosoDashboard.Data;
using ContosoDashboard.Models;
using ContosoDashboard.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ContosoDashboard.Tests;

internal sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;
    public ApplicationDbContext Context { get; }

    public TestDatabase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;
        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
        if (!Context.Users.Any(u => u.UserId == 5))
        {
            Context.Users.Add(new User
            {
                UserId = 5,
                Email = "outsider@contoso.com",
                DisplayName = "Outside User",
                Department = "Sales",
                JobTitle = "Analyst",
                Role = UserRole.Employee,
                CreatedDate = DateTime.UtcNow
            });
            Context.SaveChanges();
        }
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}

internal sealed class FakeStorageService : IFileStorageService
{
    private readonly Dictionary<string, byte[]> _files = new();
    public bool ThrowOnUpload { get; set; }
    public string? FixedPath { get; set; }
    public int UploadCalls { get; private set; }
    public int DeleteCalls { get; private set; }

    public async Task<string> UploadAsync(Stream content, string originalFileName, int userId, int? projectId, CancellationToken ct)
    {
        UploadCalls++;
        if (ThrowOnUpload) throw new IOException("simulated storage failure");
        using var copy = new MemoryStream();
        await content.CopyToAsync(copy, ct);
        var path = FixedPath ?? $"{userId}/{projectId?.ToString() ?? "personal"}/{Guid.NewGuid():N}{Path.GetExtension(originalFileName)}";
        _files[path] = copy.ToArray();
        if (content.CanSeek) content.Position = 0;
        return path;
    }

    public Task<Stream> DownloadAsync(string storagePath, CancellationToken ct)
    {
        if (!_files.TryGetValue(storagePath, out var bytes)) throw new FileNotFoundException();
        Stream stream = new MemoryStream(bytes, writable: false);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct)
    {
        DeleteCalls++;
        _files.Remove(storagePath);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string storagePath, CancellationToken ct) =>
        Task.FromResult(_files.ContainsKey(storagePath));
}

internal sealed class FakeScanService(bool isSafe = true) : IFileScanService
{
    public bool IsSafe { get; set; } = isSafe;
    public int Calls { get; private set; }

    public Task<FileScanResult> ScanAsync(Stream content, string originalFileName, string contentType, CancellationToken ct)
    {
        Calls++;
        return Task.FromResult(IsSafe ? FileScanResult.Safe() : FileScanResult.Unsafe("unsafe test file"));
    }
}

internal static class TestConfiguration
{
    public static Microsoft.Extensions.Configuration.IConfiguration Create() =>
        new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DocumentStorage:MaxFileSizeBytes"] = "26214400"
            })
            .Build();
}
