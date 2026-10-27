using HomelabBlog.Api.Services;

namespace HomelabBlog.Api.Tests;

public class TagNamesTests
{
    [Theory]
    [InlineData("Kubernetes", "kubernetes")]
    [InlineData("Argo CD", "argo-cd")]
    [InlineData("  K8s!! ", "k8s")]
    [InlineData("C# / .NET", "c-net")]
    [InlineData("---", "")]
    public void ToSlug_NormalizesNames(string input, string expected)
    {
        Assert.Equal(expected, TagNames.ToSlug(input));
    }

    [Fact]
    public void Normalize_DeduplicatesBySlugKeepingFirstSpelling()
    {
        var result = TagNames.Normalize(["K8s", "k8s", " Argo CD", "argo-cd", ""]);

        Assert.Equal([("K8s", "k8s"), ("Argo CD", "argo-cd")], result);
    }

    [Fact]
    public void Normalize_DropsNamesWithoutUsableSlug()
    {
        Assert.Empty(TagNames.Normalize(["!!!", "   "]));
    }
}
