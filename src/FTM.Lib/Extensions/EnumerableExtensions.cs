namespace FTM.Lib.Extensions;

public static class EnumerableExtensions
{
    public static void Foreach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }
}