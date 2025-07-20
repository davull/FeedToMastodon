namespace FTM.Lib.Mastodon;

public static class StatusFormatter
{
    public static string GetStatus(string title, string summary, Uri? link)
    {
        // {0}: title
        // {1}: summary
        // {2}: link

        const string formatWithSummary = """
                                         {0}

                                         {1}
                                         ---
                                         {2}
                                         """;
        const string formatWoSummary = """
                                       {0}
                                       ---
                                       {2}
                                       """;
        const string formatWoTitle = """
                                     {1}
                                     ---
                                     {2}
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

        return string.Format(format, title, summary, link);
    }
}