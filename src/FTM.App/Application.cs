using System.Diagnostics.CodeAnalysis;
using FTM.Lib;
using FTM.Lib.Data;
using FTM.Lib.Mastodon;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Polly;

namespace FTM.App;

[ExcludeFromCodeCoverage]
public class Application(
    ICollection<FeedConfiguration> feedConfigurations,
    IMastodonClient mastodonClient,
    ILoggerFactory loggerFactory)
{
    public static Application Create()
    {
        var feedConfigurations = FeedConfigurationReader.ReadConfiguration(Config.ConfigFileName);
        var mastodonClient = CreateMastodonClient();
        var loggerFactory = CreateLoggerFactory();

        return new Application(feedConfigurations, mastodonClient, loggerFactory);

        ILoggerFactory CreateLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                builder
                    .AddSimpleConsole(options =>
                    {
                        options.SingleLine = true;
                        options.ColorBehavior = LoggerColorBehavior.Enabled;
                        options.TimestampFormat = "HH:mm:ss ";
                    })
                    .SetMinimumLevel(LogLevel.Information);
            });
        }

        IMastodonClient CreateMastodonClient()
        {
            return Config.UseMastodonTestClient
                ? new MastodonTestClient()
                : new MastodonClient();
        }
    }

    public async Task Run(CancellationToken cancellationToken)
    {
        Database.Initialize();

        Task[] workerTasks =
        [
            ..InitializeFeedWorkers(cancellationToken),
            InitializeStatisticsWorker(cancellationToken)
        ];
        await Task.WhenAll(workerTasks);
    }

    private List<Task> InitializeFeedWorkers(CancellationToken cancellationToken)
    {
        return feedConfigurations
            .Select(CreateWorkerContext)
            .Select(CreateWorker)
            .Select(w => w.Start(cancellationToken))
            .ToList();

        WorkerContext CreateWorkerContext(FeedConfiguration config)
        {
            return new WorkerContext
            {
                Configuration = config,
                Logger = loggerFactory.CreateLogger(config.Title),
                HttpClient = CreateHttpClient()
            };
        }

        Worker CreateWorker(WorkerContext context) => new(context, mastodonClient);

        HttpClient CreateHttpClient()
        {
            var retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(new HttpRetryStrategyOptions
                {
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromSeconds(4),
                    MaxRetryAttempts = 3
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

    private Task InitializeStatisticsWorker(CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("Statistics");
        var worker = new StatisticsWorker(logger);
        return worker.Start(cancellationToken);
    }
}