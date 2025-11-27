namespace FTM.Lib.Mastodon;

public static class StatusFormatter
{
    public static string GetStatus(string title, string summary, string tags, Uri? link)
    {
        // TODO: Handle empty link
        
        // {0}: title
        // {1}: summary
        // {2}: tags
        // {3}: link

        var hasTags = !string.IsNullOrEmpty(tags);
        var hasTitle = !string.IsNullOrEmpty(title);
        var hasSummary = !string.IsNullOrEmpty(summary);

        var format = (hasTitle, hasSummary) switch
        {
            // No title and no summary
            (false, false) => GetFormatWoLinkAndSummary(hasTags),
            // Title only
            (true, false) => GetFormatWoSummary(hasTags),
            // Summary only
            (false, true) => GetFormatWoTitle(hasTags),
            // Title and summary
            (true, true) => GetFormatWithSummary(hasTags)
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