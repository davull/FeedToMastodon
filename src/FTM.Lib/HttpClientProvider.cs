using System.Net;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Retry;

namespace FTM.Lib;

public static class HttpClientProvider
{
    public static HttpClient CreateHttpClient(TimeSpan? delay = null)
    {
        delay ??= Config.HttpClientRetryDelay;

        var options = new HttpRetryStrategyOptions
        {
            BackoffType = DelayBackoffType.Exponential,
            Delay = delay.Value,
            MaxRetryAttempts = 3,
            UseJitter = true,
            ShouldHandle = ShouldHandle,
            ShouldRetryAfterHeader = false
        };

        var retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(options)
            .Build();

        var resilienceHandler = new ResilienceHandler(retryPipeline)
        {
            InnerHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2)
            }
        };
        return new HttpClient(resilienceHandler);
    }

    private static ValueTask<bool> ShouldHandle(RetryPredicateArguments<HttpResponseMessage> arg)
    {
        var code = arg.Outcome.Result?.StatusCode;
        if (code is null)
        {
            return ValueTask.FromResult(false);
        }

        var shouldHandle = code >= HttpStatusCode.InternalServerError;
        return ValueTask.FromResult(shouldHandle);
    }
}