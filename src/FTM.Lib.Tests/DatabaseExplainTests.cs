using Dapper;
using FluentAssertions;
using FTM.Lib.Data;
using FTM.Lib.Tests.Extensions;
using NUnit.Framework;

namespace FTM.Lib.Tests;

public class DatabaseExplainTests : DatabaseTestBase
{
    [TestCaseSource(nameof(ExplainTestCases))]
    public async Task ExplainQuery_ShouldMatchSnapshot(string sql)
    {
        using var con = Database.CreateConnection();

        var result = await con.QueryAsync(sql);

        var snapshot = new
        {
            result,
            sql
        };

        snapshot.Should().MatchSnapshotWithTestName();
    }

    private static IEnumerable<TestCaseData> ExplainTestCases()
    {
        yield return new TestCaseData(
            """
            EXPLAIN QUERY PLAN SELECT FeedId, ItemId
            FROM ProcessedPosts
            """).SetName("select from table w/o filter");

        yield return new TestCaseData(
            """
            EXPLAIN QUERY PLAN SELECT FeedId, ItemId
            FROM ProcessedPosts
            WHERE FeedId = 'https://www.heise.de/rss/heise-atom.xml'
            """).SetName("select from table with filter by FeedId");

        yield return new TestCaseData(
            """
            EXPLAIN QUERY PLAN SELECT FeedId, ItemId
            FROM ProcessedPosts
            WHERE FeedId = 'https://www.heise.de/rss/heise-atom.xml'
              AND ItemId IN ( 'http://heise.de/-10038958', 'http://heise.de/-10035763' )
            """).SetName("select from table with filter by FeedId and ItemId");

        yield return new TestCaseData(
            """
            EXPLAIN QUERY PLAN SELECT count(*)
            FROM ProcessedPosts
            """).SetName("select count from table w/o filter");

        yield return new TestCaseData(
            """
            EXPLAIN QUERY PLAN SELECT count(*)
            FROM ProcessedPosts
            WHERE FeedId = 'https://www.heise.de/rss/heise-atom.xml'
            """).SetName("select count from table with filter by FeedId");

        yield return new TestCaseData(
            """
            EXPLAIN QUERY PLAN SELECT count(*)
            FROM ProcessedPosts
            WHERE FeedId = 'https://www.heise.de/rss/heise-atom.xml'
              AND Timestamp >= 1731683439 AND Timestamp < 1731693439
              AND StatusId IS NOT NULL;
            """).SetName("select count from table with filter by FeedId, Timestamp and StatusId");

        yield return new TestCaseData(
            """
            EXPLAIN QUERY PLAN SELECT DISTINCT FeedId
            from ProcessedPosts;
            """).SetName("select distinct from table");

        yield return new TestCaseData(
            """
            EXPLAIN QUERY PLAN SELECT FeedTitle
            FROM ProcessedPosts 
            WHERE FeedId = 'https://www.heise.de/rss/heise-atom.xml'
            ORDER BY Timestamp DESC
            LIMIT 1
            """).SetName("select from table with filter, order, limit");
    }
}