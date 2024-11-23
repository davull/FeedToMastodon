using Microsoft.Extensions.Logging;

namespace FTM.Lib;

public class WorkerContext
{
    public required FeedConfiguration Configuration { get; init; } = null!;

    public required ILogger Logger { get; init; } = null!;

    public required HttpClient HttpClient { get; init; } = null!;

    public TimeSpan WaitDelay { get; private set; } = TimeSpan.FromMinutes(1);

    public void ResetDelay() => WaitDelay = TimeSpan.FromMinutes(1);

    public void SetDelay(TimeSpan delay) => WaitDelay = delay;
}