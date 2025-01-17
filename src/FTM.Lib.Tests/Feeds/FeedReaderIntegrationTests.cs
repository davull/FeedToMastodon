using System.Net;
using FTM.Lib.Feeds;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace FTM.Lib.Tests.Feeds;

public class FeedReaderIntegrationTests : TestBase
{
    private const string ETag = "\"abc\"";

    private WireMockServer _server = null!;

    [SetUp]
    public void Setup()
    {
        _server = WireMockServer.Start();
        SetupServer();
    }

    [TearDown]
    public void TearDown()
    {
        _server.Stop();
        _server.Dispose();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("invalid-etag")]
    [TestCase("\"valid-etag\"")]
    public async Task ReadIfChanged_WoMatchingEtag_ShouldReturnFeed(string? etag)
    {
        var uri = $"{_server.Url}/feed.xml";

        using var httpClient = new HttpClient();
        var result = await FeedReader.ReadIfChanged(new Uri(uri), httpClient, etag, CancellationToken.None);

        result.feed.ShouldNotBeNull();
        result.etag.ShouldBe(ETag);
    }

    [Test]
    public async Task ReadIfChanged_WithMatchingEtag_ShouldNotReturnFeed()
    {
        var uri = $"{_server.Url}/feed.xml";

        using var httpClient = new HttpClient();
        var result = await FeedReader.ReadIfChanged(new Uri(uri), httpClient, ETag, CancellationToken.None);

        result.feed.ShouldBeNull();
        result.etag.ShouldBe(ETag);
    }

    private void SetupServer()
    {
        var responseFile = Path.Combine(PathHelper.GetCurrentFileDirectory(),
            "Responses", "feed.xml");

        _server
            .Given(Request.Create()
                .WithPath("/feed.xml")
                .WithHeader("If-None-Match", ETag)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.NotModified) // 304
                .WithHeader("ETag", ETag));

        _server
            .Given(Request.Create()
                .WithPath("/feed.xml")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithSuccess()
                .WithHeader("ETag", ETag)
                .WithBodyFromFile(responseFile));
    }
}