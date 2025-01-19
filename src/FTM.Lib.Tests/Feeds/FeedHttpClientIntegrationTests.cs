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
    public async Task ReadString_Timeout()
    {
        var uri = new Uri($"{_server.Url}/feed.xml");

        var client = _server.CreateClient();
        client.Timeout = TimeSpan.FromMilliseconds(200);

        try
        {
            _ = await FeedHttpClient.ReadString(uri, client, null, CancellationToken.None);
            Assert.Fail();
        }
        catch (Exception ex)
        {
            ex.ShouldBeOfType<TaskCanceledException>();

            ex.InnerException.ShouldNotBeNull();
            ex.InnerException.ShouldBeOfType<TimeoutException>();
        }
    }

    [Test]
    public async Task ReadString_Cancelled()
    {
        var uri = new Uri($"{_server.Url}/feed.xml");

        using var cts = new CancellationTokenSource(200);
        using var client = _server.CreateClient();

        try
        {
            _ = await FeedHttpClient.ReadString(uri, client, null, cts.Token);
            Assert.Fail();
        }
        catch (Exception ex)
        {
            ex.ShouldBeOfType<TaskCanceledException>();

            ex.InnerException.ShouldNotBeNull();
            ex.InnerException.ShouldBeOfType<TaskCanceledException>();
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