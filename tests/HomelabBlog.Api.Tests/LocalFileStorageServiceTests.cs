using HomelabBlog.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace HomelabBlog.Api.Tests;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "blog-uploads-" + Guid.NewGuid());

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private LocalFileStorageService CreateService(string? publicBaseUrl = "https://api.test/uploads/")
    {
        var settings = new Dictionary<string, string?>
        {
            ["Storage:LocalPath"] = _dir,
            ["Storage:PublicBaseUrl"] = publicBaseUrl,
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new LocalFileStorageService(config, new FakeEnvironment());
    }

    [Fact]
    public async Task Upload_WritesFileAndReturnsPublicUrl()
    {
        var service = CreateService();

        var url = await service.UploadAsync(new MemoryStream([1, 2, 3]), "My Photo.PNG", "image/png");

        Assert.StartsWith("https://api.test/uploads/", url);
        Assert.EndsWith(".png", url);
        var stored = Path.Combine(_dir, url.Split('/').Last());
        Assert.Equal([1, 2, 3], await File.ReadAllBytesAsync(stored));
    }

    [Fact]
    public async Task Upload_IgnoresPathsInTheClientFileName()
    {
        var service = CreateService();

        var url = await service.UploadAsync(new MemoryStream([1]), "../../etc/passwd.png", "image/png");

        Assert.Single(Directory.GetFiles(_dir));
        Assert.DoesNotContain("..", url);
        Assert.DoesNotContain("passwd", url);
    }

    private class FakeEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
