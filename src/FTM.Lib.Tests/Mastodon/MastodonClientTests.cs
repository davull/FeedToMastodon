using FTM.Lib.Mastodon;
using FTM.Lib.Tests.Extensions;

namespace FTM.Lib.Tests.Mastodon;

public class MastodonClientTests : TestBase
{
    [TestCaseSource(nameof(StatusContentTestCases))]
    public async Task GetStatusContent_Should_MatchSnapshot(
        string status, string language, MastodonStatusVisibility? visibility)
    {
        var mastodonStatus = Dummies.MastodonStatus(status, language, visibility);
        var content = MastodonClient.GetStatusContent(mastodonStatus);
        var actual = await content.ReadAsStringAsync();

        actual.Should().MatchSnapshotWithTestName();
    }

    private static IEnumerable<TestCaseData> StatusContentTestCases()
    {
        yield return new TestCaseData("My Status", "en-US", MastodonStatusVisibility.Public)
            .SetName("Status with language and public visibility");

        yield return new TestCaseData("My Status", null, MastodonStatusVisibility.Private)
            .SetName("Status without language and private visibility");

        yield return new TestCaseData("My Status", "de-DE", MastodonStatusVisibility.Unlisted)
            .SetName("Status with language and unlisted visibility");

        yield return new TestCaseData("My Status", "en", MastodonStatusVisibility.Direct)
            .SetName("Status with language and direct visibility");

        yield return new TestCaseData("My Status", "en", null)
            .SetName("Status with language and no visibility");

        yield return new TestCaseData("My Status", null, null)
            .SetName("Status without language and visibility");

        yield return new TestCaseData("", null, null)
            .SetName("Empty status without language and visibility");
    }

    [Test]
    public void CreateIdempotencyKey_Should_ReturnString()
    {
        var status = Dummies.MastodonStatus();
        var actual = MastodonClient.CreateIdempotencyKey(status);

        actual.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void CreateIdempotencyKey_Should_ReturnSameKeyForSameStatus()
    {
        var status = Dummies.MastodonStatus();

        var key1 = MastodonClient.CreateIdempotencyKey(status);
        var key2 = MastodonClient.CreateIdempotencyKey(status);

        key1.Should().Be(key2);
    }

    [Test]
    public void CreateStatusRequest_Should_MatchSnapshot()
    {
        var request = MastodonClient.CreateStatusRequest(
            new Uri("https://mastodon.example.com/api/v1/statuses"),
            Dummies.MastodonStatus(),
            "access-token");

        request.Should().MatchSnapshot();
    }
}