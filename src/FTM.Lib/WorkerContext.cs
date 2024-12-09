using Microsoft.Extensions.Logging;

namespace FTM.Lib;

public class WorkerContext(TimeSpan defaultLoopDelay)
{
    private readonly TimeSpan _defaultLoopDelay = defaultLoopDelay;

    public required FeedConfiguration Configuration { get; init; } = null!;

    public required ILogger Logger { get; init; } = null!;

    public required HttpClient HttpClient { get; init; } = null!;

    public TimeSpan LoopDelay { get; private set; } = defaultLoopDelay;

    public void ResetLoopDelay() => LoopDelay = _defaultLoopDelay;

    public void SetLoopDelay(TimeSpan delay) => LoopDelay = delay;
}