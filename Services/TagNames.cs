using System.Text.RegularExpressions;

namespace HomelabBlog.Api.Services;

public static class TagNames
{
    public static string ToSlug(string name) =>
        Regex.Replace(name.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');

    /// <summary>Trims, drops empty names and de-duplicates by slug (first spelling wins).</summary>
    public static List<(string Name, string Slug)> Normalize(IEnumerable<string> names) =>
        names
            .Select(n => n.Trim())
            .Where(n => n.Length > 0)
            .Select(n => (Name: n, Slug: ToSlug(n)))
            .Where(t => t.Slug.Length > 0)
            .GroupBy(t => t.Slug)
            .Select(g => g.First())
            .ToList();
}
