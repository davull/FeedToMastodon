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
        var hasTitle = !string.IsNullOrEmpty(title);
        var hasSummary = !string.IsNullOrEmpty(summary);

        var compare = ContentComparer.Compare(title, summary);

        var format = (hasTitle, hasSummary, compare) switch
        {
            // No title and no summary
            (false, false, _) => GetFormatWoLinkAndSummary(hasTags),
            // Title contains summary
            (_, _, ContentComparer.CompareResult.FirstContainsSecond) => GetFormatWoSummary(hasTags),
            // Summary contains title
            (_, _, ContentComparer.CompareResult.SecondContainsFirst) => GetFormatWoTitle(hasTags),
            // Title and summary are different
            (_, _, ContentComparer.CompareResult.Different) => GetFormatWithSummary(hasTags),
            _ => throw new ArgumentOutOfRangeException(nameof(compare), compare, "Unexpected comparison result")
        };

        return string.Format(format, title, summary, tags, link);
    }

    private static string GetFormatWithSummary(bool hasTags)
    {
        if (hasTags)
        {
            return """
                   {0}

                   {1}

                   {2}
                   ---
                   {3}
                   """;
        }

        return """
               {0}

               {1}
               ---
               {3}
               """;
    }

    private static string GetFormatWoSummary(bool hasTags)
    {
        if (hasTags)
        {
            return """
                   {0}

                   {2}
                   ---
                   {3}
                   """;
        }

        return """
               {0}
               ---
               {3}
               """;
    }

    private static string GetFormatWoTitle(bool hasTags)
    {
        if (hasTags)
        {
            return """
                   {1}

                   {2}
                   ---
                   {3}
                   """;
        }

        return """
               {1}
               ---
               {3}
               """;
    }

    private static string GetFormatWoLinkAndSummary(bool hasTags)
    {
        if (hasTags)
        {
            return """
                   {2}
                   ---
                   {3}
                   """;
        }

        return """
               {3}
               """;
    }
}