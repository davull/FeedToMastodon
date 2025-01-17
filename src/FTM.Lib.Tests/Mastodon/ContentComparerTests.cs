using FTM.Lib.Mastodon;

namespace FTM.Lib.Tests.Mastodon;

public class ContentComparerTests
{
    [TestCaseSource(nameof(TestCases))]
    public void Compare_Should_Return_Expected(string first, string second, ContentComparer.CompareResult expected)
    {
        var actual = ContentComparer.Compare(first, second);
        actual.ShouldBe(expected);
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

        yield return new TestCaseData("lorem / ipsum \\ dolor", "lorem ipsum dolor sit amet",
            ContentComparer.CompareResult.SecondContainsFirst).SetName("Slashes are trimmed");

        yield return new TestCaseData("lorem ipsum https://www.google.de/q=search dolor sit amet",
                "lorem ipsum www.google.de/q=search dolor sit...", ContentComparer.CompareResult.FirstContainsSecond)
            .SetName("With https://www url");

        yield return new TestCaseData("lorem ipsum https://web.google.de/q=search dolor sit amet",
                "lorem ipsum web.google.de/q=search dolor sit...", ContentComparer.CompareResult.FirstContainsSecond)
            .SetName("With https://web url");

        yield return new TestCaseData("lorem ipsum http://www.google.de/q=search dolor sit amet",
                "lorem ipsum www.google.de/q=search dolor sit...", ContentComparer.CompareResult.FirstContainsSecond)
            .SetName("With http://www url");

        yield return new TestCaseData("lorem ipsum http://web.google.de/q=search dolor sit amet",
                "lorem ipsum web.google.de/q=search dolor sit...", ContentComparer.CompareResult.FirstContainsSecond)
            .SetName("With http://web url");

        yield return new TestCaseData("lorem ipsum http://www.google.de/q=search dolor sit amet",
                "lorem ipsum www.google.com/q=search dolor sit...", ContentComparer.CompareResult.Different)
            .SetName("Different urls");

        yield return new TestCaseData("direto:https://www.europarl.europa.eu/plenary/pt/home.html",
                "direto:europarl.europa.eu/plenary/p…", ContentComparer.CompareResult.FirstContainsSecond)
            .SetName("Full url");
    }
}