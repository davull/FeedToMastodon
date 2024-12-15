using FluentAssertions;
using FTM.Lib.Mastodon;
using NUnit.Framework;

namespace FTM.Lib.Tests.Mastodon;

public class ContentComparerTests
{
    [TestCaseSource(nameof(TestCases))]
    public void Compare_Should_Return_Expected(string first, string second, ContentComparer.CompareResult expected)
    {
        var actual = ContentComparer.Compare(first, second);
        actual.Should().Be(expected);
    }

    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData("lorem ipsum", "lorem ipsum", ContentComparer.CompareResult.FirstContainsSecond)
            .SetName("Equal content");

        yield return new TestCaseData("lorem ipsum", "dolor sit amet", ContentComparer.CompareResult.Different)
            .SetName("Different content");

        yield return new TestCaseData("lorem ipsum dolor", "ipsum", ContentComparer.CompareResult.FirstContainsSecond)
            .SetName("First contains second");

        yield return new TestCaseData("lorem", "lorem ipsum", ContentComparer.CompareResult.SecondContainsFirst)
            .SetName("Second contains first");

        yield return new TestCaseData("lorem ipsum", " ", ContentComparer.CompareResult.FirstContainsSecond)
            .SetName("Second is white space");

        yield return new TestCaseData("lorem ipsum", "", ContentComparer.CompareResult.FirstContainsSecond)
            .SetName("Second is empty string");

        yield return new TestCaseData("", "lorem ipsum", ContentComparer.CompareResult.SecondContainsFirst)
            .SetName("First is empty string");

        yield return new TestCaseData("lorem ip...", "lorem ipsum", ContentComparer.CompareResult.SecondContainsFirst)
            .SetName("First contains three dots");

        yield return new TestCaseData("lorem ip…", "lorem ipsum", ContentComparer.CompareResult.SecondContainsFirst)
            .SetName("First contains ellipsis");

        yield return new TestCaseData("loremipsumdolor", "lorem ipsum dolor sit amet",
            ContentComparer.CompareResult.SecondContainsFirst).SetName("Whitespaces are trimmed");
    }
}