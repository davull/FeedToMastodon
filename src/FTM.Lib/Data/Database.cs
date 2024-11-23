using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace FTM.Lib.Data;

public static class Database
{
    public static IDbConnection CreateConnection()
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Config.DatabaseName,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ConnectionString;

        var con = new SqliteConnection(connectionString);
        con.Open();
        return con;
    }

    public static void DeleteDatabase()
    {
        SqliteConnection.ClearAllPools();
        File.Delete(Config.DatabaseName);
    }

    public static void Initialize()
    {
        using var con = CreateConnection();
        Migrations.ApplyMigrations(con);
    }

    public static async Task<ICollection<string>> GetTables()
    {
        const string sql = "SELECT name FROM sqlite_master WHERE type='table';";

        using var con = CreateConnection();
        return (await con.QueryAsync<string>(sql)).ToList();
    }

    public static async Task<bool> TableExists(string tableName)
    {
        const string sql = """
                           SELECT COUNT(*) FROM sqlite_master
                           WHERE type='table'
                             AND name=@Name;
                           """;

        using var con = CreateConnection();
        var count = await con.QueryFirstAsync<int>(sql, new { Name = tableName });
        return count > 0;
    }

    public static async Task<ICollection<string>> GetColumns(string tableName)
    {
        var sql = $"PRAGMA table_info({tableName});";

        using var con = CreateConnection();
        return (await con.QueryAsync(sql))
            .Select(row => (string)row.name)
            .ToList();
    }
}