﻿using System.Xml.Linq;
using FluentAssertions;
using FTM.Lib.Feeds;
using FTM.Lib.Tests.Extensions;
using NUnit.Framework;

namespace FTM.Lib.Tests.Feeds;

public class RssFeedParserTests : TestBase
{
    private readonly RssFeedParser _sut = new();

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.RssFeedContentTestCases))]
    public void CanRead_WithRssFeeds_Should_ReturnTrue(string content)
    {
        var document = XDocument.Parse(content);
        var actual = _sut.CanRead(document);

        actual.Should().BeTrue();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.AtomFeedContentTestCases))]
    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.RdfFeedContentTestCases))]
    public void CanRead_WoRssFeeds_Should_ReturnFalse(string content)
    {
        var document = XDocument.Parse(content);
        var actual = _sut.CanRead(document);

        actual.Should().BeFalse();
    }

    [TestCaseSource(typeof(FeedTestsProvider), nameof(FeedTestsProvider.RssFeedContentTestCases))]
    public void ParseFeed_Should_MatchSnapshot(string content)
    {
        var document = XDocument.Parse(content);
        var actual = _sut.ParseFeed(document);

        actual.Should().MatchSnapshotWithTestName(options => options
            .IgnoreField("LastUpdatedTime")
            .IgnoreField("Items[*].PublishDate"));
    }
}