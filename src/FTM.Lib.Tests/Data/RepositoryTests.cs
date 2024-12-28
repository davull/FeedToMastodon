using FluentAssertions;
using FTM.Lib.Data;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace FTM.Lib.Tests.Data;

public class RepositoryTests : DatabaseTestBase
{
    [Test]
    public async Task SaveProcessedPost_WithStatusId_ShouldNotThrow()
    {
        var action = () => SaveTestPost();
        await action.Should().NotThrowAsync();
    }

    [Test]
    public async Task SaveProcessedPost_WoStatusId_ShouldNotThrow()
    {
        var feedId = Dummies.GuidString();
        var itemId = Dummies.GuidString();

        var action = () => Repository.SaveProcessedPost(feedId: feedId, itemId: itemId, feedTitle: "Test Feed");

        await action.Should().NotThrowAsync();
    }

    [Test]
    public async Task SaveProcessedPost_WoStatusId_ShouldSetStatusIdToNull()
    {
        var feedId = Dummies.GuidString();

        // Post w/o statusId
        await Repository.SaveProcessedPost(feedId: feedId,
            itemId: Dummies.GuidString(), feedTitle: "Test Feed 1");
        // Post w/ statusId
        await Repository.SaveProcessedPost(feedId: feedId,
            itemId: Dummies.GuidString(), statusId: "status-id", feedTitle: "Test Feed 1");

        var actual = await Repository.ProcessedPostsCountPerFeedInInterval(
            feedId: feedId,
            start: DateTimeOffset.UtcNow.AddHours(-1),
            end: DateTimeOffset.UtcNow.AddHours(1));

        actual.Should().Be(1);
    }

    [Test]
    public async Task GetProcessedItems_WoExistingPost_ShouldReturnEmptyList()
    {
        await SaveTestPost();

        var actual = await Repository.GetProcessedItems(Dummies.GuidString(), [Dummies.GuidString()]);
        actual.Should().BeEmpty();
    }

    [Test]
    public async Task GetProcessedItems_WoEmptyList_ShouldReturnEmptyList()
    {
        await SaveTestPost();

        var actual = await Repository.GetProcessedItems(Dummies.GuidString(), []);
        actual.Should().BeEmpty();
    }

    [Test]
    public async Task GetProcessedItems_CanHandleLargeLists_ShouldReturnEmptyList()
    {
        await SaveTestPost();

        var itemIds = Enumerable.Range(0, 1_000)
            .Select(_ => Dummies.GuidString())
            .ToArray();

        var actual = await Repository.GetProcessedItems(Dummies.GuidString(), itemIds);
        actual.Should().BeEmpty();
    }

    [Test]
    public async Task GetProcessedItems_WithExistingPost_ShouldReturnItems()
    {
        var feedId = Dummies.GuidString();
        var itemId = Dummies.GuidString();
        await SaveTestPost(feedId, itemId);

        var actual = await Repository.GetProcessedItems(feedId, [itemId]);

        var expected = Dummies.FeedIdItemId(feedId, itemId);

        actual.Should().BeEquivalentTo([expected]);
    }

    [Test]
    public async Task GetProcessedItems_WithExistingPosts_FilterByItemIds()
    {
        var feedId = Dummies.GuidString();
        var itemId1 = Dummies.GuidString();
        var itemId2 = Dummies.GuidString();
        await SaveTestPost(feedId, itemId1);
        await SaveTestPost(feedId, itemId2);
        await SaveTestPost(feedId);
        await SaveTestPost(feedId);

        var actual = await Repository.GetProcessedItems(feedId, [itemId1, itemId2]);

        FeedIdItemId[] expected =
        [
            Dummies.FeedIdItemId(feedId, itemId1),
            Dummies.FeedIdItemId(feedId, itemId2)
        ];

        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public async Task DuplicatePrimaryKey_ShouldThrow()
    {
        var feedId = Dummies.GuidString();
        var itemId = Dummies.GuidString();

        var action = new Func<Task>(async ()
            => await SaveTestPost(feedId, itemId));

        // First call should not throw
        await action.Should().NotThrowAsync();
        // Second call should throw
        await action.Should().ThrowAsync<SqliteException>();
    }

    [Test]
    public async Task FeedExists_WoMatchingFeed_ShouldReturnFalse()
    {
        await SaveTestPost();
        await SaveTestPost();

        var feedId = Dummies.GuidString();
        var actual = await Repository.FeedExists(feedId);

        actual.Should().BeFalse();
    }

    [Test]
    public async Task FeedExists_WithMatchingFeed_ShouldReturnTrue()
    {
        var feedId = Dummies.GuidString();
        await SaveTestPost(feedId);
        await SaveTestPost(feedId);

        var actual = await Repository.FeedExists(feedId);

        actual.Should().BeTrue();
    }

    [Test]
    public async Task ProcessedPostsCountPerFeedInInterval_WithNoPosts_ShouldReturnZero()
    {
        var feedId = Dummies.GuidString();
        var start = DateTimeOffset.UtcNow.AddSeconds(-10);
        var end = DateTimeOffset.UtcNow.AddSeconds(10);
        var actual = await Repository.ProcessedPostsCountPerFeedInInterval(feedId, start, end);

        actual.Should().Be(0);
    }

    [Test]
    public async Task ProcessedPostsCountPerFeedInInterval_ShouldIgnorePostsWoStatusId()
    {
        var feedId = Dummies.GuidString();
        await SaveTestPost(feedId);
        await SaveTestPost(feedId);
        await SaveTestPostWoPostId(feedId);

        var start = DateTimeOffset.UtcNow.AddSeconds(-10);
        var end = DateTimeOffset.UtcNow.AddSeconds(10);
        var actual = await Repository.ProcessedPostsCountPerFeedInInterval(feedId, start, end);

        actual.Should().Be(2);
    }

    [Test]
    public async Task ProcessedPostsCountPerFeedInInterval_WithPostsInInterval_ShouldReturnCount()
    {
        var feedId = Dummies.GuidString();
        var start = DateTimeOffset.UtcNow;

        await SaveTestPost(feedId);
        await SaveTestPost(feedId);
        await Task.Delay(1_000);

        var end = DateTimeOffset.UtcNow;
        var actual = await Repository.ProcessedPostsCountPerFeedInInterval(feedId, start, end);

        actual.Should().Be(2);
    }

    [Test]
    public async Task ProcessedPostsCountPerFeedInInterval_ShouldOnlyCountPostsOnInterval()
    {
        var feedId = Dummies.GuidString();

        // Save posts before interval
        await SaveTestPost(feedId);
        await Task.Delay(1_000);

        var intervalStart = DateTimeOffset.UtcNow;

        // Save posts during interval
        await SaveTestPost(feedId);
        await SaveTestPost(feedId);
        await Task.Delay(1_000);

        var intervalEnd = DateTimeOffset.UtcNow;

        // Save posts after interval
        await SaveTestPost(feedId);

        var actual = await Repository.ProcessedPostsCountPerFeedInInterval(
            feedId, intervalStart, intervalEnd);
        actual.Should().Be(2);
    }

    [Test]
    public async Task ProcessedPostsCountPerFeedInInterval_ShouldOnlyCountPostsWithMatchingFeedId()
    {
        var feedId = Dummies.GuidString();
        await SaveTestPost(feedId);
        await SaveTestPost(feedId);
        await SaveTestPost();
        await SaveTestPost();

        var start = DateTimeOffset.UtcNow.AddSeconds(-10);
        var end = DateTimeOffset.UtcNow.AddSeconds(10);
        var actual = await Repository.ProcessedPostsCountPerFeedInInterval(feedId, start, end);

        actual.Should().Be(2);
    }

    [Test]
    public async Task GetFeedIds_WithNoPosts_ShouldReturnEmptyList()
    {
        var actual = await Repository.GetFeedIds();

        actual.Should().BeEmpty();
    }

    [Test]
    public async Task GetFeedIds_WithPosts_ShouldReturnFeedIds()
    {
        var feedId1 = Dummies.GuidString();
        var feedId2 = Dummies.GuidString();
        await SaveTestPost(feedId1);
        await SaveTestPost(feedId1);
        await SaveTestPost(feedId2);

        var actual = await Repository.GetFeedIds();

        actual.Should().BeEquivalentTo(feedId1, feedId2);
    }

    [Test]
    public async Task GetFeedTitle_WoFeeds_ShouldReturnNull()
    {
        var actual = await Repository.GetFeedTitle(Dummies.GuidString());
        actual.Should().BeNull();
    }

    [Test]
    public async Task GetFeedTitle_WoFeedTitle_ShouldReturnNull()
    {
        var feedId = Dummies.GuidString();
        await SaveTestPost(feedId: feedId, feedTitle: null);

        var actual = await Repository.GetFeedTitle(feedId);
        actual.Should().BeNull();
    }

    [Test]
    public async Task GetFeedTitle_WithFeedTitle_ShouldReturnTitle()
    {
        var feedId = Dummies.GuidString();
        const string feedTitle = "Best feed ever";
        await SaveTestPost(feedId: feedId, feedTitle: feedTitle);

        var actual = await Repository.GetFeedTitle(feedId);
        actual.Should().Be(feedTitle);
    }

    [Test]
    public async Task GetFeedTitle_WithMultipleFeedTitle_ShouldReturnLastTitle()
    {
        var feedId = Dummies.GuidString();

        var timestamp = DateTimeOffset.UtcNow.AddSeconds(-1);

        await SaveTestPost(feedId: feedId, feedTitle: "First feed title", timestamp: timestamp);
        await SaveTestPost(feedId: feedId, feedTitle: "Last feed title", timestamp: timestamp.AddSeconds(1));

        var actual = await Repository.GetFeedTitle(feedId);
        actual.Should().Be("Last feed title");
    }

    private static async Task SaveTestPost(
        string? feedId = null,
        string? itemId = null,
        string? feedTitle = "Test feed",
        DateTimeOffset? timestamp = null)
    {
        feedId ??= Dummies.GuidString();
        itemId ??= Dummies.GuidString();
        var statusId = Dummies.GuidString();

        await Repository.SaveProcessedPost(feedId: feedId, itemId: itemId,
            statusId: statusId, feedTitle: feedTitle, timestamp: timestamp);
    }

    private static async Task SaveTestPostWoPostId(string? feedId = null)
    {
        feedId ??= Dummies.GuidString();
        var itemId = Dummies.GuidString();
        await Repository.SaveProcessedPost(feedId: feedId, itemId: itemId, feedTitle: "Test feed");
    }
}