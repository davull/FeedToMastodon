using FluentAssertions;
using NUnit.Framework;

namespace FTM.Lib.Tests;

public class FeedOnboardingTests
{
    [TestCase(true, false)]
    [TestCase(false, true)]
    public async Task IsNewFeed_ReturnsExpected(bool exists, bool expected)
    {
        var actual = await FeedOnboarding.IsNewFeed(Dummies.Feed(), FeedExists);

        actual.Should().Be(expected);

        Task<bool> FeedExists(string _) => Task.FromResult(exists);
    }

    [Test]
    public async Task OnboardNewFeed_Should_CallSavePostForEachItem()
    {
        var savedItems = new List<(string feedId, string itemId, string? feedTitle)>();

        var feedId = Dummies.GuidString();
        var feed = Dummies.Feed(feedId, items:
        [
            Dummies.FeedItem(feedId: feedId),
            Dummies.FeedItem(feedId: feedId)
        ]);

        await FeedOnboarding.OnboardNewFeed(feed, SavePost);

        var expected = feed.Items.Select(i => (feed.Id, i.ItemId, feed.Title));

        savedItems.Should().BeEquivalentTo(expected);

        Task SavePost(string f, string i, string? t)
        {
            savedItems.Add((f, i, t));
            return Task.CompletedTask;
        }
    }

    [Test]
    public async Task OnboardNewFeed_Should_HandleNonUniqueFeedIdItemId()
    {
        var savedItems = new List<(string feedId, string itemId, string? feedTitle)>();

        var feedId = Dummies.GuidString();
        var itemId = Dummies.GuidString();
        var feed = Dummies.Feed(items:
        [
            Dummies.FeedItem(feedId, itemId, "Item 1"),
            Dummies.FeedItem(feedId, itemId, "Item 2"),
            Dummies.FeedItem()
        ]);

        await FeedOnboarding.OnboardNewFeed(feed, SavePost);

        savedItems.Should().HaveCount(2);
        savedItems.Should().OnlyHaveUniqueItems();

        Task SavePost(string f, string i, string? t)
        {
            savedItems.Add((f, i, t));
            return Task.CompletedTask;
        }
    }
}