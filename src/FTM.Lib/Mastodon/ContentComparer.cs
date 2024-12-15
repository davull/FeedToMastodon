namespace FTM.Lib.Mastodon;

public static class ContentComparer
{
    public static CompareResult Compare(string first, string second)
    {
        if (first.Contains(second))
        {
            return CompareResult.FirstContainsSecond;
        }

        if (second.Contains(first))
        {
            return CompareResult.SecondContainsFirst;
        }

        return CompareResult.Different;
    }

    public enum CompareResult
    {
        FirstContainsSecond,
        SecondContainsFirst,
        Different
    }
}