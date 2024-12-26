namespace FTM.Lib.Mastodon;

public record PostStatusResponse(
    string Id,
    string Language,
    string Uri,
    string Url,
    string Content);

public record RateLimit(
    int Limit,
    int Remaining,
    DateTime Reset);