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
            link: new Uri("https://example.com/sample-article"));
        actual.MatchSnapshot();
    }
}