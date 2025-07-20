namespace FTM.Lib.Mastodon;

public static class StatusBuilder
{
    private const int ReservedLength = 14;
    private const int MaxStatusLength = 500 - ReservedLength;

    private const string DefaultLanguage = "en-US";

    public static MastodonStatus CreateStatus(FeedItem feedItem, string[] tags, string[] separators,
        MastodonStatusVisibility visibility = MastodonStatusVisibility.Public)
    {
        var status = BuildStatusText(feedItem, tags, separators);
        var language = string.IsNullOrEmpty(feedItem.Language)
            ? DefaultLanguage
            : feedItem.Language;

        return new MastodonStatus(status, language, visibility);
    }

    private static string BuildStatusText(FeedItem item, string[] tags, string[] separators)
    {
        var remainingLength = MaxStatusLength;

        var link = item.Link;
        remainingLength -= LinkLengthProvider.GetRelevantLength(link);

        var tagsLine = GetTags(tags);
        remainingLength -= tagsLine.Length;

        var title = GetTitle(item, remainingLength);
        remainingLength -= title.Length;

        var summary = GetSummary(item, remainingLength, separators);

        return StatusFormatter.GetStatus(title, summary, tagsLine, link);
    }

    private static string GetTitle(FeedItem item, int maxLength)
    {
        var title = StatusSanitizer.Sanitize(item.Title) ?? string.Empty;
        return TrimIfNeeded(title, maxLength);
    }

    private static string GetSummary(FeedItem item, int maxLength, string[] separators)
    {
        var summary = StatusSanitizer.Sanitize(item.Summary) ?? string.Empty;
        if (string.IsNullOrEmpty(summary))
        {
            summary = StatusSanitizer.Sanitize(item.Content) ?? string.Empty;
        }

        var summaryContainsSeparator = separators.Any(sep => !string.IsNullOrEmpty(sep) &&
                                                             summary.Contains(sep));
        if (summaryContainsSeparator)
        {
            summary = summary.Split(separators, StringSplitOptions.None)[0] + "...";
        }

        return TrimIfNeeded(summary, maxLength);
    }

    private static string GetTags(string[] tags) => string.Join(" ", tags.Select(tag => $"#{tag}"));

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