using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonTermSetTests(ParadeDbFixture fixture) {
    // Verifies ParadeDbJsonQuery.TermSet produces {"term_set":{"field":"...","terms":[...]}}
    // — matches any of the listed exact terms on the Raw-tokenized category column.
    // Distinct from CLR MatchesTermSet (which translates to `=== ARRAY[...]`); this
    // exercises the JSON @@@ jsonb path and the params object[] → JsonArray conversion.
    [Fact]
    public async Task TermSet_MultipleCategories_MatchesAnyListedTerm() {
        await using var ctx = fixture.CreateDbContext();

        var query = ParadeDbJsonQuery.TermSet("category", "machine-learning", "cooking");

        var hits = await ctx.Articles
            .JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        // 2 articles in machine-learning + 1 in cooking = 3 hits; physics excluded.
        Assert.Equal(3, hits.Count);
        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.Contains(hits, t => t == "Transformer architectures");
        Assert.Contains(hits, t => t == "Cooking pasta perfectly");
        Assert.DoesNotContain(hits, t => t == "Quantum computing fundamentals");
    }
}
