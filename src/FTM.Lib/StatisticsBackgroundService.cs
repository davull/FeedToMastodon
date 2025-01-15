using System.Diagnostics.CodeAnalysis;
using FTM.Lib.Data;
using FTM.Lib.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FTM.Lib;

[ExcludeFromCodeCoverage]
public class StatisticsBackgroundService(ILogger<StatisticsBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("StatisticsWorker started");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Loop();
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Error while processing statistics: {Message}", ex.Message);
                }

                const int delay = 1_000 * 60 * 60 * 24; // 24 hour
                await Task.Delay(delay, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("StatisticsWorker stopped");
        }
    }

    private async Task Loop()
    {
        var lastDay = await GlobalStatistics.PostsProcessedPerFeedLastDay();
        var lastWeek = await GlobalStatistics.PostsProcessedPerFeedLastWeek();

        LogMessage(StatisticsMessageProvider.CreateMessage(lastDay));
        LogMessage(StatisticsMessageProvider.CreateMessage(lastWeek));
    }

    private void LogMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        message
            .Split(Environment.NewLine)
            .Foreach(l => logger.LogInformation(l));
    }
}