using System.Diagnostics.CodeAnalysis;
using FTM.Lib.Data;
using FTM.Lib.Feeds;
using FTM.Lib.Mastodon;
using Microsoft.Extensions.Logging;

namespace FTM.Lib;

[ExcludeFromCodeCoverage]
public class Worker(WorkerContext context, IMastodonClient mastodonClient)
{
    private ILogger Logger => context.Logger;
    private FeedConfiguration Configuration => context.Configuration;

    public async Task Start(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("Worker started, feed: {FeedUri}",
            Configuration.FeedUri);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Loop(cancellationToken);
                    context.ResetLoopDelay();
                }
                catch (HttpRequestException ex)
                {
                    Logger.LogError("Error while processing feed {FeedUri}: Network error, status code {StatusCode}",
                        Configuration.FeedUri, ex.StatusCode);
                    context.SetLoopDelay(Config.HttpRequestExceptionDelay);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Logger.LogError(ex, "Error while processing feed {FeedUri}: {Message}",
                        Configuration.FeedUri, ex.Message);
                }

                await Task.Delay(context.LoopWaitDelay, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("Worker stopped, feed: {FeedUri}",
                Configuration.FeedUri);
        }
    }

    private async Task Loop(CancellationToken cancellationToken)
    {
        var feed = await FeedReader.Read(Configuration.FeedUri, context.HttpClient);
        var isNewFeed = await FeedOnboarding.IsNewFeed(feed, Repository.FeedExists);

        if (isNewFeed)
        {
            Logger.LogInformation("New feed detected: {Feed}, id {Id}, {Count} Items",
                feed.Title, feed.Id, feed.Items.Count);

            await FeedOnboarding.OnboardNewFeed(feed, Repository.SaveProcessedPost);
        }
        else
        {
            var processedItems = await Repository.GetProcessedItems(feed.Id);
            await FeedItemPoster.ProcessFeed(feed, processedItems,
                Repository.SaveProcessedPost, mastodonClient.PostStatus,
                context, cancellationToken);
        }
    }
}