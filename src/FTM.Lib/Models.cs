using System.Diagnostics;

namespace FTM.Lib;

public record Feed(
    string Id,
    Uri? Website,
    string? Title,
    DateTimeOffset? LastUpdatedTime,
    string? Description,
    string? Language,
    IReadOnlyCollection<FeedItem> Items);

public record FeedItem(
    string FeedId,
    string ItemId,
    string? Title,
    DateTimeOffset? PublishDate,
    string? Summary,
    string? Content,
    string? Language,
    Uri? Link);

// https://docs.joinmastodon.org/methods/statuses/
public record MastodonStatus(
    string Status,
    string? Language,
    MastodonStatusVisibility? Visibility);

public enum MastodonStatusVisibility
{
    Public,
    Unlisted,
    Private,
    Direct
}

public record FeedConfiguration(
    string Title,
    Uri FeedUri,
    string[] SummarySeparators,
    string MastodonServer,
    string MastodonAccessToken,
    TimeSpan? WorkerLoopDelay);

[DebuggerDisplay("{FeedTitle}: {Count}")]
public record PostsPerFeed(
    string FeedTitle,
    int Count,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);

public record FeedIdItemId(string FeedId, string ItemId);