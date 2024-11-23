namespace FTM.Lib;

public static class FeedOnboarding
{
    public delegate Task<bool> FeedExists(string feedId);

    public delegate Task SavePost(string feedId, string itemId, string? feedTitle);

    public static async Task<bool> IsNewFeed(Feed feed, FeedExists feedExists)
    {
        var result = await feedExists(feed.Id);
        return result is false;
    }

    public static async Task OnboardNewFeed(Feed feed, SavePost savePost)
    {
        var itemIds = feed.Items
            .Select(i => i.ItemId)
            .Distinct();

        foreach (var itemId in itemIds)
        {
            await savePost(feedId: feed.Id, itemId: itemId, feedTitle: feed.Title);
        }
    }
}