using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonExistsTests(ParadeDbFixture fixture) {
    // Verifies ParadeDbJsonQuery.Exists produces {"exists":{"field":"..."}} and pg_search
    // returns every row that has the named field indexed. The two-key shape is small, so
    // the test serves mainly as a regression guard against typos in "exists"/"field".
    [Fact]
    public async Task Exists_OnPopulatedField_MatchesAllIndexedRows() {
        await using var ctx = fixture.CreateDbContext();

        var query = ParadeDbJsonQuery.Exists("category");

        var hits = await ctx.Articles
            .JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        // All 4 seeded articles have a category value, so Exists matches the whole corpus.
        Assert.Equal(4, hits.Count);
    }
}
