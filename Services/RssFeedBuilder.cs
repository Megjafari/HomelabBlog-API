using System.Xml.Linq;
using HomelabBlog.Api.DTOs;

namespace HomelabBlog.Api.Services;

public static class RssFeedBuilder
{
    public static string Build(
        IEnumerable<PostListItemDto> posts, string title, string description, string baseUrl)
    {
        baseUrl = baseUrl.TrimEnd('/');

        var items = posts.Select(p => new XElement("item",
            new XElement("title", p.Title),
            new XElement("link", $"{baseUrl}/blog/{p.Slug}"),
            new XElement("guid", new XAttribute("isPermaLink", "true"), $"{baseUrl}/blog/{p.Slug}"),
            p.PublishedAt is { } published
                ? new XElement("pubDate", DateTime.SpecifyKind(published, DateTimeKind.Utc).ToString("R"))
                : null,
            string.IsNullOrWhiteSpace(p.Excerpt) ? null : new XElement("description", p.Excerpt),
            p.Tags.Select(t => new XElement("category", t))));

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("rss",
                new XAttribute("version", "2.0"),
                new XElement("channel",
                    new XElement("title", title),
                    new XElement("link", baseUrl),
                    new XElement("description", description),
                    items)));

        return doc.Declaration + Environment.NewLine + doc.Root;
    }
}
