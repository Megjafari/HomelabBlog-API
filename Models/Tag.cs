namespace HomelabBlog.Api.Models;

public class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Slug { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}