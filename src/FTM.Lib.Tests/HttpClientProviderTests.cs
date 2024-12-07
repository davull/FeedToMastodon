using System.Net;
using FluentAssertions;
using NUnit.Framework;
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

    [Test]
    public async Task WithStatusCodeOK_Should_ReturnSuccess()
    {
        SetupServer(200);

        var client = HttpClientProvider.CreateHttpClient();
        var uri = new Uri($"{_server.Url}/route");

        var response = await client.GetAsync(uri);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task WithOneFailingRequest_Should_RetryAndReturnSuccess()
    {
        SetupServer(429); // TooManyRequests

        var client = HttpClientProvider.CreateHttpClient(TimeSpan.FromMilliseconds(10));
        var uri = new Uri($"{_server.Url}/route");

        var response = await client.GetAsync(uri);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task WithTwoFailingRequest_Should_RetryAndReturnSuccess()
    {
        SetupServer(429, 429); // TooManyRequests

        var client = HttpClientProvider.CreateHttpClient(TimeSpan.FromMilliseconds(10));
        var uri = new Uri($"{_server.Url}/route");

        var response = await client.GetAsync(uri);
        var content = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task WithThreeFailingRequest_Should_RetryAndReturnSuccess()
    {
        SetupServer(429, 429, 429); // TooManyRequests

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
        SetupServer(429, 429, 429, 429); // TooManyRequests

        var client = HttpClientProvider.CreateHttpClient(TimeSpan.FromMilliseconds(10));
        var uri = new Uri($"{_server.Url}/route");

        var response = await client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    private void SetupServer(
        int statusCode0,
        int statusCode1 = 200,
        int statusCode2 = 200,
        int statusCode3 = 200)
    {
        const string scenario = "my-scenario";
        const string body = "lorem ipsum";
        const string path = "/route";

        var states = new[] { 0, 1, 2, 3 };
        var status = new[] { statusCode0, statusCode1, statusCode2, statusCode3 };

        // First call
        _server
            .Given(Request.Create()
                .WithPath(path)
                .UsingGet())
            .InScenario(scenario)
            .WillSetStateTo(states[0])
            .RespondWith(Response.Create()
                .WithStatusCode(status[0])
                .WithBody(body));

        // Second to n-th call
        for (var i = 1; i < 4; i++)
        {
            _server
                .Given(Request.Create()
                    .WithPath(path)
                    .UsingGet())
                .InScenario(scenario)
                .WhenStateIs(states[i - 1])
                .WillSetStateTo(states[i])
                .RespondWith(Response.Create()
                    .WithStatusCode(status[i])
                    .WithBody(body));
        }
    }
}