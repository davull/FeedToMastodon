using System.Xml.Linq;
using FluentAssertions;
using FTM.Lib.Feeds;
using FTM.Lib.Tests.Extensions;
using NUnit.Framework;

namespace FTM.Lib.Tests.Feeds;

public class RdfFeedParserTests : TestBase
{
    private readonly RdfFeedParser _sut = new();

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.RdfFeedContentTestCases))]
    public void CanRead_WithRdfFeeds_Should_ReturnTrue(string content)
    {
        var document = XDocument.Parse(content);
        var actual = _sut.CanRead(document);

        actual.Should().BeTrue();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.RssFeedContentTestCases))]
    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.AtomFeedContentTestCases))]
    public void CanRead_WoRdfFeeds_Should_ReturnFalse(string content)
    {
        var document = XDocument.Parse(content);
        var actual = _sut.CanRead(document);

        actual.Should().BeFalse();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.RdfFeedContentTestCases))]
    public void ParseFeed_Should_MatchSnapshot(string content)
    {
        var document = XDocument.Parse(content);
        var actual = _sut.ParseFeed(document);

        actual.Should().MatchSnapshotWithTestName();
    }
}