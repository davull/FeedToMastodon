using FluentAssertions;
using FTM.Lib.Mastodon;
using NUnit.Framework;

namespace FTM.Lib.Tests.Mastodon;

public class ContentComparerTests
{
    [Test]
    public void Same_Content_Should_Return_Equal()
    {
        const string content1 = "lorem ipsum";
        const string content2 = "lorem ipsum";

        var actual = ContentComparer.Compare(content1, content2);

        actual.Should().Be(ContentComparer.CompareResult.Equal);
    }

    [Test]
    public void Different_Content_Should_Return_NotEqual()
    {
        const string content1 = "lorem ipsum";
        const string content2 = "dolor sit amet";

        var actual = ContentComparer.Compare(content1, content2);

        actual.Should().Be(ContentComparer.CompareResult.NotEqual);
    }
}