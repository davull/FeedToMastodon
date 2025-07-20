using FTM.Lib.Mastodon;
using Microsoft.Extensions.Logging;

namespace FTM.Lib;

public static class FeedItemPoster
{
    public delegate Task SavePost(string feedId, string itemId, string statusId, string? feedTitle);

    public delegate Task<string> PostStatus(MastodonStatus status,
        WorkerContext context, CancellationToken cancellationToken);

    public static async Task ProcessFeed(Feed feed, IEnumerable<FeedIdItemId> processedItems,
        SavePost savePost, PostStatus postStatus, WorkerContext context,
        CancellationToken cancellationToken = default)
    {
        var itemsToPost = GetRelevantItems(feed, processedItems);

        context.Logger.LogDebug(
            "Processing feed \"{Title}\", Id {Id}, Found {RelevantCount} new items, Total {TotalCount}",
            feed.Title, feed.Id, itemsToPost.Count, feed.Items.Count);

        var statuses = CreateStatuses(itemsToPost, context);

        foreach (var (item, status) in statuses)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ProcessStatus(item, feed.Title, status, savePost, postStatus, context, cancellationToken);
        }
    }

    private static IEnumerable<(FeedItem, MastodonStatus)> CreateStatuses(
        List<FeedItem> itemsToPost, WorkerContext context)
    {
        return itemsToPost.Select(item =>
        {
            var status = StatusBuilder.CreateStatus(item, [], context.Configuration.SummarySeparators);
            return (item, status);
        });
    }

    private static async Task ProcessStatus(FeedItem item, string? feedTitle, MastodonStatus status,
        SavePost savePost, PostStatus postStatus, WorkerContext context,
        CancellationToken cancellationToken)
    {
        context.Logger.LogInformation("  Processing item \"{Title}\", Id {ItemId}",
            item.Title, item.ItemId);

        try
        {
            var statusId = await postStatus(status, context, cancellationToken);
            await savePost(feedId: item.FeedId, itemId: item.ItemId,
                statusId: statusId, feedTitle: feedTitle);
        }
        catch (RateLimitException ex)
        {
            if (ex.RateLimit is null)
            {
                context.Logger.LogError(ex, "Rate limit exceeded");
            }
            else
            {
                context.Logger.LogError(ex,
                    "Rate limit exceeded; limit {Limit}, remaining {Remaining}, reset {Reset}",
                    ex.RateLimit.Limit, ex.RateLimit.Remaining, ex.RateLimit.Reset);
            }

            context.SetLoopDelay(Config.MastodonRateLimitExceptionDelay);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            context.Logger.LogError(ex, "Error while processing item {ItemId}: {Message}",
                item.ItemId, ex.Message);
        }
    }

    internal static List<FeedItem> GetRelevantItems(Feed feed, IEnumerable<FeedIdItemId> processedItems)
    {
        return feed.Items
            .Reverse()
            .Where(item =>
                ItemHasRequitedContent(item) &&
                ItemAlreadyProcessed(item) is false)
            .ToList();

        bool ItemHasRequitedContent(FeedItem item)
        {
            if (item.Link is null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(item.Title) &&
                string.IsNullOrEmpty(item.Summary) &&
                string.IsNullOrEmpty(item.Content))
            {
                return false;
            }

            return true;
        }

        bool ItemAlreadyProcessed(FeedItem item)
        {
            return processedItems.Any(x =>
                x.FeedId == item.FeedId &&
                x.ItemId == item.ItemId);
        }
    }
}