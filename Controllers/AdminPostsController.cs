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

    [HttpPost]
    public async Task<ActionResult<PostDetailDto>> Create(CreatePostDto dto)
    {
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

        return CreatedAtAction(nameof(Create), new { id = post.Id }, post);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdatePostDto dto)
    {
        var post = await _db.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return NotFound();

        post.Title = dto.Title;
        post.Slug = dto.Slug;
        post.Content = dto.Content;
        post.Excerpt = dto.Excerpt;
        post.HeroImageUrl = dto.HeroImageUrl;
        post.UpdatedAt = DateTime.UtcNow;
        post.Tags = await ResolveTags(dto.Tags);

        await _db.SaveChangesAsync();
        return NoContent();
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
        post.PublishedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<List<Tag>> ResolveTags(List<string>? tagNames)
    {
        if (tagNames is null || tagNames.Count == 0) return new List<Tag>();

        var existing = await _db.Tags.Where(t => tagNames.Contains(t.Name)).ToListAsync();
        var missing = tagNames.Except(existing.Select(t => t.Name))
            .Select(name => new Tag { Name = name, Slug = name.ToLower().Replace(" ", "-") });

        return existing.Concat(missing).ToList();
    }
}