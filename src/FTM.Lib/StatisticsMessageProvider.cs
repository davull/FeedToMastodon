namespace FTM.Lib;

public static class StatisticsMessageProvider
{
    private const int Width = 60;

    public static string CreateMessage(List<PostsPerFeed> postsPerFeeds)
    {
        if (postsPerFeeds.Count == 0)
        {
            return string.Empty;
        }

        var start = postsPerFeeds.Min(p => p.StartTime);
        var end = postsPerFeeds.Max(p => p.EndTime);

        var content = CreateContent(postsPerFeeds);

        var message = $"""
                       {Line()}
                       Total Posts per feed:    {start:yyyy-MM-dd HH:mm} - {end:yyyy-MM-dd HH:mm}
                       {Line()}
                       {content}
                       {Line()}
                       """;
        return message;
    }

    private static string CreateContent(List<PostsPerFeed> postsPerFeeds)
    {
        var rows = string.Join(Environment.NewLine, postsPerFeeds.Select(FormatRow));
        var summary = CreateSummary();

        return $"""
                {rows}
                {Line('-')}
                {summary}
                """;

        string FormatRow(PostsPerFeed posts)
        {
            const int max = Width - 9;

            var feedTitle = posts.FeedTitle;
            if (feedTitle.Length > max)
            {
                feedTitle = feedTitle[..max];
            }

            feedTitle += ":";

            return $"  {feedTitle,-(Width - 5 - 3)} {posts.Count,5:N0}";
        }

        string CreateSummary()
        {
            var total = postsPerFeeds.Sum(f => f.Count);
            return $"Total: {total,Width - 7:N0}";
        }
    }

    private static string Line(char c = '=') => new(c, Width);
}