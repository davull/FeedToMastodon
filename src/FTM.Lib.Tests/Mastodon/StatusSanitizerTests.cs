using FTM.Lib.Mastodon;
using FTM.Lib.Tests.Extensions;
using FTM.Lib.Tests.Feeds;

namespace FTM.Lib.Tests.Mastodon;

public class StatusSanitizerTests : TestBase
{
    [TestCase(null, null)]
    [TestCase("", "")]
    [TestCase(" ", "")]
    [TestCase("test", "test")]
    [TestCase("<p>test</p>", "test")]
    [TestCase("<p>>test</p>", ">test")]
    [TestCase("<p>test<</p>", "test<")]
    [TestCase("<p><b>test</b></p>", "test")]
    [TestCase("<p><b>test</p>", "test")]
    [TestCase("<p><b type='very-bold'>test</b></p>", "test")]
    [TestCase("<p>lorem</p> ipsum <p>dolor sit</p>", "lorem ipsum dolor sit")]
    public void Sanitize_Should_Remove_Html_Tags(string? content, string? expected)
    {
        var actual = StatusSanitizer.Sanitize(content);
        actual.Should().BeEquivalentTo(expected);
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.FeedsTestCases))]
    public void FeedDescription_Should_BeSanitized(Feed feed)
    {
        var feedId = feed.Id;
        var raw = feed.Description;
        var sanitized = StatusSanitizer.Sanitize(raw);

        var snapshot = new { feedId, raw, sanitized };
        snapshot.Should().MatchSnapshotWithTestName();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.FeedsTestCases))]
    public void FeedItemTitle_Should_BeSanitized(Feed feed)
    {
        var snapshot = new List<object>();

        foreach (var item in feed.Items)
        {
            var itemId = item.ItemId;
            var raw = item.Title;
            var sanitized = StatusSanitizer.Sanitize(raw);

            snapshot.Add(new { itemId, raw, sanitized });
        }

        snapshot.Should().MatchSnapshotWithTestName();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.FeedsTestCases))]
    public void FeedItemSummary_Should_BeSanitized(Feed feed)
    {
        var snapshot = new List<object>();

        foreach (var item in feed.Items)
        {
            var itemId = item.ItemId;
            var raw = item.Summary;
            var sanitized = StatusSanitizer.Sanitize(raw);

            snapshot.Add(new { itemId, raw, sanitized });
        }

        snapshot.Should().MatchSnapshotWithTestName();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.FeedsTestCases))]
    public void FeedItemContent_Should_BeSanitized(Feed feed)
    {
        var snapshot = new List<object>();

        foreach (var item in feed.Items)
        {
            var itemId = item.ItemId;
            var raw = item.Content;
            var sanitized = StatusSanitizer.Sanitize(raw);

            snapshot.Add(new { itemId, raw, sanitized });
        }

        snapshot.Should().MatchSnapshotWithTestName();
    }
}