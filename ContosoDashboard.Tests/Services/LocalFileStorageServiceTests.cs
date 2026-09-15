using ContosoDashboard.Services;
using Xunit;

namespace ContosoDashboard.Tests.Services;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "contoso-storage-" + Guid.NewGuid());

    [Fact]
    public async Task UploadAsync_GeneratesSafeRelativePath()
    {
        var service = new LocalFileStorageService(_root);
        await using var stream = new MemoryStream("hello"u8.ToArray());

        var path = await service.UploadAsync(stream, "../../report.pdf", 4, null, CancellationToken.None);

        Assert.StartsWith("4/personal/", path.Replace('\\', '/'));
        Assert.DoesNotContain("..", path);
        Assert.EndsWith(".pdf", path);
        Assert.True(await service.ExistsAsync(path, CancellationToken.None));
    }

    [Fact]
    public async Task UploadDownloadDelete_RoundTripsContent()
    {
        var service = new LocalFileStorageService(_root);
        var bytes = "document-content"u8.ToArray();
        await using var upload = new MemoryStream(bytes);
        var path = await service.UploadAsync(upload, "demo.txt", 4, 1, CancellationToken.None);

        await using var download = await service.DownloadAsync(path, CancellationToken.None);
        using var copy = new MemoryStream();
        await download.CopyToAsync(copy);
        Assert.Equal(bytes, copy.ToArray());

        await service.DeleteAsync(path, CancellationToken.None);
        Assert.False(await service.ExistsAsync(path, CancellationToken.None));
    }

    [Fact]
    public async Task UploadAsync_WhenStreamFails_LeavesNoPartialFile()
    {
        var service = new LocalFileStorageService(_root);
        await using var stream = new ThrowingStream("partial-content"u8.ToArray(), 4);

        await Assert.ThrowsAsync<IOException>(() =>
            service.UploadAsync(stream, "broken.pdf", 4, null, CancellationToken.None));

        var files = Directory.Exists(_root)
            ? Directory.GetFiles(_root, "*", SearchOption.AllDirectories)
            : Array.Empty<string>();
        Assert.Empty(files);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
}


internal sealed class ThrowingStream : MemoryStream
{
    private readonly int _bytesBeforeFailure;
    private int _bytesRead;

    public ThrowingStream(byte[] buffer, int bytesBeforeFailure) : base(buffer)
    {
        _bytesBeforeFailure = bytesBeforeFailure;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_bytesRead >= _bytesBeforeFailure) throw new IOException("simulated read failure");
        var max = Math.Min(count, _bytesBeforeFailure - _bytesRead);
        var read = base.Read(buffer, offset, max);
        _bytesRead += read;
        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_bytesRead >= _bytesBeforeFailure) throw new IOException("simulated read failure");
        var slice = buffer[..Math.Min(buffer.Length, _bytesBeforeFailure - _bytesRead)];
        var read = await base.ReadAsync(slice, cancellationToken);
        _bytesRead += read;
        return read;
    }
}
