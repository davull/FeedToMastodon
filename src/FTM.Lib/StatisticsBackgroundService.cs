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

        using var timer = new PeriodicTimer(TimeSpan.FromDays(1));

        try
        {
            do
            {
                try
                {
                    await Loop();
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Error while processing statistics: {Message}", ex.Message);
                }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
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