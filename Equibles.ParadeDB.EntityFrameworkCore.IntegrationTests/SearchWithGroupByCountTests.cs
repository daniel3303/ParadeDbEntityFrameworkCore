using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class SearchWithGroupByCountTests(ParadeDbFixture fixture)
{
    // Verifies that ParadeDbQuerySqlGenerator emits a valid Postgres query when a @@@ predicate
    // is composed with GROUP BY + COUNT — the SQL shape behind any "hits per facet" UI. No
    // existing integration test exercises GROUP BY alongside ParadeDB operators, so a regression
    // in aggregate composition (e.g., the search predicate getting pushed past the GROUP BY)
    // would slip past the single-row Where/Select tests.
    [Fact]
    public async Task Search_GroupedByCategory_ReturnsHitCountPerCategory()
    {
        await using var ctx = fixture.CreateDbContext();

        var counts = await ctx
            .Articles.Where(a => EF.Functions.Matches(a.Content, "models OR pasta"))
            .GroupBy(a => a.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderBy(x => x.Category)
            .ToListAsync();

        Assert.Contains(counts, c => c.Category == "machine-learning" && c.Count == 2);
        Assert.Contains(counts, c => c.Category == "cooking" && c.Count == 1);
        Assert.DoesNotContain(counts, c => c.Category == "physics");
    }
}
