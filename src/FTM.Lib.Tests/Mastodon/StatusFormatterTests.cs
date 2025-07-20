using FTM.Lib.Mastodon;
using FTM.Lib.Tests.Extensions;

namespace FTM.Lib.Tests.Mastodon;

public class StatusFormatterTests : TestBase
{
    [TestCaseSource(nameof(TestCases))]
    public void GetStatus_Should_MatchSnapshot(
        string title, string summary, string tags, string? link)
    {
        var uri = string.IsNullOrEmpty(link) ? null : new Uri(link);

        var status = StatusFormatter.GetStatus(title, summary, tags, uri);

        status.MatchSnapshotWithTestName();
    }

    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(
                "Title", "Summary", "", "https://example.com")
            .SetName("WithTitleAndSummaryAndLink");

        yield return new TestCaseData(
                "Title", "Summary", "", null)
            .SetName("WithTitleAndSummaryAndNoLink");

        yield return new TestCaseData(
                "Title", "", "", "https://example.com")
            .SetName("WithTitleAndNoSummaryAndLink");

        yield return new TestCaseData(
                "Title", "", "", null)
            .SetName("WithTitleAndNoSummaryAndNoLink");

        yield return new TestCaseData(
                "", "Summary", "", "https://example.com")
            .SetName("WithNoTitleAndSummaryAndLink");

        yield return new TestCaseData(
                "Title", "Summary contains Title", "", "https://example.com")
            .SetName("SummaryContainsTitle");

        yield return new TestCaseData(
                "Title contains Summary", "Summary", "", "https://example.com")
            .SetName("TitleContainsSummary");

        yield return new TestCaseData(
                "Title", "Summary", "#tag1 #tag2 #tag3", "https://example.com")
            .SetName("WithTitleAndSummaryAndLinkAndTags");

        yield return new TestCaseData(
                "Title", "Summary", "#tag1 #tag2 #tag3", null)
            .SetName("WithTitleAndSummaryAndTagAndNoLink");

        yield return new TestCaseData(
                "Title", "", "#tag1 #tag2 #tag3", "https://example.com")
            .SetName("WithTitleAndTagAndNoSummaryAndLink");

        yield return new TestCaseData(
                "Title", "", "#tag1 #tag2 #tag3", null)
            .SetName("WithTitleAndTagAndNoSummaryAndNoLink");

        yield return new TestCaseData(
                "", "Summary", "#tag1 #tag2 #tag3", "https://example.com")
            .SetName("WithTagAndNoTitleAndSummaryAndLink");

        yield return new TestCaseData(
                "Title", "Summary contains Title", "#tag1 #tag2 #tag3", "https://example.com")
            .SetName("WithTagAndSummaryContainsTitle");

        yield return new TestCaseData(
                "Title contains Summary", "Summary", "#tag1 #tag2 #tag3", "https://example.com")
            .SetName("WithTagAndTitleContainsSummary");
    }
}