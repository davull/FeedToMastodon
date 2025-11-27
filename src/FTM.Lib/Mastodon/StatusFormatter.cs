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
        var hasLink = link != null;

        var format = (hasTitle, hasSummary, hasTags, hasLink) switch
        {
            // No title, no summary, no tags, no link
            (false, false, false, false) => "",
            // No title, no summary, no tags
            (false, false, false, true) => "{3}",
            // No title, no summary, no link
            (false, false, true, false) => "{2}",
            // No title, no summary
            (false, false, true, true) => """
                                          {2}
                                          ---
                                          {3}
                                          """,
            // Title only
            (true, false, false, false) => "{0}",
            // Title and link only
            (true, false, false, true) => """
                                          {0}
                                          ---
                                          {3}
                                          """,
            // Title and tags
            (true, false, true, false) => """
                                          {0}

                                          {2}
                                          """,
            // Title, tags and link
            (true, false, true, true) => """
                                         {0}

                                         {2}
                                         ---
                                         {3}
                                         """,
            // Summary only
            (false, true, false, false) => "{1}",
            // Summary and link only
            (false, true, false, true) => """
                                          {1}
                                          ---
                                          {3}
                                          """,
            // Summary and tags
            (false, true, true, false) => """
                                          {1}

                                          {2}
                                          """,
            // Summary, tags and link
            (false, true, true, true) => """
                                         {1}

                                         {2}
                                         ---
                                         {3}
                                         """,
            // Title and summary
            (true, true, false, false) => """
                                          {0}

                                          {1}
                                          """,
            // Title, summary and link
            (true, true, false, true) => """
                                         {0}

                                         {1}
                                         ---
                                         {3}
                                         """,
            // Title, summary, tags
            (true, true, true, false) => """
                                         {0}

                                         {1}

                                         {2}
                                         """,
            // Title, summary, tags, link
            (true, true, true, true) => """
                                        {0}

                                        {1}

                                        {2}
                                        ---
                                        {3}
                                        """
        };

        return string.Format(format, title, summary, tags, link);
    }
}