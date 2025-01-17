using System.Net;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace FTM.Lib.Tests;

public class HttpClientProviderTests : TestBase
{
    private WireMockServer _server = null!;

    [SetUp]
    public void Setup()
    {
        _server = WireMockServer.Start();
    }

    [TearDown]
    public void TearDown()
    {
        _server.Stop();
        _server.Dispose();
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public async Task WithUpToThreeFailingRequests_Should_RetryAndReturnSuccess(int failures)
    {
        // 500 Internal Server Error
        var failureCodes = Enumerable.Range(0, failures).Select(_ => 500);
        int[] status = [..failureCodes, 200];

        SetupServer(status);

        var client = HttpClientProvider.CreateHttpClient(TimeSpan.FromMilliseconds(10));
        var uri = new Uri($"{_server.Url}/route");

        var response = await client.GetAsync(uri);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task WithFourFailingRequest_Should_ReturnFailure()
    {
        SetupServer(500, 500, 500, 502);

        var client = HttpClientProvider.CreateHttpClient(TimeSpan.FromMilliseconds(10));
        var uri = new Uri($"{_server.Url}/route");

        var response = await client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.BadGateway);
    }

    [Test]
    public async Task Should_NotRetryOnTooManyRequests()
    {
        SetupServer(500, 429);

        var client = HttpClientProvider.CreateHttpClient(TimeSpan.FromMilliseconds(10));
        var uri = new Uri($"{_server.Url}/route");

        var response = await client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    private void SetupServer(params int[] status)
    {
        const string scenario = "my-scenario";
        const string body = "lorem ipsum";
        const string path = "/route";

        var states = new[] { 0, 1, 2, 3 };

        if (status.Length < 4)
        {
            var missing = Enumerable.Range(0, 4 - status.Length).Select(_ => 200);
            status = [..status, ..missing];
        }

        var request = Request.Create()
            .WithPath(path)
            .UsingGet();

        // First call
        _server
            .Given(request)
            .InScenario(scenario)
            .WillSetStateTo(states[0])
            .RespondWith(Response.Create()
                .WithStatusCode(status[0])
                .WithBody(body));

        // Second to n-th call
        for (var i = 1; i < 4; i++)
        {
            _server
                .Given(request)
                .InScenario(scenario)
                .WhenStateIs(states[i - 1])
                .WillSetStateTo(states[i])
                .RespondWith(Response.Create()
                    .WithStatusCode(status[i])
                    .WithBody(body));
        }
    }
}