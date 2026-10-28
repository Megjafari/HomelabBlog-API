using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomelabBlog.Api.Data;
using HomelabBlog.Api.DTOs;
using HomelabBlog.Api.Models;

namespace HomelabBlog.Api.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly BlogDbContext _db;

    public PostsController(BlogDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<PostListItemDto>>> GetAll([FromQuery] string? tag, [FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.Posts
            .Where(p => p.Status == PostStatus.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(p => p.Tags.Any(t => t.Slug == tag));
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var pattern = $"%{EscapeLike(q.Trim())}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Title, pattern, "\\") ||
                (p.Excerpt != null && EF.Functions.ILike(p.Excerpt, pattern, "\\")) ||
                EF.Functions.ILike(p.Content, pattern, "\\"));
        }

        var posts = await query
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PostListItemDto(
                p.Id, p.Title, p.Slug, p.Excerpt, p.HeroImageUrl,
                p.PublishedAt, p.Tags.Select(t => t.Slug).ToList()))
            .ToListAsync();

        return Ok(posts);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<PostDetailDto>> GetBySlug(string slug)
    {
        var post = await _db.Posts
            .Include(p => p.Tags)
            .Where(p => p.Slug == slug && p.Status == PostStatus.Published)
            .FirstOrDefaultAsync();

        if (post is null) return NotFound();

        return Ok(new PostDetailDto(
            post.Id, post.Title, post.Slug, post.Content, post.Excerpt,
            post.HeroImageUrl, post.Status.ToString(), post.PublishedAt, post.UpdatedAt,
            post.Tags.Select(t => t.Slug).ToList()));
    }

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
