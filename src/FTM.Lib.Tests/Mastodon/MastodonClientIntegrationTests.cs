using FluentAssertions;
using FTM.Lib.Mastodon;
using NUnit.Framework;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace FTM.Lib.Tests.Mastodon;

public class MastodonClientIntegrationTests : TestBase
{
    private WireMockServer _server = null!;
    private readonly MastodonClient _sut = new();

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
    public async Task PostStatus_WithValidData_Should_ReturnStatusId()
    {
        SetupServer(200, "PostStatusSuccessResponse.json");

        var statusId = await PostStatus();

        statusId.Should().Be("123456789012345678");
    }

    [Test]
    public async Task PostStatus_WithInvalidToken_Should_ThrowException()
    {
        SetupServer(401, "PostStatusInvalidTokenResponse.json");

        var action = async () => await PostStatus();

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*The access token is invalid*");
    }

    [Test]
    public async Task PostStatus_WithRateLimit_Should_ThrowException()
    {
        SetupServer(429, "PostStatusWithRateLimitResponse.json", 0);

        var action = async () => await PostStatus();

        var exception = (await action.Should().ThrowAsync<RateLimitException>())
            .Subject.Single();

        exception.Limit.Should().Be(300);
        exception.Remaining.Should().Be(0);
        exception.Reset.Should().BeCloseTo(
            new DateTime(2024, 11, 2, 1, 0, 0, DateTimeKind.Utc),
            TimeSpan.FromSeconds(1));
    }

    private void SetupServer(int statusCode, string responseFileName, int remaining = 299)
    {
        var responseFile = Path.Combine(PathHelper.GetCurrentFileDirectory(),
            "Responses", responseFileName);

        _server
            .Given(Request.Create()
                .WithPath("/api/v1/statuses")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(statusCode)
                .WithHeader("X-Ratelimit-Limit", "300")
                .WithHeader("X-Ratelimit-Remaining", $"{remaining}")
                .WithHeader("X-Ratelimit-Reset", "2024-11-02T00:00:00.581781Z")
                .WithBodyFromFile(responseFile));
    }

    private async Task<string> PostStatus()
    {
        var status = Dummies.MastodonStatus("This is a test status",
            "en-US", MastodonStatusVisibility.Public);

        var context = Dummies.WorkerContext(Dummies.FeedConfiguration(
            mastodonServer: _server.Url!,
            mastodonAccessToken: "some-mastodon-token"));

        return await _sut.PostStatus(status, context, default);
    }
}