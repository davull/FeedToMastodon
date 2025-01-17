using System.Globalization;

namespace FTM.Lib.Tests;

public class StatisticsMessageProviderTests : TestBase
{
    [SetUp]
    public void SetUp()
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
    }

    [Test]
    public void PostsPerFeed_CreateMessage_EmptyList()
    {
        var postsPerHour = new List<PostsPerFeed>();

        var message = StatisticsMessageProvider.CreateMessage(postsPerHour);

        message.Should().BeEmpty();
    }

    [Test]
    public void PostsPerFeed_CreateMessage_EmptyStats()
    {
        // 2024-10-01 15:30
        var start = DateTimeOffset(10, 1, 15, 30);
        var end = start.AddDays(1);
        var postsPerFeed = new List<PostsPerFeed>
        {
            new("Feed number 1", 0, start, end),
            new("Feed number 2", 0, start, end),
            new("Feed number 3", 0, start, end)
        };
        var message = StatisticsMessageProvider.CreateMessage(postsPerFeed);
        message.Should().MatchSnapshot();
    }

    [Test]
    public void PostsPerFeed_CreateMessage_ReturnsExpected()
    {
        // 2024-10-01 15:30
        var start = DateTimeOffset(10, 1, 15, 30);
        var end = start.AddDays(1);
        var postsPerFeed = new List<PostsPerFeed>
        {
            new("Feed number 1", 0, start, end),
            new("Feed number 2", 100, start, end),
            new("Feed number 300", 2_345, start, end),
            new("Very long feed title with many chars - Very long feed title with many chars", 12, start, end),
            new("Very long feed title with many chars - Very long feed title with many chars", 9_999, start, end)
        };
        var message = StatisticsMessageProvider.CreateMessage(postsPerFeed);
        message.Should().MatchSnapshot();
    }

    private static DateTimeOffset DateTimeOffset(int month, int day,
        int hour = 0, int minute = 0, int second = 0)
        => new(2024, month, day, hour, minute, second, TimeSpan.Zero);
}