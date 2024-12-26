namespace FTM.Lib.Mastodon;

public class RateLimitException : Exception
{
    public RateLimit? RateLimit { get; }

    public RateLimitException() : base("Rate limit exceeded")
    {
    }

    public RateLimitException(RateLimit rateLimit)
        : base($"Rate limit exceeded; limit {rateLimit.Limit}, " +
               $"remaining {rateLimit.Remaining}, " +
               $"reset {rateLimit.Reset}")
    {
        RateLimit = rateLimit;
    }
}