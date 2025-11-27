using FTM.Lib.Mastodon;

namespace FTM.Lib.Tests.Mastodon;

public class StatusTextProviderTests : TestBase
{
    [TestCaseSource(nameof(GetTagsTestCases))]
    public void GetTags_Should_RespectMaxLength(string[] tags, int maxLength, string expected)
    {
        var actual = StatusTextProvider.GetTags(tags, maxLength);
        actual.ShouldBe(expected);
    }

    private static IEnumerable<TestCaseData> GetTagsTestCases()
    {
        yield return new TestCaseData(Array.Empty<string>(), 50, "")
            .SetName("No tags");

        yield return new TestCaseData(new[] { "Tag001" }, 50, "#Tag001")
            .SetName("Single tag within limit");

        yield return new TestCaseData(new[] { "Tag001", "Tag002", "Tag003" }, 50,
            "#Tag001 #Tag002 #Tag003").SetName("Multiple tags within limit");

        yield return new TestCaseData(new[] { "Tag001", "Tag002" }, 15, "#Tag001 #Tag002")
            .SetName("Multiple tags exactly at limit");

        yield return new TestCaseData(new[] { "Tag001", "Tag002" }, 14, "#Tag001")
            .SetName("Multiple tags just under limit");

        yield return new TestCaseData(new[] { "Tag001", "Tag002" }, 16, "#Tag001 #Tag002")
            .SetName("Multiple tags just over limit");

        yield return new TestCaseData(new[] { "Tag001", "Tag002", "Tag003", "Tag004" }, 20,
            "#Tag001 #Tag002").SetName("Multiple tags way over limit");

        yield return new TestCaseData(new[] { "VeryLongTagName001", "Tag002" }, 15, "")
            .SetName("Single tag exceeds limit");
    }

    [Test]
    public void GetText_ShouldMatchSnapshot()
    {
        var actual = StatusTextProvider.GetText(
            title: "This is a sample title for testing purposes.",
            summary: "This is a sample summary that provides additional context to the title.",
            tags: ["SampleTag", "Testing"],
            link: new Uri("https://example.com/sample-article"),
            maxLength: 500);
        actual.MatchSnapshot();
    }

    [Test]
    public void GetText_WoLink_ShouldMatchSnapshot()
    {
        var actual = StatusTextProvider.GetText(
            title: "This is a sample title for testing purposes.",
            summary: "This is a sample summary that provides additional context to the title.",
            tags: ["SampleTag", "Testing"],
            link: null,
            maxLength: 50);
        actual.MatchSnapshot();
    }

    [TestCaseSource(nameof(GetEdgesTestCases))]
    public void GetText_EdgeCases_ShouldNotExceedLimit(string title, string summary, string[] tags)
    {
        const int maxLength = 50;

        var actual = StatusTextProvider.GetText(title, summary, tags, new Uri("https://example.com/abc"), maxLength);
        actual.Length.ShouldBeLessThanOrEqualTo(maxLength);
    }

    private static IEnumerable<TestCaseData> GetEdgesTestCases()
    {
        yield return new TestCaseData("Test title", "Test summary", new[] { "tag01", "tag02", "tag03" })
            .SetName("No space remaining after tags");

        yield return new TestCaseData("Test title", "Test summary", new[] { "tag1", "tag2", "tag3" })
            .SetName("Not enough space remaining after tags 1");

        yield return new TestCaseData("Test title", "Test summary", new[] { "tag01", "tag02" })
            .SetName("Not enough space remaining after tags 2");

        yield return new TestCaseData("Test title", "Test summary", new[] { "tag12345" })
            .SetName("Minimum space for title");

        yield return new TestCaseData("", "Test summary", new[] { "tag01", "tag02", "tag03" })
            .SetName("w/o title, no space remaining after tags");

        yield return new TestCaseData("", "Test summary", new[] { "tag01", "tag02" })
            .SetName("w/o title, not enough space remaining after tags");

        yield return new TestCaseData("", "Test summary", new[] { "tag12345" })
            .SetName("w/o title, minimum space for summary");

        yield return new TestCaseData("Test title abc", "Test summary", Array.Empty<string>())
            .SetName("w/o tags, not enough space remaining after title");
    }
}