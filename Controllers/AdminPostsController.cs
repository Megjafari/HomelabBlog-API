using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomelabBlog.Api.Data;
using HomelabBlog.Api.DTOs;
using HomelabBlog.Api.Models;

namespace HomelabBlog.Api.Controllers;

[ApiController]
[Route("api/admin/posts")]
[Authorize]
public class AdminPostsController : ControllerBase
{
    private readonly BlogDbContext _db;

    public AdminPostsController(BlogDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<PostDetailDto>>> GetAll()
    {
        var posts = await _db.Posts
            .Include(p => p.Tags)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostDetailDto(
                p.Id, p.Title, p.Slug, p.Content, p.Excerpt,
                p.HeroImageUrl, p.Status.ToString(), p.PublishedAt,
                p.Tags.Select(t => t.Slug).ToList()))
            .ToListAsync();

        return Ok(posts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PostDetailDto>> GetById(Guid id)
    {
        var post = await _db.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return NotFound();

        return Ok(new PostDetailDto(
            post.Id, post.Title, post.Slug, post.Content, post.Excerpt,
            post.HeroImageUrl, post.Status.ToString(), post.PublishedAt,
            post.Tags.Select(t => t.Slug).ToList()));
    }

    [HttpPost]
    public async Task<ActionResult<PostDetailDto>> Create(CreatePostDto dto)
    {
        var slugExists = await _db.Posts.AnyAsync(p => p.Slug == dto.Slug);
        if (slugExists)
        {
            return Conflict($"A post with slug '{dto.Slug}' already exists.");
        }

        var tags = await ResolveTags(dto.Tags);

        var post = new Post
        {
            Title = dto.Title,
            Slug = dto.Slug,
            Content = dto.Content,
            Excerpt = dto.Excerpt,
            HeroImageUrl = dto.HeroImageUrl,
            Tags = tags
        };

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        var result = new PostDetailDto(
            post.Id, post.Title, post.Slug, post.Content, post.Excerpt,
            post.HeroImageUrl, post.Status.ToString(), post.PublishedAt,
            post.Tags.Select(t => t.Slug).ToList());

        return CreatedAtAction(nameof(GetById), new { id = post.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PostDetailDto>> Update(Guid id, UpdatePostDto dto)
    {
        var post = await _db.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return NotFound();

        var slugTaken = await _db.Posts.AnyAsync(p => p.Slug == dto.Slug && p.Id != id);
        if (slugTaken)
        {
            return Conflict($"A post with slug '{dto.Slug}' already exists.");
        }

        post.Title = dto.Title;
        post.Slug = dto.Slug;
        post.Content = dto.Content;
        post.Excerpt = dto.Excerpt;
        post.HeroImageUrl = dto.HeroImageUrl;
        post.UpdatedAt = DateTime.UtcNow;
        post.Tags = await ResolveTags(dto.Tags);

        await _db.SaveChangesAsync();

        var result = new PostDetailDto(
            post.Id, post.Title, post.Slug, post.Content, post.Excerpt,
            post.HeroImageUrl, post.Status.ToString(), post.PublishedAt,
            post.Tags.Select(t => t.Slug).ToList());

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post is null) return NotFound();

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post is null) return NotFound();

        post.Status = PostStatus.Published;
        post.PublishedAt ??= DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/unpublish")]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post is null) return NotFound();

        post.Status = PostStatus.Draft;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<List<Tag>> ResolveTags(List<string>? tagNames)
    {
        if (tagNames is null || tagNames.Count == 0) return new List<Tag>();

        // Tags are identified by slug, so "K8s" and "k8s" are the same tag.
        var wanted = tagNames
            .Select(n => n.Trim())
            .Where(n => n.Length > 0)
            .Select(n => (Name: n, Slug: TagSlug(n)))
            .Where(t => t.Slug.Length > 0)
            .GroupBy(t => t.Slug)
            .Select(g => g.First())
            .ToList();

        var slugs = wanted.Select(t => t.Slug).ToList();
        var existing = await _db.Tags.Where(t => slugs.Contains(t.Slug)).ToListAsync();
        var missing = wanted
            .Where(t => existing.All(e => e.Slug != t.Slug))
            .Select(t => new Tag { Name = t.Name, Slug = t.Slug });

        return existing.Concat(missing).ToList();
    }

    private static string TagSlug(string name)
    {
        var slug = System.Text.RegularExpressions.Regex.Replace(name.ToLowerInvariant(), @"[^a-z0-9]+", "-");
        return slug.Trim('-');
    }
}