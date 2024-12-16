using NUnit.Framework;

namespace FTM.Lib.Tests.Feeds;

public class UpdateTestFeeds
{
    [Explicit]
    [TestCaseSource(nameof(Feeds))]
    public async Task UpdateFeeds(string name, string url)
    {
        var directory = Path.Combine(PathHelper.GetCurrentFileDirectory(), "TestFeeds");
        var filePath = Path.Combine(directory, $"{name}.xml");

        var content = await ReadFeed(url);
        await File.WriteAllTextAsync(filePath, content);

        Assert.Pass();
    }

    private static async Task<string> ReadFeed(string url)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private static IEnumerable<TestCaseData> Feeds()
    {
        var feeds = FeedTestsProvider.TestFeedUrls();
        foreach (var (name, url) in feeds)
        {
            yield return new TestCaseData(name, url)
                .SetName(name);
        }
    }
}