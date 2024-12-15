namespace FTM.Lib.Mastodon;

public static class ContentComparer
{
    public static CompareResult Compare(string content1, string content2)
    {
        var equal = string.Equals(content1, content2, StringComparison.OrdinalIgnoreCase);
        return equal ? CompareResult.Equal : CompareResult.NotEqual;
    }

    public enum CompareResult
    {
        Equal,
        NotEqual
    }
}