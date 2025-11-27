namespace FTM.Lib.Mastodon;

public static class StatusTextProvider
{
    private const int MaxStatusLength = 500;

    public static string GetText(string title, string summary, string[] tags, Uri? link)
    {
        var hasTitle = !string.IsNullOrEmpty(title);
        var hasSummary = !string.IsNullOrEmpty(summary);
        var hasTags = tags.Length > 0;
        var hasContent = hasTitle || hasSummary || hasTags;

        var remainingLength = MaxStatusLength;
        remainingLength -= LinkLengthProvider.GetRelevantLength(link);

        var tagsLine = GetTags(tags, remainingLength);
        remainingLength -= tagsLine.Length;

        if (hasContent)
        {
            remainingLength -= "\r\n---\r\n".Length;
        }

        if (hasTitle && remainingLength > 4)
        {
            remainingLength -= "\r\n\r\n".Length;

            title = TrimIfNeeded(title, remainingLength);
            remainingLength -= title.Length;
        }

        if (hasSummary && remainingLength > 4)
        {
            remainingLength -= "\r\n\r\n".Length;

            summary = TrimIfNeeded(summary, remainingLength);
        }

        return StatusFormatter.GetStatus(title, summary, tagsLine, link);
    }

    internal static string GetTags(string[] tags, int maxLength)
    {
        var effectiveTags = GetEffectiveTags();
        return string.Join(" ", effectiveTags.Select(tag => $"#{tag}"));

        IEnumerable<string> GetEffectiveTags()
        {
            var currentLength = 0;

            for (var i = 0; i < tags.Length; i++)
            {
                var tag = tags[i];

                // +1 for '#' and +1 for space
                var tagLength = tag.Length + (i == 0 ? 1 : 2);
                currentLength += tagLength;
                if (currentLength > maxLength)
                {
                    break;
                }

                yield return tag;
            }
        }
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