﻿using FTM.Lib.Feeds;

namespace FTM.Lib.Tests.Feeds;

public class FeedHttpClientTests : TestBase
{
    [TestCase("https://www.teslarati.com/feed/")]
    [TestCase("https://production-ready.de/feed/de.xml")]
    public async Task ReadString_Should_ReturnNonEmptyString(string uri)
    {
        using var httpClient = new HttpClient();
        var result = await FeedHttpClient.ReadString(new Uri(uri), httpClient, etag: null, CancellationToken.None);

        result.ContentHasChanged.ShouldBeTrue();
        result.Content.ShouldNotBeNull();
    }

    [Theory]
    [TestCase("", false)]
    [TestCase(null, false)]
    [TestCase("W/\"9443658e7a59be38c14d4d3e1553a623\"", true)]
    [TestCase("\"d2a66f246020651404343d58a8394819-ssl\"", true)]
    [TestCase("\"63c9aecc-2b77a\"", true)]
    [TestCase("RSS-d740e7ca1c01c664422af73be1695061", false)]
    [TestCase("\"abcd", false)]
    [TestCase("abcd\"", false)]
    public void IsValidETag_Should_ReturnExpected(string? etag, bool expected)
    {
        var actual = FeedHttpClient.IsValidETag(etag);
        actual.ShouldBe(expected);
    }
}