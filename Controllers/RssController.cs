using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HomelabBlog.Api.Data;
using HomelabBlog.Api.DTOs;
using HomelabBlog.Api.Models;
using HomelabBlog.Api.Services;

namespace HomelabBlog.Api.Controllers;

[ApiController]
[Route("api/rss")]
public class RssController : ControllerBase
{
    private readonly BlogDbContext _db;
    private readonly IConfiguration _config;

    public RssController(BlogDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpGet]
    public async Task<ContentResult> Get()
    {
        var posts = await _db.Posts
            .Where(p => p.Status == PostStatus.Published)
            .OrderByDescending(p => p.PublishedAt)
            .Take(20)
            .Select(p => new PostListItemDto(
                p.Id, p.Title, p.Slug, p.Excerpt, p.HeroImageUrl,
                p.PublishedAt, p.Tags.Select(t => t.Slug).ToList()))
            .ToListAsync();

        var xml = RssFeedBuilder.Build(
            posts,
            _config["Site:Title"] ?? "Blog",
            _config["Site:Description"] ?? "",
            _config["Site:BaseUrl"] ?? "http://localhost:5173");

        return Content(xml, "application/rss+xml; charset=utf-8");
    }
}
