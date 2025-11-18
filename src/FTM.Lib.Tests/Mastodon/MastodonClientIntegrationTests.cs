using FTM.Lib.Mastodon;
using Microsoft.Extensions.Logging.Abstractions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace FTM.Lib.Tests.Mastodon;

public class MastodonClientIntegrationTests : TestBase
{
    private WireMockServer _server = null!;
    private readonly MastodonClient _sut = new(NullLogger<MastodonClient>.Instance);

    protected override async Task SetUp()
    {
        await base.SetUp();

        _server = WireMockServer.Start();
    }

    protected override async Task TearDown()
    {
        await base.TearDown();

        _server.Stop();
        _server.Dispose();
    }

    [Test]
    public async Task PostStatus_WithValidData_Should_ReturnStatusId()
    {
        SetupServer(200, "PostStatusSuccessResponse.json");

        var statusId = await PostStatus();

        statusId.ShouldBe("123456789012345678");
    }

    [Test]
    public async Task PostStatus_WithInvalidToken_Should_ThrowException()
    {
        SetupServer(401, "PostStatusInvalidTokenResponse.json");

        var action = async () => await PostStatus();

        (await action.ShouldThrowAsync<InvalidOperationException>())
            .Message.ShouldContain("The access token is invalid");
    }

    [Test]
    public async Task PostStatus_TooManyRequests_WithRateLimit_Should_ThrowException()
    {
        SetupServer(429, "PostStatusWithRateLimitResponse.json", 0);

        var action = async () => await PostStatus();

        var exception = await action.ShouldThrowAsync<RateLimitException>();

        var expected = Dummies.RateLimit();

        exception.RateLimit.ShouldBe(expected);
    }

    [Test]
    public async Task PostStatus_TooManyRequests_WoRateLimit_Should_ThrowException()
    {
        SetupServer(429, "PostStatusWithRateLimitResponse.json", 0, false);

        var action = async () => await PostStatus();

        var exception = await action.ShouldThrowAsync<RateLimitException>();

        exception.RateLimit.ShouldBeNull();
    }

    [Test]
    public async Task PostStatus_Should_MatchSnapshot()
    {
        SetupServer(200, "PostStatusSuccessResponse.json");

        _ = await PostStatus();

        var log = _server.LogEntries.Single();
        var snapshot = new
        {
            log.RequestMessage,
            log.ResponseMessage
        };
        snapshot.MatchSnapshot(o => o
            .ExcludeField("RequestMessage.Url")
            .ExcludeField("RequestMessage.AbsoluteUrl")
            .ExcludeField("RequestMessage.DateTime")
            .ExcludeField("RequestMessage.Headers.Host")
            .ExcludeField("RequestMessage.BodyData.Encoding")
            .ExcludeField("RequestMessage.Port")
            .ExcludeField("RequestMessage.Origin")
            .ExcludeField("ResponseMessage.BodyData.BodyAsFile"));
    }

    private void SetupServer(int statusCode, string responseFileName,
        int remaining = 299, bool rateLimitHeaders = true)
    {
        var responseFile = Path.Combine(PathHelper.GetCurrentFileDirectory(),
            "Responses", responseFileName);

        var request = Request.Create()
            .WithPath("/api/v1/statuses")
            .UsingPost();

        var response = Response.Create()
            .WithStatusCode(statusCode)
            .WithBodyFromFile(responseFile);
        if (rateLimitHeaders)
        {
            response
                .WithHeader("X-Ratelimit-Limit", "300")
                .WithHeader("X-Ratelimit-Remaining", $"{remaining}")
                .WithHeader("X-Ratelimit-Reset", "2024-11-02T00:00:00.00Z");
        }

        _server
            .Given(request)
            .RespondWith(response);
    }

    private async Task<string> PostStatus()
    {
        var status = Dummies.MastodonStatus("This is a test status",
            "en-US", MastodonStatusVisibility.Public);

        var context = Dummies.WorkerContext(Dummies.FeedConfiguration(
            mastodonServer: _server.Url!,
            mastodonAccessToken: "some-mastodon-token"));

        return await _sut.PostStatus(status, context, CancellationToken.None);
    }
}