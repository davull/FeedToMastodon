namespace FTM.Lib.Mastodon;

public record PostStatusResponse(
    string Id,
    string Language,
    string Uri,
    string Url,
    string Content);