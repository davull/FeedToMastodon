namespace FTM.Lib.Mastodon;

public static class StatusBuilder
{
    private const int ReservedLength = 11;
    private const int MaxStatusLength = 500 - ReservedLength;

    private const string DefaultLanguage = "en-US";

    public static MastodonStatus CreateStatus(FeedItem feedItem, string separator,
        MastodonStatusVisibility visibility = MastodonStatusVisibility.Public)
    {
        var status = BuildStatusText(feedItem, separator);
        var language = string.IsNullOrEmpty(feedItem.Language)
            ? DefaultLanguage
            : feedItem.Language;

        return new MastodonStatus(status, language, visibility);
    }

    private static string BuildStatusText(FeedItem item, string separator)
    {
        var remainingLength = MaxStatusLength;

        var link = item.Link;
        remainingLength -= LinkLengthProvider.GetRelevantLength(link);

        var title = GetTitle(item, remainingLength);
        remainingLength -= title.Length;

        var summary = GetSummary(item, remainingLength, separator);

        return GetStatus(title, summary, link);
    }

    private static string GetTitle(FeedItem item, int maxLength)
    {
        var title = StatusSanitizer.Sanitize(item.Title) ?? string.Empty;
        return TrimIfNeeded(title, maxLength);
    }

    private static string GetSummary(FeedItem item, int maxLength, string separator)
    {
        var summary = StatusSanitizer.Sanitize(item.Summary) ?? string.Empty;
        if (string.IsNullOrEmpty(summary))
        {
            summary = StatusSanitizer.Sanitize(item.Content) ?? string.Empty;
        }

        if (!string.IsNullOrEmpty(separator) &&
            summary.Contains(separator))
        {
            summary = summary.Split(separator)[0] + "...";
        }

        return TrimIfNeeded(summary, maxLength);
    }

    private static string GetStatus(string title, string summary, Uri? link)
    {
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

        var format = string.IsNullOrWhiteSpace(summary)
            ? formatWoSummary
            : formatWithSummary;

        return string.Format(format, title, summary, link);
    }

    private static string TrimIfNeeded(string text, int maxLength)
    {
        const string ellipsis = "...";

        if (maxLength < ellipsis.Length)
        {
            return string.Empty;
        }

        if (text.Length <= maxLength)
        {
            return text;
        }

        var l = maxLength - ellipsis.Length;
        return text[..l] + ellipsis;
    }
}