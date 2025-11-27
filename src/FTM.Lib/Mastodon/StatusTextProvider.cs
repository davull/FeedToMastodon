namespace FTM.Lib.Mastodon;

public static class StatusTextProvider
{
    private const string Ellipsis = "...";

    private static readonly int TwoNewLinesLength = "\r\n\r\n".Length;

    public static string GetText(string title, string summary, string[] tags, Uri? link, int maxLength)
    {
        var hasTitle = !string.IsNullOrEmpty(title);
        var hasSummary = !string.IsNullOrEmpty(summary);
        var hasTags = tags.Length > 0;
        var hasContent = hasTitle || hasSummary || hasTags;
        var hasLink = link != null;

        var remainingLength = maxLength;
        remainingLength -= LinkLengthProvider.GetRelevantLength(link);

        if (hasContent && hasLink)
        {
            remainingLength -= "\r\n---\r\n".Length;
        }

        var tagsLine = GetTags(tags, remainingLength);
        remainingLength -= tagsLine.Length;

        var remainingLengthRequiredForTitle = TwoNewLinesLength + Ellipsis.Length + 4;
        if (hasTitle && remainingLength >= remainingLengthRequiredForTitle)
        {
            remainingLength -= TwoNewLinesLength;

            title = TrimIfNeeded(title, remainingLength);
            remainingLength -= title.Length;
        }
        else
        {
            title = string.Empty;
        }

        var remainingLengthRequiredForSummary = Ellipsis.Length + Ellipsis.Length + 4;
        if (hasSummary && remainingLength >= remainingLengthRequiredForSummary)
        {
            remainingLength -= TwoNewLinesLength;

            summary = TrimIfNeeded(summary, remainingLength);
        }
        else
        {
            summary = string.Empty;
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
        if (maxLength < Ellipsis.Length)
        {
            return string.Empty;
        }

        if (text.Length <= maxLength)
        {
            return text;
        }

        var l = maxLength - Ellipsis.Length;
        return text[..l] + Ellipsis;
    }
}