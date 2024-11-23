using System.Runtime.CompilerServices;

namespace FTM.Lib.Tests;

public static class PathHelper
{
    public static string GetCurrentFileDirectory([CallerFilePath] string path = "")
        => Path.GetDirectoryName(path)!;
}