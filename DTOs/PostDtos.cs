namespace HomelabBlog.Api.DTOs;

public record PostListItemDto(
    Guid Id,
    string Title,
    string Slug,
    string? Excerpt,
    string? HeroImageUrl,
    DateTime? PublishedAt,
    List<string> Tags
);

public record PostDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    string? HeroImageUrl,
    string Status,
    DateTime? PublishedAt,
    List<string> Tags
);

public record CreatePostDto(
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    string? HeroImageUrl,
    List<string>? Tags
);

public record UpdatePostDto(
    string Title,
    string Slug,
    string Content,
    string? Excerpt,
    string? HeroImageUrl,
    List<string>? Tags
);