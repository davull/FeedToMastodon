using FluentAssertions;
using FTM.Lib.Feeds;
using NUnit.Framework;

namespace FTM.Lib.Tests.Feeds;

public class FeedParserBaseTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("invalid-date-string")]
    public void TryGetDate_WithInvalidString_ShouldReturnNull(string? value)
    {
        var actual = FeedParserBase.TryGetDate(value);
        actual.Should().BeNull();
    }
   
    [TestCase("2024-01-31")]
    [TestCase("2024-01-31 12:00:00")]
    [TestCase("Tue, 08 Mar 2022 18:39:11 +0000")]
    [TestCase("Sun, 03 Nov 2024 22:33:59 GMT")]
    [TestCase("2023-01-19T20:57:45+00:00")]
    [TestCase("2024-11-04T20:00:00+01:00")]
    [TestCase("2024-09-26T07:37:26+00:00")]
    [TestCase("Tue, 04 Jun 2024 11:24:33 GMT")]
    [TestCase("2024-11-04T19:48:33Z")]
    [TestCase("Mon, 04 Nov 2024 07:46:29 UTC")]
    [TestCase("Mon, 07 Oct 2024 18:37:00 UTC")]
    [TestCase("30.10.2024")]
    [TestCase("Mo, 04 Nov 2024 13:22:06 +0100")]
    public void TryGetDate_WithValidString_ShouldReturnValue(string value)
    {
        var actual = FeedParserBase.TryGetDate(value);
        actual.Should().NotBeNull();
    }
}