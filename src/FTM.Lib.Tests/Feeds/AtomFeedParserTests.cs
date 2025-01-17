using System.Xml.Linq;
using FTM.Lib.Feeds;
using FTM.Lib.Tests.Extensions;

namespace FTM.Lib.Tests.Feeds;

public class AtomFeedParserTests : TestBase
{
    private readonly AtomFeedParser _sut = new();

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.AtomFeedContentTestCases))]
    public void CanRead_WithAtomFeeds_Should_ReturnTrue(string content)
    {
        var document = XDocument.Parse(content);
        var actual = _sut.CanRead(document);

        actual.ShouldBeTrue();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.RssFeedContentTestCases))]
    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.RdfFeedContentTestCases))]
    public void CanRead_WoAtomFeeds_Should_ReturnTrue(string content)
    {
        var document = XDocument.Parse(content);
        var actual = _sut.CanRead(document);

        actual.ShouldBeFalse();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.AtomFeedContentTestCases))]
    public void ParseFeed_Should_MatchSnapshot(string content)
    {
        var document = XDocument.Parse(content);
        var actual = _sut.ParseFeed(document);

        actual.MatchSnapshotWithTestName();
    }
}