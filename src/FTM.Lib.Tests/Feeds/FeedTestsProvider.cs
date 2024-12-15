using System.Security.Cryptography;
using System.Text;
using FTM.Lib.Feeds;
using NUnit.Framework;

namespace FTM.Lib.Tests.Feeds;

public static class FeedTestsProvider
{
    public static IEnumerable<TestCaseData> ValidRssContentTestCases()
    {
        return from file in TestFeedFiles()
            let content = File.ReadAllText(file)
            let testName = Path.GetFileNameWithoutExtension(file)
            select new TestCaseData(content).SetName(testName);
    }

    public static IEnumerable<TestCaseData> AtomFeedContentTestCases()
    {
        var directory = Path.Combine(PathHelper.GetCurrentFileDirectory(), "TestFeeds");

        var atomFeeds = new List<string>
        {
            "cleancoder.com.xml",
            "heise.de.xml",
            "home-assistant.io.xml",
            "production-ready.de.xml",
            "wdr.de.xml",
            "YouTube - Linus Tech Tips.xml"
        };

        return from fileName in atomFeeds
            let filePath = Path.Combine(directory, fileName)
            let content = File.ReadAllText(filePath)
            let testName = Path.GetFileNameWithoutExtension(filePath)
            select new TestCaseData(content).SetName(testName);
    }

    public static IEnumerable<TestCaseData> RssFeedContentTestCases()
    {
        var directory = Path.Combine(PathHelper.GetCurrentFileDirectory(), "TestFeeds");

        var rssFeeds = new List<string>
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

        return from fileName in rssFeeds
            let filePath = Path.Combine(directory, fileName)
            let content = File.ReadAllText(filePath)
            let testName = Path.GetFileNameWithoutExtension(filePath)
            select new TestCaseData(content).SetName(testName);
    }

    public static IEnumerable<TestCaseData> RdfFeedContentTestCases()
    {
        var directory = Path.Combine(PathHelper.GetCurrentFileDirectory(), "TestFeeds");

        var atomFeeds = new List<string>
        {
            "orf.at.xml"
        };

        return from fileName in atomFeeds
            let filePath = Path.Combine(directory, fileName)
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
        return from feed in TestFeeds()
            from item in feed.Items
            select new TestCaseData(item).SetName(GetTestName(feed, item));
    }

    public static IEnumerable<TestCaseData> LessFeedItemsTestCases()
    {
        return from feed in TestFeeds()
            from item in feed.Items.Take(5)
            select new TestCaseData(item).SetName(GetTestName(feed, item));
    }

    public static IEnumerable<TestCaseData> FeedItemsWithSeparatorTestCases()
    {
        var directory = Path.Combine(PathHelper.GetCurrentFileDirectory(), "TestFeeds");

        var data = new List<(string file, string separator)>
        {
            ("stadt-bremerhaven.de.xml", "...Zum Beitrag"),
            ("stonewars.de.xml", "[...]"),
            ("teslarati.com.xml", "[…]"),
            ("Nextmove.de.xml", "[…]"),
            ("elektroauto-news.net.xml", "\nDer Beitrag "),
            ("driveteslacanada.ca.xml", "[…]")
        };

        foreach (var (file, separator) in data)
        {
            var filePath = Path.Combine(directory, file);
            var feed = FeedReader.Read(File.ReadAllText(filePath));

            foreach (var item in feed.Items)
            {
                yield return new TestCaseData(item, separator)
                    .SetName(GetTestName(feed, item));
            }
        }
    }

    public static IEnumerable<FeedItem> TestFeedItems()
    {
        return from feed in TestFeeds()
            from item in feed.Items
            select item;
    }

    private static IEnumerable<Feed> TestFeeds()
    {
        return from file in TestFeedFiles()
            select FeedReader.Read(File.ReadAllText(file));
    }

    private static IEnumerable<string> TestFeedFiles()
    {
        var directory = Path.Combine(PathHelper.GetCurrentFileDirectory(), "TestFeeds");
        return Directory.EnumerateFiles(directory);
    }

    private static string GetTestName(Feed feed, FeedItem item)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(item.ItemId));
        var md5 = BitConverter.ToString(bytes).Replace("-", "");
        return $"{feed.Title} - {md5}";
    }
}