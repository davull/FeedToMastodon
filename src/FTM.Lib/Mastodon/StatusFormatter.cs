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

        var format = (hasTitle, hasSummary, hasTags) switch
        {
            // No title, no summary, no tags
            (false, false, false) => "{3}",
            // No title, no summary
            (false, false, true) => """
                                    {2}
                                    ---
                                    {3}
                                    """,
            // Title only
            (true, false, false) => """
                                    {0}
                                    ---
                                    {3}
                                    """,
            // Title and tags
            (true, false, true) => """
                                   {0}

                                   {2}
                                   ---
                                   {3}
                                   """,
            // Summary only
            (false, true, false) => """
                                    {1}
                                    ---
                                    {3}
                                    """,
            // Summary and tags
            (false, true, true) => """
                                   {1}

                                   {2}
                                   ---
                                   {3}
                                   """,
            // Title and summary
            (true, true, false) => """
                                   {0}

                                   {1}
                                   ---
                                   {3}
                                   """,
            // Title, summary, tags
            (true, true, true) => """
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