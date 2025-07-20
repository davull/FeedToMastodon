using FTM.Lib.Mastodon;
using FTM.Lib.Tests.Extensions;

namespace FTM.Lib.Tests.Mastodon;

public class StatusFormatterTests : TestBase
{
    [TestCaseSource(nameof(TestCases))]
    public void GetStatus_Should_MatchSnapshot(
        string title, string summary, string? link)
    {
        var uri = string.IsNullOrEmpty(link) ? null : new Uri(link);

        var status = StatusFormatter.GetStatus(title, summary, "", uri);

        status.MatchSnapshotWithTestName();
    }

    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(
                "Title", "Summary", "https://example.com")
            .SetName("WithTitleAndSummaryAndLink");

        yield return new TestCaseData(
                "Title", "Summary", null)
            .SetName("WithTitleAndSummaryAndNoLink");

        yield return new TestCaseData(
                "Title", "", "https://example.com")
            .SetName("WithTitleAndNoSummaryAndLink");

        yield return new TestCaseData(
                "Title", "", null)
            .SetName("WithTitleAndNoSummaryAndNoLink");

        yield return new TestCaseData(
                "", "Summary", "https://example.com")
            .SetName("WithNoTitleAndSummaryAndLink");

        yield return new TestCaseData(
            "Title", "Summary contains Title", "https://example.com")
            .SetName("SummaryContainsTitle");

        yield return new TestCaseData(
            "Title contains Summary", "Summary", "https://example.com")
            .SetName("TitleContainsSummary");
    }
}