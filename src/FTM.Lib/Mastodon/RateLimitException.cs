namespace FTM.Lib.Mastodon;

public class RateLimitException(int limit, int remaining, DateTime reset)
    : Exception($"Rate limit exceeded; limit {limit}, remaining {remaining}, reset {reset}")
{
    public DateTime Reset { get; } = reset;
    public int Limit { get; } = limit;
    public int Remaining { get; } = remaining;
}