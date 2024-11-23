namespace FTM.Lib.Data;

public static class GlobalStatistics
{
    public static async Task<List<PostsPerFeed>> PostsProcessedPerFeedLastDay(DateTimeOffset? now = null)
    {
        now ??= DateTimeOffset.UtcNow;

        var end = now.Value;
        var start = end.AddDays(-1);
        return await PostsProcessedPerFeed(start, end);
    }

    public static async Task<List<PostsPerFeed>> PostsProcessedPerFeedLastWeek(DateTimeOffset? now = null)
    {
        now ??= DateTimeOffset.UtcNow;

        var end = now.Value.Date.AddDays(1);
        var start = end.AddDays(-7);
        return await PostsProcessedPerFeed(start, end);
    }

    private static async Task<List<PostsPerFeed>> PostsProcessedPerFeed(DateTimeOffset start, DateTimeOffset end)
    {
        var feedIds = await Repository.GetFeedIds();
        return await GetPostsPerFeed(feedIds, start, end);
    }

    private static async Task<List<PostsPerFeed>> GetPostsPerFeed(
        IEnumerable<string> feedIds, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        var result = new List<PostsPerFeed>();

        foreach (var feedId in feedIds)
        {
            var feedTitle = await Repository.GetFeedTitle(feedId) ?? feedId;
            var count = await Repository.ProcessedPostsCountPerFeedInInterval(feedId, startTime, endTime);

            if (count > 0)
            {
                result.Add(new PostsPerFeed(feedTitle, count, startTime, endTime));
            }
        }

        return result;
    }
}