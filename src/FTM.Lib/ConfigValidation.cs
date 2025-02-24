namespace FTM.Lib;

public static class ConfigValidation
{
    public static void Validate()
    {
        EnsureIsNotDirectory(Config.DatabaseName, "DatabaseName has to be a file path, not a directory path.");
        EnsureIsNotDirectory(Config.ConfigFileName, "ConfigFileName has to be a file path, not a directory path.");
    }

    internal static void EnsureIsNotDirectory(string candidate, string message)
    {
        if (!Path.Exists(candidate))
        {
            return;
        }

        if (Directory.Exists(candidate))
        {
            throw new InvalidOperationException(message);
        }
    }
}