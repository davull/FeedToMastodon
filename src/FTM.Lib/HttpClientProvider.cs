using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace FTM.Lib;

public static class HttpClientProvider
{
    public static HttpClient CreateHttpClient(TimeSpan? delay = null)
    {
        delay ??= TimeSpan.FromSeconds(4);

        var retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new HttpRetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Exponential,
                Delay = delay.Value,
                MaxRetryAttempts = 3,
                UseJitter = true
            })
            .Build();

#pragma warning disable EXTEXP0001
        var resilienceHandler = new ResilienceHandler(retryPipeline)
#pragma warning restore EXTEXP0001
        {
            InnerHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2)
            }
        };
        return new HttpClient(resilienceHandler);
    }
}