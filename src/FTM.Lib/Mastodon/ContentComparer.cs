namespace FTM.Lib.Mastodon;

public static class ContentComparer
{
    private static readonly string[] CharsToRemove =
        ["https", "http", "www", ".", " ", "…", "\n", "\r", "\t", ":", "/", "\\"];

    public static CompareResult Compare(string first, string second)
    {
        first = Sanitize(first);
        second = Sanitize(second);

        if (first.Contains(second))
        {
            return CompareResult.FirstContainsSecond;
        }

        if (second.Contains(first))
        {
            return CompareResult.SecondContainsFirst;
        }

        return CompareResult.Different;

        static string Sanitize(string s) => CharsToRemove.Aggregate(s, (current, c) => current.Replace(c, ""));
    }

    public enum CompareResult
    {
        FirstContainsSecond,
        SecondContainsFirst,
        Different
    }
}