using System.Diagnostics.CodeAnalysis;
using FTM.Lib;
using FTM.Lib.Data;
using FTM.Lib.Mastodon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FTM.App;

[ExcludeFromCodeCoverage]
public class Application(
    ApplicationOptions applicationOptions,
    ICollection<FeedConfiguration> feedConfigurations,
    IMastodonClient mastodonClient,
    ILoggerFactory loggerFactory)
{
    public static Application Create()
    {
        var feedConfigurations = FeedConfigurationReader.ReadConfiguration(Config.ConfigFileName);
        var appConfiguration = GetAppConfiguration();
        var appOptions = GetAppOptions();

        var loggerFactory = LoggerFactoryProvider.Create(appConfiguration);
        var mastodonClient = CreateMastodonClient(loggerFactory);

        return new Application(appOptions, feedConfigurations, mastodonClient, loggerFactory);

        IMastodonClient CreateMastodonClient(ILoggerFactory lf)
        {
            return Config.UseMastodonTestClient
                ? new MastodonTestClient()
                : new MastodonClient(lf.CreateLogger<MastodonClient>());
        }

        IConfiguration GetAppConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();
            return configuration;
        }

        ApplicationOptions GetAppOptions()
        {
            try
            {
                return appConfiguration.Get<ApplicationOptions>()!;
            }
            catch
            {
                return ApplicationOptions.Default;
            }
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
            var workerLoopDelay = config.WorkerLoopDelay ?? applicationOptions.DefaultWorkerLoopDelay;
            return new WorkerContext(workerLoopDelay)
            {
                Configuration = config,
                Logger = loggerFactory.CreateLogger(config.Title),
                HttpClient = HttpClientProvider.CreateHttpClient()
            };
        }

        Worker CreateWorker(WorkerContext context) => new(context, mastodonClient);
    }

    private Task InitializeStatisticsWorker(CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("Statistics");
        var worker = new StatisticsWorker(logger);
        return worker.Start(cancellationToken);
    }
}