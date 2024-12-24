using System.Data;
using System.Reflection;
using DbUp.Engine;
using DbUp.Sqlite.Helpers;

namespace FTM.Lib.Data;

public static class Migrations
{
    public static void ApplyMigrations(IDbConnection connection)
    {
        var upgrader = DbUp.DeployChanges.To
            .SqliteDatabase(new SharedConnection(connection))
            .WithScripts(GetScripts())
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw new Exception(
                $"Error while applying migrations. " +
                $"Script: {result.ErrorScript.Name}, " +
                $"Error: {result.Error.Message}");
        }
    }

    internal static IEnumerable<SqlScript> GetScripts()
    {
        const string prefix = "FTM.Lib.Data.Scripts.";

        var assembly = Assembly.GetExecutingAssembly();
        var resources = assembly.GetManifestResourceNames()
            .Where(r => r.StartsWith(prefix));

        foreach (var resource in resources)
        {
            var name = resource[prefix.Length..];

            using var stream = assembly.GetManifestResourceStream(resource);
            yield return SqlScript.FromStream(name, stream);
        }
    }
}