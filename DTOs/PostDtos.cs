using System.ComponentModel.DataAnnotations;

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
    [Required, StringLength(200, MinimumLength = 1)] string Title,
    [Required, StringLength(200), RegularExpression(PostRules.SlugPattern, ErrorMessage = "Slug may only contain lowercase letters, digits and single hyphens.")] string Slug,
    [Required] string Content,
    [StringLength(500)] string? Excerpt,
    [StringLength(2048)] string? HeroImageUrl,
    [MaxLength(20)] List<string>? Tags
);

public record UpdatePostDto(
    [Required, StringLength(200, MinimumLength = 1)] string Title,
    [Required, StringLength(200), RegularExpression(PostRules.SlugPattern, ErrorMessage = "Slug may only contain lowercase letters, digits and single hyphens.")] string Slug,
    [Required] string Content,
    [StringLength(500)] string? Excerpt,
    [StringLength(2048)] string? HeroImageUrl,
    [MaxLength(20)] List<string>? Tags
);

public static class PostRules
{
    public const string SlugPattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$";
}
