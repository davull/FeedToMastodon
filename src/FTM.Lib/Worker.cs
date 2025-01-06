using System.Diagnostics.CodeAnalysis;
using System.Net;
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
        Logger.LogInformation("Worker started, loop delay: {LoopDelay}, feed: {FeedUri}",
            context.LoopDelay, Configuration.FeedUri);

        try
        {
            var startDelay = Config.WorkerStartDelay(context.LoopDelay);
            Logger.LogDebug("Delay worker start for {StartDelay}", startDelay.ToString(@"hh\:mm\:ss"));
            await Task.Delay(startDelay, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Loop(cancellationToken);
                    context.ResetLoopDelay();
                }
                catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.TooManyRequests)
                {
                    Logger.LogError(
                        "Error while processing feed {FeedUri}: Too Many Network Requests (HTTP Status {StatusCode}), resetting loop delay",
                        Configuration.FeedUri, (int)ex.StatusCode);
                    context.ResetLoopDelay();
                }
                catch (HttpRequestException ex)
                {
                    Logger.LogError(
                        "Error while processing feed {FeedUri}: Network error, status code {StatusCode} ({NumericStatusCode}), setting loop delay to {Delay}",
                        Configuration.FeedUri, ex.StatusCode, (int)(ex.StatusCode ?? 0),
                        Config.HttpRequestExceptionDelay);
                    context.SetLoopDelay(Config.HttpRequestExceptionDelay);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Logger.LogError(ex, "Error while processing feed {FeedUri}: {Message}",
                        Configuration.FeedUri, ex.Message);
                }

                await Task.Delay(context.LoopDelay, cancellationToken);
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
        var (feed, etag) = await FeedReader.ReadIfChanged(Configuration.FeedUri,
            context.HttpClient, context.ETag, cancellationToken);
        context.ETag = etag;

        if (feed is null)
        {
            Logger.LogDebug("Feed {Feed} has not changed due to HTTP ETag Header {ETag}",
                Configuration.Title, context.ETag);
            return;
        }

        var isNewFeed = await FeedOnboarding.IsNewFeed(feed, Repository.FeedExists);

        if (isNewFeed)
        {
            Logger.LogInformation("New feed detected: {Feed}, id {Id}, {Count} Items",
                feed.Title, feed.Id, feed.Items.Count);

            await FeedOnboarding.OnboardNewFeed(feed, Repository.SaveProcessedPost);
        }
        else
        {
            var itemIds = feed.Items.Select(i => i.ItemId).ToArray();
            var processedItems = await Repository.GetProcessedItems(feed.Id, itemIds);
            await FeedItemPoster.ProcessFeed(feed, processedItems,
                Repository.SaveProcessedPost, mastodonClient.PostStatus,
                context, cancellationToken);
        }
    }
}