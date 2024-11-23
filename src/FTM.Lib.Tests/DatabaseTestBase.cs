using FTM.Lib.Data;
using NUnit.Framework;

namespace FTM.Lib.Tests;

[NonParallelizable]
public abstract class DatabaseTestBase : TestBase
{
    protected virtual bool InitializeDatabase => true;

    [SetUp]
    public virtual Task SetUp()
    {
        Directory.CreateDirectory("databases");

        var databaseName = $"databases/ftm-tests-{Guid.NewGuid()}.sqlite";
        var databaseFilePath = Path.GetFullPath(databaseName);
        Environment.SetEnvironmentVariable(Config.DatabaseNameKey, databaseFilePath);

        if (InitializeDatabase)
        {
            Database.Initialize();
        }

        return Task.CompletedTask;
    }

    [TearDown]
    public Task TearDown()
    {
        Database.DeleteDatabase();
        return Task.CompletedTask;
    }
}