using FTM.Lib.Data;

namespace FTM.Lib.Tests;

[NonParallelizable]
public abstract class DatabaseTestBase : TestBase
{
    protected virtual bool InitializeDatabase => true;

    protected override async Task SetUp()
    {
        await base.SetUp();

        Directory.CreateDirectory("databases");

        var databaseName = $"databases/ftm-tests-{Guid.NewGuid()}.sqlite";
        var databaseFilePath = Path.GetFullPath(databaseName);
        Environment.SetEnvironmentVariable(Config.DatabaseNameKey, databaseFilePath);

        if (InitializeDatabase)
        {
            Database.Initialize();
        }
    }

    protected override async Task TearDown()
    {
        await base.TearDown();

        Database.DeleteDatabase();
    }
}