namespace HomelabBlog.Api.Services;

/// <summary>Stores uploads on disk; the API serves them under /uploads.</summary>
public class LocalFileStorageService : IFileStorageService
{
    public const string RequestPath = "/uploads";

    private readonly string _root;
    private readonly string _publicBaseUrl;

    public LocalFileStorageService(IConfiguration config, IHostEnvironment env)
    {
        _root = ResolveRoot(config, env);
        Directory.CreateDirectory(_root);
        _publicBaseUrl = (config["Storage:PublicBaseUrl"] ?? $"http://localhost:5062{RequestPath}").TrimEnd('/');
    }

    public static string ResolveRoot(IConfiguration config, IHostEnvironment env) =>
        Path.GetFullPath(config["Storage:LocalPath"] ?? "uploads", env.ContentRootPath);

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        // Never trust the caller's file name: keep only the extension.
        var objectName = $"{Guid.NewGuid()}{Path.GetExtension(fileName).ToLowerInvariant()}";

        await using var target = File.Create(Path.Combine(_root, objectName));
        await fileStream.CopyToAsync(target);

        return $"{_publicBaseUrl}/{objectName}";
    }
}
