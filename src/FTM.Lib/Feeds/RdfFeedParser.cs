using System.Xml.Linq;

namespace FTM.Lib.Feeds;

public class RdfFeedParser : FeedParserBase
{
    private const string Ns = "http://purl.org/rss/1.0/";
    private const string RdfNs = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private const string DcNs = "http://purl.org/dc/elements/1.1/";

    public override bool CanRead(XDocument document)
    {
        return document.Root?.Name is { LocalName: "RDF", NamespaceName: RdfNs };
    }

    public override Feed ParseFeed(XDocument document)
    {
        var channel = document.Root!.Element(XName.Get("channel", Ns))!;

        var id = GetId(channel);
        var website = GetLink(channel);
        var title = GetTitle(channel);
        var description = GetDescription(channel);
        var language = GetFeedLanguage(channel);
        var lastUpdated = GetDate(channel);

        var items = document.Root.Elements(XName.Get("item", Ns))
            .Select(e => ParseItem(e, id, language))
            .ToList();

        return new Feed(id, website, title, lastUpdated,
            description, language, items);
    }

    private static FeedItem ParseItem(XElement element, string feedId, string? language)
    {
        var itemId = GetId(element);
        var title = GetTitle(element);
        var publishDate = GetDate(element);
        var summary = GetDescription(element);
        var link = GetLink(element);

        return new FeedItem(feedId, itemId, title, publishDate,
            summary, null, language, link);
    }

    private static string? GetFeedLanguage(XElement element)
        => element.Element(XName.Get("language", DcNs))?.Value;

    private static string GetId(XElement element)
    {
        return element.Attribute(XName.Get("about", RdfNs))?.Value
               ?? throw new Exception("Feed has no id");
    }

    private static Uri? GetLink(XElement element)
    {
        var link = element.Element(XName.Get("link", Ns))?.Value;
        return !string.IsNullOrEmpty(link) ? new Uri(link) : null;
    }

    private static string? GetTitle(XElement element)
        => element.Element(XName.Get("title", Ns))?.Value;

    private static string? GetDescription(XElement element)
        => element.Element(XName.Get("description", Ns))?.Value;

    private static DateTimeOffset? GetDate(XElement element)
        => TryGetDate(element.Element(XName.Get("date", DcNs))?.Value);
}