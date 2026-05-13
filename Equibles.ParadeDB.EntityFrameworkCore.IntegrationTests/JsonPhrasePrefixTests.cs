using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonPhrasePrefixTests(ParadeDbFixture fixture) {
    // Verifies ParadeDbJsonQuery.PhrasePrefix builds {"phrase_prefix":{"field":"...","phrases":[...]}}
    // — the last phrase element is treated as a prefix (useful for autocomplete). The CLR
    // PhrasePrefix translates via pdb.phrase_prefix(ARRAY[...]); this JSON form goes through
    // the @@@ jsonb path. A regression in the phrases array or field key would either fail
    // to match or change the match set entirely.
    [Fact]
    public async Task PhrasePrefix_LastTermAsPrefix_MatchesContainingDocument() {
        await using var ctx = fixture.CreateDbContext();

        var query = ParadeDbJsonQuery.PhrasePrefix("content", "neural", "net");

        var hits = await ctx.Articles
            .JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        // "neural net*" prefix-matches "Neural networks" in Article 1's content;
        // unrelated articles (cooking, quantum) must not match.
        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.DoesNotContain(hits, t => t == "Cooking pasta perfectly");
        Assert.DoesNotContain(hits, t => t == "Quantum computing fundamentals");
    }
}
