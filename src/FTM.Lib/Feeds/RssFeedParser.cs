using System.Xml.Linq;

namespace FTM.Lib.Feeds;

public class RssFeedParser : FeedParserBase
{
    private const string ContentNs = "http://purl.org/rss/1.0/modules/content/";

    public override bool CanRead(XDocument document)
    {
        return document.Root?.Name is { LocalName: "rss", NamespaceName: "" };
    }

    public override Feed ParseFeed(XDocument document)
    {
        var channel = document.Root!.Element("channel")!;

        var id = GetFeedId(channel);
        var website = GetLink(channel);
        var title = GetTitle(channel);
        var description = GetDescription(channel);
        var language = GetLanguage(channel);

        var items = channel.Elements("item")
            .Select(e => ParseItem(e, id, language))
            .ToList();

        var lastUpdated = GetFeedLastUpdated(channel, items);

        return new Feed(id, website, title, lastUpdated,
            description, language, items);
    }

    private static FeedItem ParseItem(XElement element, string feedId, string? language)
    {
        var itemId = GetItemId(element);
        var title = GetTitle(element);
        var publishDate = GetItemDate(element);
        var summary = GetDescription(element);
        var content = GetItemContent(element);
        var link = GetLink(element);

        return new FeedItem(feedId, itemId, title, publishDate,
            summary, content, language, link);
    }

    private static string GetFeedId(XElement element)
    {
        var potentialIds = new[]
        {
            element.Element(XName.Get("id", AtomNs))?.Value,
            GetLinkHref(element, XName.Get("link", AtomNs), "self"),
            element.Element("link")?.Value
        };

        return Array.Find(potentialIds, id => !string.IsNullOrEmpty(id))
               ?? throw new Exception("Feed has no id");
    }

    private static DateTimeOffset? GetFeedLastUpdated(
        XElement element, IEnumerable<FeedItem> items)
    {
        var updated =
            TryGetDate(element.Element("lastBuildDate")?.Value) ??
            TryGetDate(element.Element("pubDate")?.Value);

        if (updated is not null)
        {
            return updated;
        }

        return items
            .Select(item => item.PublishDate)
            .OrderByDescending(date => date)
            .FirstOrDefault();
    }

    private static string GetItemId(XElement element)
    {
        var potentialIds = new[]
        {
            element.Element("guid")?.Value,
            element.Element("link")?.Value
        };

        return Array.Find(potentialIds, id => !string.IsNullOrEmpty(id))
               ?? throw new Exception("Item has no id");
    }

    private static string? GetItemContent(XElement element)
        => element.Element(XName.Get("encoded", ContentNs))?.Value;

    private static DateTimeOffset? GetItemDate(XElement element)
        => TryGetDate(element.Element("pubDate")?.Value);

    private static string? GetTitle(XElement element)
        => element.Element("title")?.Value;

    private static string? GetDescription(XElement element)
        => element.Element("description")?.Value;

    private static string? GetLanguage(XElement element)
        => element.Element("language")?.Value;

    private static Uri? GetLink(XElement element)
    {
        var link = element.Element("link")?.Value;
        return !string.IsNullOrEmpty(link) ? new Uri(link) : null;
    }

    private static string? GetLinkHref(XElement element, XName xName, string? rel)
    {
        return element
            .Elements(xName)
            .FirstOrDefault(e => e.Attribute("rel")?.Value == rel)
            ?.Attribute("href")
            ?.Value;
    }
}