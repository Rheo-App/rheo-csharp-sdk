namespace Rheo.Sdk.Exceptions;

public sealed class RheoRateLimitException : RheoApiException
{
    public TimeSpan? RetryAfter { get; }

    public RheoRateLimitException(TimeSpan? retryAfter = null)
        : base("Rate limit exceeded", 429, "rate_limit_exceeded")
    {
        RetryAfter = retryAfter;
    }
}
