using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HomelabBlog.Api.Services;

namespace HomelabBlog.Api.Controllers;

public record UploadResponse(string Url);

[ApiController]
[Route("api/admin/upload")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IFileStorageService _storage;

    public UploadController(IFileStorageService storage)
    {
        _storage = storage;
    }

    private const long MaxBytes = 5 * 1024 * 1024;

    private static readonly Dictionary<string, string[]> AllowedTypes = new()
    {
        ["image/jpeg"] = [".jpg", ".jpeg"],
        ["image/png"] = [".png"],
        ["image/gif"] = [".gif"],
        ["image/webp"] = [".webp"],
        ["image/avif"] = [".avif"],
    };

    [HttpPost]
    [RequestSizeLimit(MaxBytes + 1024 * 1024)]
    public async Task<ActionResult<UploadResponse>> Upload(IFormFile file)
    {
        if (file.Length == 0) return BadRequest("Empty file");
        if (file.Length > MaxBytes) return BadRequest("File is larger than 5 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedTypes.TryGetValue(file.ContentType, out var extensions) || !extensions.Contains(extension))
        {
            return BadRequest("Only JPEG, PNG, GIF, WebP and AVIF images are allowed.");
        }

        using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(stream, file.FileName, file.ContentType);

        return Ok(new UploadResponse(url));
    }
}