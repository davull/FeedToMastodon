using FluentAssertions;
using NUnit.Framework;

namespace FTM.Lib.Tests;

public class TimeSpanParserTests
{
    [TestCaseSource(nameof(TestCases))]
    public void Parse_Should_ReturnExpected(string? raw, TimeSpan? expected)
    {
        var actual = TimeSpanParser.Parse(raw);

        actual.Should().Be(expected);
    }

    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData(null, null);
        yield return new TestCaseData("", null);

        yield return new TestCaseData("00:00:00", new TimeSpan(00, 00, 00));
        yield return new TestCaseData("00:00:01", new TimeSpan(00, 00, 01));
        yield return new TestCaseData("00:01:00", new TimeSpan(00, 01, 00));
        yield return new TestCaseData("01:00:00", new TimeSpan(01, 00, 00));
        yield return new TestCaseData("23:59:59", new TimeSpan(23, 59, 59));

        yield return new TestCaseData("24:00:00", new TimeSpan(01, 00, 00, 00));
        yield return new TestCaseData("48:30:00", new TimeSpan(02, 00, 30, 00));
    }
}