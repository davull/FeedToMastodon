using Microsoft.Extensions.Logging;

namespace FTM.Lib;

public class WorkerContext
{
    public required FeedConfiguration Configuration { get; init; } = null!;

    public required ILogger Logger { get; init; } = null!;

    public required HttpClient HttpClient { get; init; } = null!;

    public TimeSpan LoopWaitDelay { get; private set; } = Config.WorkerLoopDelay;

    public void ResetLoopDelay() => LoopWaitDelay = Config.WorkerLoopDelay;

    public void SetLoopDelay(TimeSpan delay) => LoopWaitDelay = delay;
}