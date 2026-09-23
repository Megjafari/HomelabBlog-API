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

    [HttpPost]
    public async Task<ActionResult<UploadResponse>> Upload(IFormFile file)
    {
        if (file.Length == 0) return BadRequest("Empty file");

        using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(stream, file.FileName, file.ContentType);

        return Ok(new UploadResponse(url));
    }
}