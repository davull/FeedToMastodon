using FTM.Lib.Feeds;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace FTM.Lib.Tests.Feeds;

public class FeedHttpClientIntegrationTests : TestBase
{
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

    [Test]
    public async Task ReadString_Timeout_ShouldThrow_TimeoutException_As_InnerException()
    {
        var uri = new Uri($"{_server.Url}/feed.xml");

        var client = _server.CreateClient();
        client.Timeout = TimeSpan.FromMilliseconds(200);

        try
        {
            _ = await FeedHttpClient.ReadString(uri, client, null, CancellationToken.None);
            Assert.Fail();
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            Assert.Pass();
        }
        catch
        {
            Assert.Fail();
        }
    }

    [Test]
    public async Task ReadString_Cancelled_ShouldThrow_TaskCanceledException_As_InnerException()
    {
        var uri = new Uri($"{_server.Url}/feed.xml");

        using var cts = new CancellationTokenSource(200);
        using var client = _server.CreateClient();

        try
        {
            _ = await FeedHttpClient.ReadString(uri, client, null, cts.Token);
            Assert.Fail();
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TaskCanceledException)
        {
            Assert.Pass();
        }
        catch
        {
            Assert.Fail();
        }
    }

    private void SetupServer()
    {
        var delay = TimeSpan.FromSeconds(1);

        _server
            .Given(Request.Create()
                .WithPath("/feed.xml")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("lorem ipsum")
                .WithDelay(delay));
    }
}