using NUnit.Framework;

namespace FTM.Lib.Tests.Feeds;

public class UpdateTestFeeds
{
    [Explicit]
    [TestCaseSource(nameof(Feeds))]
    public async Task UpdateFeeds(string name, string url)
    {
        var directory = Path.Combine(PathHelper.GetCurrentFileDirectory(), "TestFeeds");
        var filePath = Path.Combine(directory, $"{name}.xml");

        var content = await ReadFeed(url);
        await File.WriteAllTextAsync(filePath, content);

        Assert.Pass();
    }

    private static async Task<string> ReadFeed(string url)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private static IEnumerable<TestCaseData> Feeds()
    {
        var feeds = new List<(string name, string url)>
        {
            ("teslarati.com", "https://www.teslarati.com/feed/"),
            ("production-ready.de", "https://production-ready.de/feed/de.xml"),
            ("ksta.de", "https://feed.ksta.de/feed/rss/region/euskirchen-eifel/index.rss"),
            ("heise.de", "https://www.heise.de/rss/heise-atom.xml"),
            ("bioinfowelten.uni-jena.de", "http://bioinfowelten.uni-jena.de/feed/"),
            ("alltagsforschung", "http://feeds.feedburner.com/alltagsforschung"),
            ("Persoenlichkeits-Blog", "http://feeds.feedburner.com/Persoenlichkeits-Blog"),
            ("howtoforge.com", "http://www.howtoforge.com/feed.rss"),
            ("nextcloud.com", "https://nextcloud.com/feed/"),
            ("ich-roll-dann-mal-aus", "https://www.heise.de/developer/rss/ich-roll-dann-mal-aus.rdf"),
            ("rosehosting.com", "https://www.rosehosting.com/blog/feed/"),
            ("silicon.de", "https://www.silicon.de/feed"),
            ("golem.de", "http://rss.golem.de/rss.php?feed=RSS2.0"),
            ("technikblog.ch", "http://feeds.feedburner.com/technikblog_ch"),
            ("hyper-v-server.de", "http://www.hyper-v-server.de/feed/"),
            ("WindowsPro", "http://feeds.feedburner.com/WindowsPro"),
            ("kernel-error.de", "https://www.kernel-error.de/kernel-error-blog?format=feed&amp;type=rss"),
            ("stadt-bremerhaven.de", "http://feeds2.feedburner.com/stadt-bremerhaven/dqXM"),
            ("visualstudiomagazine.com", "https://visualstudiomagazine.com/rss-feeds/news.aspx"),
            ("stonewars.de", "https://stonewars.de/feed"),
            ("ploeh.dk", "https://blog.ploeh.dk/rss"),
            ("cleancoder.com", "https://blog.cleancoder.com/atom.xml"),
            ("ardalis.com", "https://ardalis.com/rss.xml"),
            ("devblogs.microsoft.com", "https://devblogs.microsoft.com/visualstudio/feed/"),
            ("thereformedprogrammer.net", "https://www.thereformedprogrammer.net/feed/"),
            ("dkriesel.com", "https://www.dkriesel.com/feed.php"),
            ("khalidabuhakmeh.com", "https://khalidabuhakmeh.com/feed.xml"),
            ("home-assistant.io", "https://www.home-assistant.io/atom.xml"),
            ("andrewlock.net", "https://andrewlock.net/rss/"),
            ("stevetalkscode.co.uk", "https://stevetalkscode.co.uk/feed/index.rss"),
            //("fernuni-hagen.de", "https://www.fernuni-hagen.de/ssi/universitaet/aktuelles_rss.xml"),
            ("roots-at-eifel.net", "https://www.roots-at-eifel.net/rss"),
            ("Yet another developer blog", "https://feeds.feedburner.com/YetAnotherDeveloperBlog"),
            ("heiseplus", "https://www.heise.de/rss/heiseplus.rdf"),
            ("embarc.de", "https://www.embarc.de/blog/index.xml"),
            ("YouTube - Linus Tech Tips",
                "https://www.youtube.com/feeds/videos.xml?channel_id=UCXuqSBlHAE6Xw-yeJA0Tunw"),
            ("Nextmove.de", "https://nextmove.de/feed/"),
            ("tagesschau.de", "https://www.tagesschau.de/infoservices/alle-meldungen-100~rss2.xml"),
            ("wdr.de", "https://www1.wdr.de/uebersicht-100.feed"),
            ("orf.at", "https://rss.orf.at/news.xml"),
            ("wired.com", "https://www.wired.com/feed/rss"),
            ("gizmodo.com", "https://gizmodo.com/feed"),
            ("mashable.com", "https://mashable.com/feeds/rss/tech"),
            ("digitaltrends.com", "https://www.digitaltrends.com/feed/"),
            ("techradar.com", "https://www.techradar.com/rss"),
            ("businessinsider.de", "https://www.businessinsider.de/feed/businessinsider-alle-artikel"),
            ("elektroauto-news.net", "https://www.elektroauto-news.net/feed"),
            ("WDR 5 Zeitzeichen", "https://www1.wdr.de/mediathek/audio/zeitzeichen/zeitzeichen-podcast-100.podcast"),
            ("driveteslacanada.ca", "https://driveteslacanada.ca/feed")
        };

        foreach (var (name, url) in feeds)
        {
            yield return new TestCaseData(name, url)
                .SetName(name);
        }
    }
}