namespace FTM.Lib.Mastodon;

public static class ContentComparer
{
    public static bool ContentEquals(string content1, string content2)
    {
        return string.Equals(content1, content2, StringComparison.OrdinalIgnoreCase);
    }
}