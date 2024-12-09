using Microsoft.Extensions.Logging;

namespace FTM.Lib;

public class WorkerContext(TimeSpan defaultLoopWaitDelay)
{
    private readonly TimeSpan _defaultLoopWaitDelay = defaultLoopWaitDelay;

    public required FeedConfiguration Configuration { get; init; } = null!;

    public required ILogger Logger { get; init; } = null!;

    public required HttpClient HttpClient { get; init; } = null!;

    public TimeSpan LoopWaitDelay { get; private set; } = defaultLoopWaitDelay;

    public void ResetLoopDelay() => LoopWaitDelay = _defaultLoopWaitDelay;

    public void SetLoopDelay(TimeSpan delay) => LoopWaitDelay = delay;
}