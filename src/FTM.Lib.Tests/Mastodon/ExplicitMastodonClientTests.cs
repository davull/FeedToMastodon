using FluentAssertions;
using FTM.Lib.Mastodon;
using NUnit.Framework;

namespace FTM.Lib.Tests.Mastodon;

public class ExplicitMastodonClientTests : TestBase
{
    private const string MastodonServer = "https://feedmirror.social";
    private const string MastodonAccessToken = "***";

    private static readonly WorkerContext WorkerContext = Dummies.WorkerContext(
        Dummies.FeedConfiguration(mastodonServer: MastodonServer,
            mastodonAccessToken: MastodonAccessToken));

    private readonly MastodonClient _client = new();

    [Explicit]
    [Test]
    public async Task PostStatus()
    {
        var content = $"test post - lorem ipsum - {Guid.NewGuid()}";
        var status = new MastodonStatus(content, "en",
            MastodonStatusVisibility.Private);

        var postId = await _client.PostStatus(status, WorkerContext, default);
        postId.Should().NotBeNullOrEmpty();
    }
}