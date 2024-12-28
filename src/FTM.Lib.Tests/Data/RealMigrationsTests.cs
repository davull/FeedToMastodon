using FluentAssertions;
using FTM.Lib.Data;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

namespace FTM.Lib.Tests.Data;

public class RealMigrationsTests : TestBase
{
    [TestCaseSource(nameof(GetMigrationsTestCases))]
    public async Task ApplyMigrations_WithExistingDatabase_ShouldMigrate(string filePath)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = filePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ConnectionString;

        await using (var con = new SqliteConnection(connectionString))
        {
            await con.OpenAsync();

            // ReSharper disable once AccessToDisposedClosure
            var action = () => Migrations.ApplyMigrations(con);
            action.Should().NotThrow();

            await con.CloseAsync();

            SqliteConnection.ClearPool(con);
        }

        File.Delete(filePath);
    }

    private static IEnumerable<TestCaseData> GetMigrationsTestCases()
    {
        var sourcePath = Path.Combine(PathHelper.GetCurrentFileDirectory(), "TestDatabases");
        var sourceFiles = Directory.GetFiles(sourcePath, "*.sqlite");

        foreach (var sourceFilePath in sourceFiles)
        {
            var destinationFilePath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".sqlite");
            File.Copy(sourceFilePath, destinationFilePath, true);

            yield return new TestCaseData(destinationFilePath)
            {
                TestName = Path.GetFileNameWithoutExtension(sourceFilePath)
            };
        }
    }
}