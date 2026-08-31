namespace HomelabBlog.Api.Models;

public enum PostStatus
{
    Draft,
    Published
}

public class Post
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Slug { get; set; }
    public required string Content { get; set; }
    public string? Excerpt { get; set; }
    public string? HeroImageUrl { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}