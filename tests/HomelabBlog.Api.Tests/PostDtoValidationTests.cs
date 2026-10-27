using System.ComponentModel.DataAnnotations;
using HomelabBlog.Api.DTOs;

namespace HomelabBlog.Api.Tests;

public class PostDtoValidationTests
{
    private static List<ValidationResult> Validate(CreatePostDto dto)
    {
        // Record constructor-parameter attributes are what MVC validates; mirror that here.
        var results = new List<ValidationResult>();
        var parameters = typeof(CreatePostDto).GetConstructors()[0].GetParameters();
        var values = new object?[] { dto.Title, dto.Slug, dto.Content, dto.Excerpt, dto.HeroImageUrl, dto.Tags };

        for (var i = 0; i < parameters.Length; i++)
        {
            var ctx = new ValidationContext(dto) { MemberName = parameters[i].Name, DisplayName = parameters[i].Name };
            var attributes = parameters[i].GetCustomAttributes(typeof(ValidationAttribute), true).Cast<ValidationAttribute>();
            Validator.TryValidateValue(values[i]!, ctx, results, attributes);
        }

        return results;
    }

    [Fact]
    public void ValidPost_HasNoErrors()
    {
        var dto = new CreatePostDto("My post", "my-post", "<p>Hi</p>", "Short", null, ["k8s"]);

        Assert.Empty(Validate(dto));
    }

    [Theory]
    [InlineData("Bad Slug")]
    [InlineData("UPPER")]
    [InlineData("double--hyphen")]
    [InlineData("-leading")]
    [InlineData("trailing-")]
    public void InvalidSlug_IsRejected(string slug)
    {
        var dto = new CreatePostDto("Title", slug, "Body", null, null, null);

        Assert.NotEmpty(Validate(dto));
    }

    [Fact]
    public void TooManyTags_IsRejected()
    {
        var tags = Enumerable.Range(0, 21).Select(i => $"t{i}").ToList();
        var dto = new CreatePostDto("Title", "title", "Body", null, null, tags);

        Assert.NotEmpty(Validate(dto));
    }
}
