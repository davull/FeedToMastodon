using FluentAssertions;
using FluentAssertions.Execution;
using FTM.Lib.Feeds;
using FTM.Lib.Tests.Extensions;
using NUnit.Framework;

namespace FTM.Lib.Tests.Feeds;

public class FeedReaderTests : TestBase
{
    [TestCase("https://www.teslarati.com/feed/")]
    [TestCase("https://production-ready.de/feed/de.xml")]
    public async Task CanReadFeedFromUri(string uri)
    {
        var httpClient = new HttpClient();
        var feed = await FeedReader.Read(new Uri(uri), httpClient);
        feed.Should().NotBeNull();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.ValidRssContentTestCases))]
    public void CanReadValidRssContent(string content)
    {
        var feed = FeedReader.Read(content);
        feed.Should().NotBeNull();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.ValidRssContentTestCases))]
    public void Properties_Should_BeSet(string content)
    {
        var feed = FeedReader.Read(content);

        using var _ = new AssertionScope();

        feed.Id.Should().NotBeNullOrEmpty();
        feed.Website.Should().NotBeNull();
        feed.Title.Should().NotBeNullOrEmpty();
        feed.Items.Should().NotBeEmpty();

        feed.Items.Should().AllSatisfy(item =>
        {
            item.ItemId.Should().NotBeNullOrEmpty();
            item.Title.Should().NotBeNullOrEmpty();
            item.Link.Should().NotBeNull();
        });
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.ValidRssContentTestCases))]
    public void Feed_Should_MatchSnapshot(string content)
    {
        var feed = FeedReader.Read(content);
        feed.Should().MatchSnapshotWithTestName(options => options
            .IgnoreField("LastUpdatedTime")
            .IgnoreField("Items[*].PublishDate")
        );
    }
}