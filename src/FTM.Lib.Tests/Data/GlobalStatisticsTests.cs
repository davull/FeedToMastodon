using FluentAssertions;
using FluentAssertions.Execution;
using FTM.Lib.Data;
using NUnit.Framework;

namespace FTM.Lib.Tests.Data;

public class GlobalStatisticsTests : DatabaseTestBase
{
    [Test]
    public async Task PostsProcessedPerFeedLastDay_Should_ReturnExpected()
    {
        var feedId1 = Dummies.GuidString();
        var feedId2 = Dummies.GuidString();
        const string feedTitle1 = "Feed Title 1";
        const string feedTitle2 = "Feed Title 2";

        // Feed 1
        // 2024-10-01 11:XX
        await SaveTestPost(DateTimeOffset(10, 1, 11), Dummies.GuidString(), feedId1, feedTitle1);
        // 2024-10-02 11:XX
        await SaveTestPost(DateTimeOffset(10, 2, 11, 30), Dummies.GuidString(), feedId1, feedTitle1);
        await SaveTestPost(DateTimeOffset(10, 2, 11, 40), Dummies.GuidString(), feedId1, feedTitle1);
        // 2024-10-02 12:XX
        await SaveTestPost(DateTimeOffset(10, 2, 12, 15), Dummies.GuidString(), feedId1, feedTitle1);
        await SaveTestPost(DateTimeOffset(10, 2, 12, 25), Dummies.GuidString(), feedId1, feedTitle1);

        // Feed 2
        // 2024-10-02 11:XX
        await SaveTestPost(DateTimeOffset(10, 2, 11, 31), Dummies.GuidString(), feedId2, feedTitle2);
        await SaveTestPost(DateTimeOffset(10, 2, 11, 41), Dummies.GuidString(), feedId2, feedTitle2);
        // 2024-10-02 13:XX
        await SaveTestPost(DateTimeOffset(10, 2, 13), Dummies.GuidString(), feedId2, feedTitle2);

        // 2024-10-02 15:00
        var now = DateTimeOffset(10, 2, 15);
        var stats = await GlobalStatistics.PostsProcessedPerFeedLastDay(now);

        using var _ = new AssertionScope();

        stats.Should().HaveCount(2);

        stats.Should().ContainSingle(s => s.FeedTitle == feedTitle1 && s.Count == 4);
        stats.Should().ContainSingle(s => s.FeedTitle == feedTitle2 && s.Count == 3);
    }

    [Test]
    public async Task PostsProcessedPerFeedLastWeek_Should_ReturnExpected()
    {
        var feedId1 = Dummies.GuidString();
        var feedId2 = Dummies.GuidString();
        const string feedTitle1 = "Feed Title 1";
        const string feedTitle2 = "Feed Title 2";

        // Feed 1
        // 2024-09-01 11:XX
        await SaveTestPost(DateTimeOffset(09, 1, 11), Dummies.GuidString(), feedId1, feedTitle1);
        // 2024-09-26 17:XX
        await SaveTestPost(DateTimeOffset(09, 26, 17, 30), Dummies.GuidString(), feedId1, feedTitle1);
        // 2024-10-01 11:XX
        await SaveTestPost(DateTimeOffset(10, 02, 11, 40), Dummies.GuidString(), feedId1, feedTitle1);
        // 2024-10-02 12:XX
        await SaveTestPost(DateTimeOffset(10, 2, 12, 15), Dummies.GuidString(), feedId1, feedTitle1);
        await SaveTestPost(DateTimeOffset(10, 2, 12, 25), Dummies.GuidString(), feedId1, feedTitle1);

        // Feed 2
        // 2024-09-28 11:XX
        await SaveTestPost(DateTimeOffset(09, 28, 11, 31), Dummies.GuidString(), feedId2, feedTitle2);
        await SaveTestPost(DateTimeOffset(09, 28, 11, 41), Dummies.GuidString(), feedId2, feedTitle2);
        // 2024-10-02 13:XX
        await SaveTestPost(DateTimeOffset(10, 2, 13), Dummies.GuidString(), feedId2, feedTitle2);

        // 2024-10-02 15:00
        var now = DateTimeOffset(10, 2, 15);
        var stats = await GlobalStatistics.PostsProcessedPerFeedLastWeek(now);

        using var _ = new AssertionScope();

        stats.Should().HaveCount(2);

        stats.Should().ContainSingle(s => s.FeedTitle == feedTitle1 && s.Count == 4);
        stats.Should().ContainSingle(s => s.FeedTitle == feedTitle2 && s.Count == 3);
    }

    private static async Task SaveTestPost(DateTimeOffset timestamp, string? statusId = null,
        string? feedId = null, string? feedTitle = "Test feed")
    {
        feedId ??= Dummies.GuidString();
        var itemId = Dummies.GuidString();

        await Repository.SaveProcessedPost(feedId: feedId, itemId: itemId,
            statusId: statusId, feedTitle: feedTitle, timestamp: timestamp);
    }

    private static DateTimeOffset DateTimeOffset(int month, int day, int hour,
        int minute = 0, int second = 0)
        => new(2024, month, day, hour, minute, second, TimeSpan.Zero);
}