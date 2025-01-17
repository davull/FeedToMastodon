using FTM.Lib.Data;

namespace FTM.Lib.Tests.Data;

public class MigrationsTests : DatabaseTestBase
{
    protected override bool InitializeDatabase => false;

    [Test]
    public void GetScripts_Should_MatchSnapshot()
    {
        var scripts = Migrations.GetScripts();

        var snapshot = scripts
            .Select(s => new { s.Name, s.Contents })
            .ToList();

        snapshot.MatchSnapshot();
    }

    [Test]
    public void ApplyMigrations_WoDatabase_ShouldNotThrow()
    {
        using var connection = Database.CreateConnection();

        // ReSharper disable once AccessToDisposedClosure
        var action = () => Migrations.ApplyMigrations(connection);
        action.ShouldNotThrow();
    }

    [Test]
    public async Task ApplyMigrations_WoDatabase_Should_CreateSchemaTable()
    {
        var schemaTableExistsBefore = await Database.TableExists("SchemaVersions");
        schemaTableExistsBefore.ShouldBeFalse();

        using var connection = Database.CreateConnection();
        Migrations.ApplyMigrations(connection);

        var schemaTableExistsAfter = await Database.TableExists("SchemaVersions");
        schemaTableExistsAfter.ShouldBeTrue();
    }

    [Test]
    public async Task ApplyMigrations_Tables_ShouldMatchSnapshot()
    {
        using var connection = Database.CreateConnection();

        Migrations.ApplyMigrations(connection);

        var tables = (await Database.GetTables()).Order();
        tables.MatchSnapshot();
    }
}