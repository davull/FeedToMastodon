using System.Linq.Expressions;

namespace FTM.Lib.Tests.Extensions;

public static class ShouldlyExtensions
{
    public static void ShouldHaveCount<T>(this IEnumerable<T> actual, int expectedCount)
    {
        actual.Count().ShouldBe(expectedCount);
    }

    public static void ShouldNotBeNullOrEmpty<T>(this IEnumerable<T> actual)
    {
        actual.ShouldSatisfyAllConditions(
            () => actual.ShouldNotBeNull(),
            () => actual.ShouldNotBeEmpty());
    }

    public static void ShouldContainSingle<T>(this IEnumerable<T> actual)
    {
        actual.ShouldHaveCount(1);
    }

    public static void ShouldContainSingle<T>(this IEnumerable<T> actual,
        Expression<Func<T, bool>> elementPredicate, string? customMessage = null)
    {
        var condition = elementPredicate.Compile();

        actual
            .Where(condition)
            .ShouldHaveCount(1);
    }
}