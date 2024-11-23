using System.Text;
using FluentAssertions;
using FTM.Lib.Data;
using NUnit.Framework;
using Snapshooter.NUnit;

namespace FTM.Lib.Tests.Data;

public class DatabaseTests : DatabaseTestBase
{
    [Test]
    public async Task AfterInitialize_GetTables_ShouldMatchSnapshot()
    {
        var tables = await Database.GetTables();
        tables.Should().MatchSnapshot();
    }

    [Test]
    public async Task AfterInitialize_GetColumns_ShouldMatchSnapshot()
    {
        var snapshot = new StringBuilder();

        var tables = await Database.GetTables();

        foreach (var table in tables)
        {
            var fields = await Database.GetColumns(table);

            snapshot.AppendLine(table);
            foreach (var field in fields)
            {
                snapshot.AppendLine($"  {field}");
            }
        }

        snapshot.ToString().Should().MatchSnapshot();
    }
}