using Snapshooter;

namespace FTM.Lib.Tests.Extensions;

internal static class SnapshooterExtensions
{
    public static void MatchSnapshotWithTestName(this object currentResult,
        Func<MatchOptions, MatchOptions>? matchOptions = null)
    {
        var test = TestContext.CurrentContext.Test;
        var name = SanitizeFileName($"{test.DisplayName}_{test.MethodName}_{test.Name}");

        currentResult.MatchSnapshot(name, matchOptions!);
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars()
            .Concat("|\"'=.&,");

        return invalid.Aggregate(name, (current, c) => current.Replace(c, '-'));
    }
}