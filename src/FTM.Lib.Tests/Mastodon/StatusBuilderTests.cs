using FTM.Lib.Mastodon;
using FTM.Lib.Tests.Extensions;
using FTM.Lib.Tests.Feeds;

namespace FTM.Lib.Tests.Mastodon;

public class StatusBuilderTests : TestBase
{
    private const int MaxStatusLength = 500;

    [Test]
    public void StatusContent_WithAllValues_ShouldUseSummary()
    {
        var feedItem = Dummies.FeedItem(
            title: "My post title",
            summary: "My summary",
            content: "My content",
            link: "https://example.com/feed=123");

        var status = StatusBuilder.CreateStatus(feedItem, ["tag1", "tag2"], [], MaxStatusLength, "en-US").Status;

        status.ShouldContain("My post title");
        status.ShouldContain("My summary");
        status.ShouldNotContain("My content");
        status.ShouldContain("https://example.com/feed=123");
        status.ShouldContain("#tag1");
        status.ShouldContain("#tag2");
    }

    [Test]
    public void StatusContent_WithAllValues_ShouldMatchSnapshot()
    {
        var feedItem = Dummies.FeedItem(
            title: "My post title",
            summary: "My summary",
            content: "My content",
            link: "https://example.com/feed=123");

        var status = StatusBuilder.CreateStatus(feedItem, ["tag1", "tag2"], [], MaxStatusLength, "en-US").Status;

        status.MatchSnapshot();
    }

    [Test]
    public void StatusContent_WoSummary_ShouldUseContent()
    {
        var feedItem = Dummies.FeedItem(
            title: "My post title",
            summary: "",
            content: "My content",
            link: "https://example.com/feed=123");

        var status = StatusBuilder.CreateStatus(feedItem, [], [], MaxStatusLength, "en-US").Status;

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

        var status = StatusBuilder.CreateStatus(feedItem, [], [], MaxStatusLength, "en-US").Status;

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
        var status = StatusBuilder.CreateStatus(feedItem, [], [separator], MaxStatusLength, "en-US").Status;

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
        var status = StatusBuilder.CreateStatus(feedItem, [], [separator], MaxStatusLength, "en-US").Status;

        const string expected = """
                                My post title

                                My Summary...
                                ---
                                https://example.com/feed=123
                                """;
        status.ShouldBe(expected);
    }

    [Test]
    public void StatusContent_WithMultipleSeparators_ShouldBeTrimmedAtFirstSeparator()
    {
        var feedItem = Dummies.FeedItem(
            title: "My post title",
            summary: "My Summary[...]This is the content[---]This is the rest",
            content: "",
            link: "https://example.com/feed=123");

        string[] separators = ["[...]", "[---]"];
        var status = StatusBuilder.CreateStatus(feedItem, [], separators, MaxStatusLength, "en-US").Status;

        const string expected = """
                                My post title

                                My Summary...
                                ---
                                https://example.com/feed=123
                                """;
        status.ShouldBe(expected);
    }

    [Test]
    public void StatusContent_WithMultipleSeparators_ShouldBeTrimmedAtSecondSeparator()
    {
        var feedItem = Dummies.FeedItem(
            title: "My post title",
            summary: "My Summary. This is the content[---]This is the rest",
            content: "",
            link: "https://example.com/feed=123");

        string[] separators = ["[...]", "[---]"];
        var status = StatusBuilder.CreateStatus(feedItem, [], separators, MaxStatusLength, "en-US").Status;

        const string expected = """
                                My post title

                                My Summary. This is the content...
                                ---
                                https://example.com/feed=123
                                """;
        status.ShouldBe(expected);
    }

    [Theory]
    [TestCase([new string[0]])]
    [TestCase([new[] { "" }])]
    [TestCase([new[] { "", "" }])]
    public void StatusContent_WoSeparator_ShouldNotBeTrimmed(string[] separators)
    {
        var feedItem = Dummies.FeedItem(
            title: "My post title",
            summary: "My Summary. [...] This is the content",
            content: "",
            link: "https://example.com/feed=123");

        var status = StatusBuilder.CreateStatus(feedItem, [], separators, MaxStatusLength, "en-US").Status;

        const string expected = """
                                My post title

                                My Summary. [...] This is the content
                                ---
                                https://example.com/feed=123
                                """;
        status.ShouldBe(expected);
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.LessFeedItemsTestCases))]
    public void StatusContent_Should_MatchSnapshot(FeedItem item, string[] separators)
    {
        var status = StatusBuilder.CreateStatus(item, [], separators, MaxStatusLength, "en-US");
        status.Status.MatchSnapshotWithTestName();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.LessFeedItemsTestCases))]
    public void Status_Should_MatchSnapshot(FeedItem item, string[] separators)
    {
        var status = StatusBuilder.CreateStatus(item, [], separators, MaxStatusLength, "en-US");

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
    public void Status_Should_HaveContentAndLink(FeedItem item, string[] separators)
    {
        var status = StatusBuilder.CreateStatus(item, [], separators, MaxStatusLength, "en-US");

        var split = status.Status.Split("---");

        split[0].ShouldNotBeNullOrWhiteSpace();
        split[1].ShouldNotBeNullOrWhiteSpace();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.FeedItemsTestCases))]
    public void Status_Should_HaveLanguage(FeedItem item, string[] separators)
    {
        var status = StatusBuilder.CreateStatus(item, [], separators, MaxStatusLength, "en-US");
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
    public void Status_ShouldBeSplitAtSeparator(FeedItem item, string[] separators)
    {
        var statusWithSeparator = StatusBuilder.CreateStatus(item, [], separators, MaxStatusLength, "en-US");
        var statusWoSeparator = StatusBuilder.CreateStatus(item, [], [], MaxStatusLength, "en-US");

        var snapshot = new
        {
            raw__ = statusWoSeparator.Status,
            split = statusWithSeparator.Status
        };
        snapshot.MatchSnapshotWithTestName();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.FeedItemsTestCases))]
    public void Status_ShouldNotExceedMaxLength(FeedItem item, string[] separators)
    {
        var status = StatusBuilder.CreateStatus(item, ["tag1", "tag2"], separators, MaxStatusLength, "en-US");
        var split = status.Status.Split("---");

        const int maxLength = 500 - 23 - 4; // 23 for link and 4 for ---\n
        split[0].Length.ShouldBeLessThanOrEqualTo(maxLength);
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.FeedItemsWithTagsTestCases))]
    public void Status_WithTags_ShouldMatchSnapshot(FeedItem item, string[] tags)
    {
        var status = StatusBuilder.CreateStatus(item, tags, [], MaxStatusLength, "en-US").Status;
        status.MatchSnapshotWithTestName();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.FeedItemsTestCases))]
    public void Status_Should_NotExceedMaxLength(FeedItem item, string[] separators)
    {
        item = item with
        {
            // l=23, exactly the lenght that mastodon reserves for links
            Link = new Uri("https://example.com/abc")
        };

        var status = StatusBuilder.CreateStatus(item, [], separators, 500, "en-US");
        status.Status.Length.ShouldBeLessThanOrEqualTo(500);
    }

    [Test]
    public void Status_WithManyTags_ShouldNotExceedMaxLength()
    {
        var feedItem = Dummies.FeedItem(link: "https://example.com/abc");
        var tags = Enumerable.Range(1, 100).Select(i => $"tag{i:000}").ToArray();

        var status = StatusBuilder.CreateStatus(feedItem, tags, [], MaxStatusLength, "en-US").Status;
        status.Length.ShouldBeLessThanOrEqualTo(500);
    }

    [Test]
    public void Status_WithManyTags_ShouldMatchSnapshot()
    {
        var feedItem = Dummies.FeedItem();
        var tags = Enumerable.Range(1, 100).Select(i => $"tag{i:000}").ToArray();

        var status = StatusBuilder.CreateStatus(feedItem, tags, [], MaxStatusLength, "en-US").Status;
        status.MatchSnapshot();
    }

    [TestCaseSource(nameof(MaxStatusLengthTestCases))]
    public void Status_ShouldNotExceedMaxLenght(string title, string summary,
        string content, string link, string[] tags, int maxStatusLength)
    {
        var feedItem = Dummies.FeedItem(title: title, summary: summary, content: content, link: link);
        var status = StatusBuilder.CreateStatus(feedItem, tags, [], maxStatusLength, "en-US").Status;

        status.Length.ShouldBeLessThanOrEqualTo(maxStatusLength);
    }

    private static IEnumerable<TestCaseData> MaxStatusLengthTestCases()
    {
        string[] titles =
        [
            "",
            "Short title",
            "Medium long title: Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
            "Very long title: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat."
        ];

        string[] summaries =
        [
            "",
            "Short summary.",
            "Medium long summary: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            "Very long summary: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
        ];

        string[] contents =
        [
            "",
            "Short content.",
            "Medium long content: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            "Very long content: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
        ];

        string[] links =
        [
            "",
            "https://example.com/abc" // l=23, exactly the lenght that mastodon reserves for links
        ];

        string[][] tagsList =
        [
            [],
            ["tag1", "tag2", "tag3"],
            [
                "verylongtagname001", "verylongtagname002", "verylongtagname003",
                "verylongtagname004", "verylongtagname005"
            ]
        ];

        int[] maxStatusLengths = [50, MaxStatusLength, 5_000];

        var q = from title in titles
            from summary in summaries
            from content in contents
            from link in links
            from tags in tagsList
            from maxStatusLength in maxStatusLengths
            select (title, summary, content, link, tags, maxStatusLength);
        foreach (var (title, summary, content, link, tags, maxStatusLength) in q)
        {
            yield return new TestCaseData(title, summary, content, link, tags, maxStatusLength).SetName(
                $"Title length: {title.Length}, Summary length: {summary.Length}, Content length: {content.Length}");
        }
    }
}