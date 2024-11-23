namespace FTM.Lib.Mastodon;

public class MastodonTestClient : IMastodonClient
{
    public Task<string> PostStatus(MastodonStatus status, WorkerContext context,
        CancellationToken cancellationToken) =>
        Task.FromResult($"tests-id-{Guid.NewGuid()}");
}