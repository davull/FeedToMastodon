namespace FTM.Lib.Mastodon;

public class RateLimitException : Exception
{
    public DateTime? Reset { get; }
    public int? Limit { get; }
    public int? Remaining { get; }

    public RateLimitException() : base("Rate limit exceeded")
    {
    }

    public RateLimitException(int limit, int remaining, DateTime reset)
        : base($"Rate limit exceeded; limit {limit}, remaining {remaining}, reset {reset}")
    {
        Limit = limit;
        Remaining = remaining;
        Reset = reset;
    }
}