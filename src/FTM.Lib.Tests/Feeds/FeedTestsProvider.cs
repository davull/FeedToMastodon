using System.Security.Cryptography;
using System.Text;
using FTM.Lib.Feeds;

namespace FTM.Lib.Tests.Feeds;

public static class FeedTestsProvider
{
    private const string TestFeedDir = "TestFeeds";

    public static IEnumerable<TestCaseData> ValidRssContentTestCases()
        => GetFeedContentTestCases();

    public static IEnumerable<TestCaseData> AtomFeedContentTestCases()
    {
        var atomFeeds = new[]
        {
            "cleancoder.com.xml",
            "heise.de.xml",
            "home-assistant.io.xml",
            "production-ready.de.xml",
            "wdr.de.xml",
            "YouTube - Linus Tech Tips.xml",
            "ich-roll-dann-mal-aus.xml"
        };

        return GetFeedContentTestCases(atomFeeds);
    }

    public static IEnumerable<TestCaseData> RssFeedContentTestCases()
    {
        var rssFeeds = new[]
        {
            "alltagsforschung.xml",
            "andrewlock.net.xml",
            "ardalis.com.xml",
            "bioinfowelten.uni-jena.de.xml",
            "devblogs.microsoft.com.xml",
            "dkriesel.com.xml",
            "driveteslacanada.ca.xml",
            "elektroauto-news.net.xml",
            "embarc.de.xml",
            "fernuni-hagen.de.xml",
            "golem.de.xml",
            "heiseplus.xml",
            "howtoforge.com.xml",
            "hyper-v-server.de.xml",
            "kernel-error.de.xml",
            "khalidabuhakmeh.com.xml",
            "ksta.de.xml",
            "nextcloud.com.xml",
            "Nextmove.de.xml",
            "Persoenlichkeits-Blog.xml",
            "ploeh.dk.xml",
            "roots-at-eifel.net.xml",
            "rosehosting.com.xml",
            "silicon.de.xml",
            "stadt-bremerhaven.de.xml",
            "stevetalkscode.co.uk.xml",
            "stonewars.de.xml",
            "tagesschau.de.xml",
            "technikblog.ch.xml",
            "teslarati.com.xml",
            "thereformedprogrammer.net.xml",
            "WDR 5 Zeitzeichen.xml",
            "WindowsPro.xml",
            "Yet another developer blog.xml"
        };

        return GetFeedContentTestCases(rssFeeds);
    }

    public static IEnumerable<TestCaseData> RdfFeedContentTestCases()
    {
        var rdfFeeds = new[] { "orf.at.xml" };
        return GetFeedContentTestCases(rdfFeeds);
    }

    private static IEnumerable<TestCaseData> GetFeedContentTestCases(string[]? fileNames = null)
    {
        var directory = Path.Combine(PathHelper.GetCurrentFileDirectory(), TestFeedDir);

        var filePaths = Directory.EnumerateFiles(directory);

        if (fileNames is { Length: > 0 })
        {
            filePaths = from filePath in filePaths
                let fileName = Path.GetFileName(filePath)
                where fileNames.Contains(fileName)
                select filePath;
        }

        return from filePath in filePaths
            let content = File.ReadAllText(filePath)
            let testName = Path.GetFileNameWithoutExtension(filePath)
            select new TestCaseData(content).SetName(testName);
    }

    public static IEnumerable<TestCaseData> FeedsTestCases()
    {
        return from feed in TestFeeds()
            select new TestCaseData(feed).SetName(feed.Title);
    }

    public static IEnumerable<TestCaseData> FeedItemsTestCases()
    {
        return from tuple in TestFeedsWithSeparators()
            let feed = tuple.feed
            let separators = tuple.separators
            from item in feed.Items
            let testName = GetTestName(feed, item)
            select new TestCaseData(item, separators).SetName(testName);
    }

    public static IEnumerable<TestCaseData> LessFeedItemsTestCases()
    {
        return from tuple in TestFeedsWithSeparators()
            let feed = tuple.feed
            let separators = tuple.separators
            from item in feed.Items.Take(5)
            let testName = GetTestName(feed, item)
            select new TestCaseData(item, separators).SetName(testName);
    }

    public static IEnumerable<TestCaseData> FeedItemsWithSeparatorTestCases()
    {
        return from tuple in TestFeedsWithSeparators()
            let feed = tuple.feed
            let separators = tuple.separators
            where separators.Length > 0
            from item in feed.Items
            let testName = GetTestName(feed, item)
            select new TestCaseData(item, separators).SetName(testName);
    }

    public static IEnumerable<FeedItem> TestFeedItems()
    {
        return from feed in TestFeeds()
            from item in feed.Items
            select item;
    }

    private static IEnumerable<Feed> TestFeeds()
    {
        var directory = Path.Combine(PathHelper.GetCurrentFileDirectory(), TestFeedDir);

        return from file in Directory.EnumerateFiles(directory)
            select FeedReader.Read(File.ReadAllText(file));
    }

    private static IEnumerable<(Feed feed, string[] separators)> TestFeedsWithSeparators()
    {
        var separators = new Dictionary<string, string[]>
        {
            { "Caschys Blog", ["...Zum Beitrag"] },
            { "StoneWars", ["[...]"] },
            { "TESLARATI", ["[…]", "\nThe post"] },
            { "nextmove", ["[…]"] },
            { "Elektroauto-News.net", ["\nDer Beitrag "] },
            { "Drive Tesla", ["[…]"] },
            { "DER Persönlichkeits-Blog", ["[…]"] },
            { "Technikblog", ["\nDer Beitrag", "[…]"] },
            { "The Reformed Programmer", ["…"] },
            { "BioinfoWelten", ["...\nThe post"] },
            { "Nextcloud", ["\nThe post"] },
            { "RoseHosting", ["... \nRead More"] }
        };

        return from feed in TestFeeds()
            let sep = separators.GetValueOrDefault(feed.Title!, [])
            select (feed, sep);
    }

    private static string GetTestName(Feed feed, FeedItem item)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(item.ItemId));
        var md5 = Convert.ToHexString(bytes);
        return $"{feed.Title} - {md5}";
    }

    public static IEnumerable<(string name, string url)> TestFeedUrls()
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
            ("fernuni-hagen.de", "https://www.fernuni-hagen.de/ssi/universitaet/aktuelles_rss.xml"),
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
            ("digitaltrends.com", "https://www.digitaltrends.com/rss/"),
            ("techradar.com", "https://www.techradar.com/rss"),
            ("businessinsider.de", "https://www.businessinsider.de/feed/businessinsider-alle-artikel"),
            ("elektroauto-news.net", "https://www.elektroauto-news.net/feed"),
            ("WDR 5 Zeitzeichen", "https://www1.wdr.de/mediathek/audio/zeitzeichen/zeitzeichen-podcast-100.podcast"),
            ("driveteslacanada.ca", "https://driveteslacanada.ca/feed")
        };

        return feeds;
    }
}