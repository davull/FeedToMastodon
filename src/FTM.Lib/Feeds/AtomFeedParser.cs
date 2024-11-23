using System.Xml.Linq;

namespace FTM.Lib.Feeds;

public class AtomFeedParser : FeedParserBase
{
    private const string XmlNs = "http://www.w3.org/XML/1998/namespace";
    private const string MediaNs = "http://search.yahoo.com/mrss/";

    public override bool CanRead(XDocument document)
    {
        return document.Root?.Name is { LocalName: "feed", NamespaceName: AtomNs };
    }

    public override Feed ParseFeed(XDocument document)
    {
        var root = document.Root!;

        var id = GetFeedId(root);
        var website = GetAlternateLink(root);
        var title = GetTitle(root);
        var description = GetFeedDescription(root);
        const string? language = null;

        var items = root.Elements(XName.Get("entry", AtomNs))
            .Select(e => ParseItem(e, id))
            .ToList();

        var lastUpdated = GetFeedLastUpdated(root, items);

        return new Feed(id, website, title, lastUpdated,
            description, language, items);
    }

    private static FeedItem ParseItem(XElement element, string feedId)
    {
        var itemId = GetItemId(element);
        var title = GetTitle(element);
        var publishDate = GetItemDate(element);
        var summary = GetItemSummary(element);
        var content = GetItemContent(element);
        var language = GetItemLanguage(element);
        var link = GetAlternateLink(element);

        return new FeedItem(feedId, itemId, title, publishDate,
            summary, content, language, link);
    }

    private static string GetFeedId(XElement element)
    {
        var potentialIds = new[]
        {
            element.Element(XName.Get("id", AtomNs))?.Value,
            GetLink(element, "self")
        };

        return Array.Find(potentialIds, id => !string.IsNullOrEmpty(id))
               ?? throw new Exception("Feed has no id");
    }

    private static DateTimeOffset? GetFeedLastUpdated(
        XElement element, IEnumerable<FeedItem> items)
    {
        var updated = TryGetDate(element.Element(XName.Get("updated", AtomNs))?.Value);

        if (updated is not null)
        {
            return updated;
        }

        return items
            .Select(item => item.PublishDate)
            .OrderByDescending(date => date)
            .FirstOrDefault();
    }

    private static Uri? GetAlternateLink(XElement element)
    {
        var potentialLinks = new[]
        {
            GetLink(element, "alternate"),
            GetLink(element, null)
        };

        return potentialLinks
            .Where(link => !string.IsNullOrEmpty(link))
            .Select(link => new Uri(link!))
            .FirstOrDefault();
    }

    private static string? GetFeedDescription(XElement element)
        => element.Element(XName.Get("subtitle", AtomNs))?.Value;

    private static string GetItemId(XElement element)
    {
        return element.Element(XName.Get("id", AtomNs))?.Value
               ?? throw new Exception("Item has no id");
    }

    private static DateTimeOffset? GetItemDate(XElement element)
    {
        var potentialDates = new[]
        {
            element.Element(XName.Get("published", AtomNs))?.Value,
            element.Element(XName.Get("updated", AtomNs))?.Value
        };

        return potentialDates
            .Select(TryGetDate)
            .FirstOrDefault(date => date.HasValue);
    }

    private static string? GetItemSummary(XElement element)
        => element.Element(XName.Get("summary", AtomNs))?.Value;

    private static string? GetItemContent(XElement element)
    {
        var potentialContent = new[]
        {
            element
                .Element(XName.Get("content", AtomNs))?.Value,
            element
                .Element(XName.Get("group", MediaNs))?
                .Element(XName.Get("description", MediaNs))?.Value
        };

        return Array.Find(potentialContent, content => !string.IsNullOrEmpty(content));
    }

    private static string? GetItemLanguage(XElement element)
        => element.Attribute(XName.Get("lang", XmlNs))?.Value;

    private static string? GetTitle(XElement element)
        => element.Element(XName.Get("title", AtomNs))?.Value;

    private static string? GetLink(XElement element, string? rel)
    {
        return element
            .Elements(XName.Get("link", AtomNs))
            .FirstOrDefault(e => e.Attribute("rel")?.Value == rel)
            ?.Attribute("href")
            ?.Value;
    }
}