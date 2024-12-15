using System.Security.Cryptography;
using System.Text;
using FTM.Lib.Feeds;
using NUnit.Framework;

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
            "YouTube - Linus Tech Tips.xml"
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
            //"fernuni-hagen.de.xml",
            "golem.de.xml",
            "heiseplus.xml",
            "howtoforge.com.xml",
            "hyper-v-server.de.xml",
            "ich-roll-dann-mal-aus.xml",
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
            "visualstudiomagazine.com.xml",
            "WDR 5 Zeitzeichen.xml",
            "WindowsPro.xml",
            "Yet another developer blog.xml",
            "JLMelenchon-nitter.privacydev.net.xml",
            "EUDelegationUA-nitter.privacydev.net.xml"
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
            let separator = tuple.separator
            from item in feed.Items
            let testName = GetTestName(feed, item)
            select new TestCaseData(item, separator).SetName(testName);
    }

    public static IEnumerable<TestCaseData> LessFeedItemsTestCases()
    {
        return from tuple in TestFeedsWithSeparators()
            let feed = tuple.feed
            let separator = tuple.separator
            from item in feed.Items.Take(5)
            let testName = GetTestName(feed, item)
            select new TestCaseData(item, separator).SetName(testName);
    }

    public static IEnumerable<TestCaseData> FeedItemsWithSeparatorTestCases()
    {
        return from tuple in TestFeedsWithSeparators()
            let feed = tuple.feed
            let separator = tuple.separator
            where !string.IsNullOrEmpty(separator)
            from item in feed.Items
            let testName = GetTestName(feed, item)
            select new TestCaseData(item, separator).SetName(testName);
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

    private static IEnumerable<(Feed feed, string separator)> TestFeedsWithSeparators()
    {
        var separators = new Dictionary<string, string>
        {
            { "Caschys Blog", "...Zum Beitrag" },
            { "StoneWars", "[...]" },
            { "TESLARATI", "[…]" },
            { "nextmove", "[…]" },
            { "Elektroauto-News.net", "\nDer Beitrag " },
            { "Drive Tesla", "[…]" },
            { "Visual Studio Blog", "[…]" },
            { "DER Persönlichkeits-Blog", "[…]" },
            { "Technikblog", "[…]" },
            { "The Reformed Programmer", "…" }
        };

        return from feed in TestFeeds()
            let separator = separators.GetValueOrDefault(feed.Title!, "")
            select (feed, separator);
    }

    private static string GetTestName(Feed feed, FeedItem item)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(item.ItemId));
        var md5 = Convert.ToHexString(bytes);
        return $"{feed.Title} - {md5}";
    }
}