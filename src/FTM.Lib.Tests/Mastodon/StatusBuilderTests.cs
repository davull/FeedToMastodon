using FTM.Lib.Mastodon;
using FTM.Lib.Tests.Extensions;
using FTM.Lib.Tests.Feeds;

namespace FTM.Lib.Tests.Mastodon;

public class StatusBuilderTests : TestBase
{
    [Test]
    public void StatusContent_WithAllValues_ShouldUseSummary()
    {
        var feedItem = Dummies.FeedItem(
            title: "My post title",
            summary: "My summary",
            content: "My content",
            link: "https://example.com/feed=123");

        var status = StatusBuilder.CreateStatus(feedItem, "").Status;

        status.ShouldContain("My post title");
        status.ShouldContain("My summary");
        status.ShouldNotContain("My content");
        status.ShouldContain("https://example.com/feed=123");
    }

    [Test]
    public void StatusContent_WoSummary_ShouldUseContent()
    {
        var feedItem = Dummies.FeedItem(
            title: "My post title",
            summary: "",
            content: "My content",
            link: "https://example.com/feed=123");

        var status = StatusBuilder.CreateStatus(feedItem, "").Status;

        status.ShouldContain("My post title");
        status.ShouldContain("My content");
        status.ShouldContain("https://example.com/feed=123");
    }

    [Test]
    public void StatusContent_WoSummaryAndContent_ShouldTrim()
    {
        var feedItem = Dummies.FeedItem(
            title: "My post title",
            summary: "",
            content: "",
            link: "https://example.com/feed=123");

        var status = StatusBuilder.CreateStatus(feedItem, "").Status;

        const string expected = """
                                My post title
                                ---
                                https://example.com/feed=123
                                """;
        status.ShouldBe(expected);
    }

    [Test]
    public void StatusContent_SummaryWoSeparator_ShouldNotBeTrimmed()
    {
        var feedItem = Dummies.FeedItem(
            title: "My post title",
            summary: "My Summary. This is the content",
            content: "",
            link: "https://example.com/feed=123");

        const string separator = "[...]";
        var status = StatusBuilder.CreateStatus(feedItem, separator).Status;

        const string expected = """
                                My post title

                                My Summary. This is the content
                                ---
                                https://example.com/feed=123
                                """;
        status.ShouldBe(expected);
    }

    [Test]
    public void StatusContent_WithSeparator_ShouldBeTrimmed()
    {
        var feedItem = Dummies.FeedItem(
            title: "My post title",
            summary: "My Summary[...]This is the content",
            content: "",
            link: "https://example.com/feed=123");

        const string separator = "[...]";
        var status = StatusBuilder.CreateStatus(feedItem, separator).Status;

        const string expected = """
                                My post title

                                My Summary...
                                ---
                                https://example.com/feed=123
                                """;
        status.ShouldBe(expected);
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.LessFeedItemsTestCases))]
    public void StatusContent_Should_MatchSnapshot(FeedItem item, string separator)
    {
        var status = StatusBuilder.CreateStatus(item, separator);
        status.Status.MatchSnapshotWithTestName();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.LessFeedItemsTestCases))]
    public void Status_Should_MatchSnapshot(FeedItem item, string separator)
    {
        var status = StatusBuilder.CreateStatus(item, separator);

        var indexes = new[]
        {
            status.Status.IndexOf('\r'),
            status.Status.IndexOf('\n'),
            50
        };
        var index = indexes
            .Where(i => i > 0)
            .Min();
        var snapshot = new
        {
            status.Language,
            status.Visibility,
            Status = status.Status[..index] + "..."
        };
        snapshot.MatchSnapshotWithTestName();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.FeedItemsTestCases))]
    public void Status_Should_HaveContentAndLink(FeedItem item, string separator)
    {
        var status = StatusBuilder.CreateStatus(item, separator);

        var split = status.Status.Split("---");

        split[0].ShouldNotBeNullOrWhiteSpace();
        split[1].ShouldNotBeNullOrWhiteSpace();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.FeedItemsTestCases))]
    public void Status_Should_HaveLanguage(FeedItem item, string separator)
    {
        var status = StatusBuilder.CreateStatus(item, separator);
        status.Language.ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    public void Items_Should_BeUniqueByFeedIdItemIdPublishDate()
    {
        var items = FeedTestsProvider.TestFeedItems();
        var groups = items
            .GroupBy(i => new
            {
                i.FeedId,
                i.ItemId,
                i.PublishDate
            })
            .ToList();

        foreach (var group in groups)
        {
            group.ShouldContainSingle();
        }
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.FeedItemsWithSeparatorTestCases))]
    public void Status_ShouldBeSplitAtSeparator(FeedItem item, string separator)
    {
        var statusWithSeparator = StatusBuilder.CreateStatus(item, separator);
        var statusWoSeparator = StatusBuilder.CreateStatus(item, "");

        var snapshot = new
        {
            raw__ = statusWoSeparator.Status,
            split = statusWithSeparator.Status
        };
        snapshot.MatchSnapshotWithTestName();
    }
}