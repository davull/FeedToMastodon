namespace FTM.Lib.Mastodon;

public static class StatusFormatter
{
    public static string GetStatus(string title, string summary, string tags, Uri? link)
    {
        // {0}: title
        // {1}: summary
        // {2}: tags
        // {3}: link

        var hasTags = !string.IsNullOrEmpty(tags);
        var format = GetFormat(title, summary, hasTags);

        return string.Format(format, title, summary, tags, link);
    }

    private static string GetFormat(string title, string summary, bool hasTags)
    {
        if (hasTags)
        {
            return "";
        }
        else
        {
            return GetFormatWoTags(title, summary);
        }
    }

    private static string GetFormatWoTags(string title, string summary)
    {
        const string formatWithSummary = """
                                         {0}

                                         {1}
                                         ---
                                         {3}
                                         """;
        const string formatWoSummary = """
                                       {0}
                                       ---
                                       {3}
                                       """;
        const string formatWoTitle = """
                                     {1}
                                     ---
                                     {3}
                                     """;

        var compare = ContentComparer.Compare(title, summary);

        switch (compare)
        {
            // Title contains summary
            case ContentComparer.CompareResult.FirstContainsSecond:
                return formatWoSummary;
            // Summary contains title
            case ContentComparer.CompareResult.SecondContainsFirst:
                return formatWoTitle;
            case ContentComparer.CompareResult.Different:
                return formatWithSummary;
            default:
                throw new ArgumentOutOfRangeException(nameof(compare),
                    compare, "Unexpected comparison result");
        }
    }
}