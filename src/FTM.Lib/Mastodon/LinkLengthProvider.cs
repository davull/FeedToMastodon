namespace FTM.Lib.Mastodon;

public static class LinkLengthProvider
{
    // https://docs.joinmastodon.org/user/posting/#links
    // Links must start with http(s):// and are counted as 23 characters regardless of length.
    public static int GetRelevantLength(Uri? link) => link is null ? 0 : 23;
}