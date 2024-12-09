namespace FTM.Lib;

public record ApplicationOptions(TimeSpan DefaultWorkerLoopDelay)
{
    public static ApplicationOptions Default { get; } = new(TimeSpan.FromMinutes(1));
}