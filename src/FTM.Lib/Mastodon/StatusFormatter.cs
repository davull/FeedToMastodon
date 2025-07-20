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

        var format = compare switch
        {
            // Title contains summary
            ContentComparer.CompareResult.FirstContainsSecond => formatWoSummary,
            // Summary contains title
            ContentComparer.CompareResult.SecondContainsFirst => formatWoTitle,
            _ => formatWithSummary
        };

        return format;
    }
}