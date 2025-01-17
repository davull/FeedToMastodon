using FTM.Lib.Feeds;
using FTM.Lib.Tests.Extensions;

namespace FTM.Lib.Tests.Feeds;

public class FeedReaderTests : TestBase
{
    [TestCase("https://www.teslarati.com/feed/")]
    [TestCase("https://production-ready.de/feed/de.xml")]
    public async Task CanReadFeedFromUri(string uri)
    {
        using var httpClient = new HttpClient();
        var feed = await FeedReader.ReadIfChanged(new Uri(uri), httpClient, etag: null, CancellationToken.None);

        feed.feed.ShouldNotBeNull();
        feed.etag.ShouldNotBeNullOrEmpty();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.ValidRssContentTestCases))]
    public void CanReadValidRssContent(string content)
    {
        var feed = FeedReader.Read(content);
        feed.ShouldNotBeNull();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.ValidRssContentTestCases))]
    public void Properties_Should_BeSet(string content)
    {
        var feed = FeedReader.Read(content);

        feed.Id.ShouldNotBeNullOrEmpty();
        feed.Website.ShouldNotBeNull();
        feed.Title.ShouldNotBeNullOrEmpty();
        feed.Items.ShouldNotBeEmpty();

        foreach (var item in feed.Items)
        {
            item.ItemId.ShouldNotBeNullOrEmpty();
            item.Title.ShouldNotBeNullOrEmpty();
            item.Link.ShouldNotBeNull();
        }
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.ValidRssContentTestCases))]
    public void Feed_Should_MatchSnapshot(string content)
    {
        var feed = FeedReader.Read(content);
        feed.MatchSnapshotWithTestName(options => options
            .IgnoreField("LastUpdatedTime")
            .IgnoreField("Items[*].PublishDate")
        );
    }
}