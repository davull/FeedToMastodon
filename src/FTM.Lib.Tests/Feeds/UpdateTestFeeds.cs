using System.Text;
using FTM.Lib.Tests.Extensions;

namespace FTM.Lib.Tests.Feeds;

public class UpdateTestFeeds : TestBase
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

    [Explicit]
    [TestCaseSource(nameof(Feeds))]
    public async Task UpdateFeedHeaders(string name, string url)
    {
        using var message = await ReadHttpMessage(url);

        var relevantHeaders = message.Headers
            .Where(Filter)
            .OrderBy(h => h.Key);

        var snapshot = CreateSnapshot(relevantHeaders);
        snapshot.MatchSnapshotWithTestName();

        bool Filter(KeyValuePair<string, IEnumerable<string>> header)
        {
            string[] ignoredHeaders =
            [
                "Age", "Date", "x-cloud-trace-context", "Via", "X-Amz-Cf-Id", "Report-To", "Server-Timing",
                "reporting-endpoints", "Content-Security-Policy", "CF-RAY", "permissions-policy", "Set-Cookie",
                "X-XSS-Protection", "x-nf-request-id", "Content-Security-Policy-Report-Only",
                "cross-origin-embedder-policy-report-only", "cross-origin-opener-policy-report-only",
                "cross-origin-resource-policy", "X-Fastly-Request-ID", "X-Served-By", "X-Timer", "X-Varnish",
                "x-svr-id", "x-rq", "x-ftr-backend-server", "x-ftr-request-id", "xkey", "X-GitHub-Request-Id",
                "x-generated-by", "x-ftr-balancer", "x-total-count", "x-ftr-cache-status", "X-Vercel-Id"
            ];
            return !ignoredHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase);
        }

        string CreateSnapshot(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            var sb = new StringBuilder();
            foreach (var (key, value) in headers)
            {
                sb.AppendLine($"{key,-30}: {string.Join(", ", value)}");
            }

            return sb.ToString();
        }
    }

    private static async Task<string> ReadFeed(string url)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private static async Task<HttpResponseMessage> ReadHttpMessage(string url)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return response;
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