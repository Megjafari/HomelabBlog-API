using System.Xml.Linq;
using HomelabBlog.Api.DTOs;
using HomelabBlog.Api.Services;

namespace HomelabBlog.Api.Tests;

public class RssFeedBuilderTests
{
    private static PostListItemDto Post(string title, string slug, string? excerpt = null) =>
        new(Guid.NewGuid(), title, slug, excerpt, null, new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc), ["k8s"]);

    [Fact]
    public void Build_ProducesValidRssWithAbsoluteLinks()
    {
        var xml = RssFeedBuilder.Build([Post("Hello", "hello", "Intro")], "Journal", "Desc", "https://blog.example.com/");

        var channel = XDocument.Parse(xml).Root!.Element("channel")!;
        var item = channel.Element("item")!;

        Assert.Equal("Journal", channel.Element("title")!.Value);
        Assert.Equal("https://blog.example.com", channel.Element("link")!.Value);
        Assert.Equal("https://blog.example.com/blog/hello", item.Element("link")!.Value);
        Assert.Equal("Intro", item.Element("description")!.Value);
        Assert.Equal("k8s", item.Element("category")!.Value);
        Assert.Equal("Thu, 01 Oct 2026 12:00:00 GMT", item.Element("pubDate")!.Value);
    }

    [Fact]
    public void Build_EscapesSpecialCharactersInTitles()
    {
        var xml = RssFeedBuilder.Build([Post("A & B <script>", "a-b")], "J", "D", "https://x.test");

        var title = XDocument.Parse(xml).Root!.Element("channel")!.Element("item")!.Element("title")!.Value;

        Assert.Equal("A & B <script>", title);
    }

    [Fact]
    public void Build_WithoutPosts_StillReturnsChannel()
    {
        var xml = RssFeedBuilder.Build([], "J", "D", "https://x.test");

        Assert.Empty(XDocument.Parse(xml).Root!.Element("channel")!.Elements("item"));
    }
}
