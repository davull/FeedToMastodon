namespace FTM.Lib.Tests;

public class FeedItemPosterTests
{
    [Test]
    public void GetRelevantItems_WithValidNewItems_Should_ReturnItems()
    {
        FeedIdItemId[] processedItems =
        [
            Dummies.FeedIdItemId(),
            Dummies.FeedIdItemId()
        ];

        FeedItem[] items =
        [
            Dummies.FeedItem(),
            Dummies.FeedItem()
        ];

        var feed = Dummies.Feed(items: items);
        var actual = FeedItemPoster.GetRelevantItems(feed, processedItems)
            .OrderBy(item => item.ItemId);

        actual.ShouldBe(items.OrderBy(i => i.ItemId));
    }

    [Test]
    public void GetRelevantItems_WithInvalidItems_Should_ReturnNone()
    {
        FeedItem[] items =
        [
            Dummies.FeedItem(link: null),
            Dummies.FeedItem(title: "", summary: "", content: "")
        ];

        var feed = Dummies.Feed(items: items);
        var actual = FeedItemPoster.GetRelevantItems(feed, []);

        actual.ShouldBeEmpty();
    }

    [Test]
    public void GetRelevantItems_WithExistingItems_Should_ReturnNone()
    {
        FeedIdItemId[] processedItems =
        [
            Dummies.FeedIdItemId(),
            Dummies.FeedIdItemId()
        ];

        var items = processedItems
            .Select(x => Dummies.FeedItem(x.FeedId, x.ItemId))
            .ToArray();

        var feed = Dummies.Feed(items: items);
        var actual = FeedItemPoster.GetRelevantItems(feed, processedItems);

        actual.ShouldBeEmpty();
    }

    [Test]
    public async Task ProcessFeed_HappyPath()
    {
        var savePostFeedId = "";
        var savePostItemId = "";
        var savePostStatusId = "";
        var saveFeedTitle = "";

        const string postStatusId = "status-id";

        var item = Dummies.FeedItem("feed-id", "item-id");
        var feed = Dummies.Feed("feed-id", title: "Best feed ever", items: [item]);
        var context = Dummies.WorkerContext(Dummies.FeedConfiguration());

        await FeedItemPoster.ProcessFeed(feed, [], SavePost, PostStatus, context);

        savePostFeedId.ShouldBe(feed.Id);
        savePostItemId.ShouldBe(item.ItemId);
        savePostStatusId.ShouldBe(postStatusId);
        saveFeedTitle.ShouldBe("Best feed ever");

        Task SavePost(string feedId, string itemId, string statusId, string? feedTitle)
        {
            savePostFeedId = feedId;
            savePostItemId = itemId;
            savePostStatusId = statusId;
            saveFeedTitle = feedTitle;
            return Task.CompletedTask;
        }

        Task<string> PostStatus(MastodonStatus status, WorkerContext _, CancellationToken __)
            => Task.FromResult(postStatusId);
    }
}