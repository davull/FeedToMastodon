namespace FTM.Lib.Mastodon;

public static class StatusBuilder
{
    public static MastodonStatus CreateStatus(FeedItem feedItem, string[] tags, string[] separators,
        int maxStatusLength, string defaultLanguage)
    {
        var text = BuildStatusText(feedItem, tags, separators, maxStatusLength);
        var language = string.IsNullOrEmpty(feedItem.Language)
            ? defaultLanguage
            : feedItem.Language;

        return new MastodonStatus(text, language, MastodonStatusVisibility.Public);
    }

    private static string BuildStatusText(FeedItem item, string[] tags, string[] separators, int maxStatusLength)
    {
        var link = item.Link;
        var title = GetTitle(item);
        var summary = GetSummary(item, separators);

        var compare = ContentComparer.Compare(title, summary);
        if (compare == ContentComparer.CompareResult.SecondContainsFirst)
        {
            title = string.Empty;
        }
        else if (compare == ContentComparer.CompareResult.FirstContainsSecond)
        {
            summary = string.Empty;
        }

        return StatusTextProvider.GetText(title, summary, tags, link, maxStatusLength);
    }

    private static string GetTitle(FeedItem item)
        => StatusSanitizer.Sanitize(item.Title) ?? string.Empty;

    private static string GetSummary(FeedItem item, string[] separators)
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

        return summary;
    }
}