using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonMatchDefaultFieldTests(ParadeDbFixture fixture) {
    // Verifies the 1-arg ParadeDbJsonQuery.Match(value) overload — emits
    // {"match":{"value":"..."}} with no "field" key. The 2-arg and 4-arg
    // overloads are tested elsewhere; the 1-arg path was never exercised
    // end-to-end. Probes pg_search's documented behavior of searching across
    // all indexed fields when no field is specified.
    [Fact(Skip = "GH-56 — 1-arg Match overload produces JSON pg_search rejects")]
    public async Task Match_ValueOnlyWithoutField_SearchesAllIndexedFields() {
        await using var ctx = fixture.CreateDbContext();

        // "neural" appears in Article 1's title AND content — either field
        // matching is enough to return the hit. Article 4 (cooking) has neither.
        var query = ParadeDbJsonQuery.Match("neural");

        var hits = await ctx.Articles
            .JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.DoesNotContain(hits, t => t == "Cooking pasta perfectly");
    }
}
