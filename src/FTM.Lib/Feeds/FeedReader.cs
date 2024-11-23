using System.Xml.Linq;

namespace FTM.Lib.Feeds;

public static class FeedReader
{
    private static readonly RssFeedParser RssFeedParser = new();
    private static readonly AtomFeedParser AtomFeedParser = new();
    private static readonly RdfFeedParser RdfFeedParser = new();

    public static async Task<Feed> Read(Uri url, HttpClient httpClient)
    {
        using var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return Read(content);
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