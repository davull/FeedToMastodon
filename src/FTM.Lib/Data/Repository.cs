using Dapper;

namespace FTM.Lib.Data;

public static class Repository
{
    public static async Task<List<FeedIdItemId>> GetProcessedItems(string feedId)
    {
        const string sql = """
                           SELECT FeedId, ItemId
                           from ProcessedPosts
                           WHERE FeedId = @FeedId;
                           """;

        using var con = Database.CreateConnection();

        var processedItems = await con.QueryAsync<FeedIdItemId>(
            sql, new { FeedId = feedId });
        return processedItems.ToList();
    }

    public static async Task SaveProcessedPost(string feedId, string itemId, string? feedTitle)
        => await SaveProcessedPost(feedId: feedId, itemId: itemId,
            statusId: null, timestamp: null, feedTitle: feedTitle);

    public static async Task SaveProcessedPost(string feedId, string itemId, string statusId, string? feedTitle)
        => await SaveProcessedPost(feedId: feedId, itemId: itemId,
            statusId: statusId, timestamp: null, feedTitle: feedTitle);

    public static async Task SaveProcessedPost(string feedId, string itemId,
        string? statusId, DateTimeOffset? timestamp, string? feedTitle)
    {
        const string sql = """
                           INSERT INTO ProcessedPosts (FeedId, ItemId, Timestamp, StatusId, FeedTitle)
                           VALUES (@FeedId, @ItemId, @Timestamp, @StatusId, @FeedTitle);
                           """;
        timestamp ??= DateTimeOffset.UtcNow;

        var seconds = timestamp.Value.ToUnixTimeSeconds();
        var record = new ProcessedPostRecord(feedId, itemId, seconds, statusId, feedTitle);

        using var con = Database.CreateConnection();
        await con.ExecuteAsync(sql, record);
    }

    public static async Task<bool> FeedExists(string feedId)
    {
        const string sql = """
                           SELECT count(*)
                           from ProcessedPosts
                           WHERE FeedId = @FeedId;
                           """;

        using var con = Database.CreateConnection();

        var count = await con.QueryFirstAsync<int>(sql, new { FeedId = feedId });
        return count > 0;
    }

    public static async Task<int> ProcessedPostsCountPerFeedInInterval(
        string feedId, DateTimeOffset start, DateTimeOffset end)
    {
        const string sql = """
                           SELECT count(*)
                           from ProcessedPosts
                           WHERE FeedId = @FeedId
                             AND Timestamp >= @Start AND Timestamp < @End
                             AND StatusId IS NOT NULL;
                           """;

        var startUnix = start.ToUnixTimeSeconds();
        var endUnix = end.ToUnixTimeSeconds();

        using var con = Database.CreateConnection();

        var count = await con.QueryFirstAsync<int>(sql,
            new { FeedId = feedId, Start = startUnix, End = endUnix });
        return count;
    }

    public static async Task<List<string>> GetFeedIds()
    {
        const string sql = """
                           SELECT DISTINCT FeedId
                           from ProcessedPosts;
                           """;

        using var con = Database.CreateConnection();

        var feedIds = await con.QueryAsync<string>(sql);
        return feedIds.ToList();
    }

    public static async Task<string?> GetFeedTitle(string feedId)
    {
        const string sql = """
                           SELECT FeedTitle
                           FROM ProcessedPosts 
                           WHERE FeedId = @FeedId
                           ORDER BY Timestamp DESC
                           LIMIT 1
                           """;

        using var con = Database.CreateConnection();

        var feedTitle = await con.QuerySingleOrDefaultAsync<string?>(sql, new { FeedId = feedId });
        return feedTitle;
    }
}