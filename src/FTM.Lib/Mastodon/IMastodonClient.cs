namespace FTM.Lib.Mastodon;

public interface IMastodonClient
{
    Task<string> PostStatus(MastodonStatus status, WorkerContext context,
        CancellationToken cancellationToken);
}