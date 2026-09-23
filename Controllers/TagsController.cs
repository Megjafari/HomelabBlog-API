using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomelabBlog.Api.Data;

namespace HomelabBlog.Api.Controllers;

public record TagDto(Guid Id, string Name, string Slug, int PostCount);

[ApiController]
[Route("api/tags")]
public class TagsController : ControllerBase
{
    private readonly BlogDbContext _db;

    public TagsController(BlogDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<TagDto>>> GetAll()
    {
        var tags = await _db.Tags
            .Select(t => new TagDto(
                t.Id, t.Name, t.Slug,
                t.Posts.Count(p => p.Status == Models.PostStatus.Published)))
            .ToListAsync();

        return Ok(tags);
    }
}