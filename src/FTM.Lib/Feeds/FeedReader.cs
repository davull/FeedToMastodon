using System.Xml.Linq;

namespace FTM.Lib.Feeds;

public static class FeedReader
{
    private static readonly RssFeedParser RssFeedParser = new();
    private static readonly AtomFeedParser AtomFeedParser = new();
    private static readonly RdfFeedParser RdfFeedParser = new();

    public static async Task<Feed?> ReadIfChanged(Uri url, HttpClient httpClient, string? etag)
    {
        var result = await FeedHttpClient.ReadString(url, httpClient, etag);
        return result.ContentHasChanged
            ? Read(result.Content)
            : null;
    }

    public static Feed Read(string content)
    {
        var xDoc = XDocument.Parse(content);
        if (AtomFeedParser.CanRead(xDoc))
        {
            return AtomFeedParser.ParseFeed(xDoc);
        }

        if (RssFeedParser.CanRead(xDoc))
        {
            return RssFeedParser.ParseFeed(xDoc);
        }

        if (RdfFeedParser.CanRead(xDoc))
        {
            return RdfFeedParser.ParseFeed(xDoc);
        }

        throw new ArgumentException("Unknown feed format");
    }
}