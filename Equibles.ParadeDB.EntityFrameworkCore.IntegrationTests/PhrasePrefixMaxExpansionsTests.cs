using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class PhrasePrefixMaxExpansionsTests(ParadeDbFixture fixture) {
    // Verifies the 3-arg PhrasePrefix overload (with maxExpansions) translates to
    // pdb.phrase_prefix(ARRAY[...], max_expansions => N) and runs. The 2-arg form is
    // already tested; this exercises the extra named arg, which — if parameterized by
    // the translator — would fail PostgreSQL parsing (cf. GH-15 for the fuzzy overloads).
    [Fact(Skip = "GH-17 — 3-arg PhrasePrefix emits nonexistent pdb.phrase_prefix(text[], max_expansions => int) signature")]
    public async Task PhrasePrefix_WithMaxExpansions_MatchesContainingDocument() {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx.Articles
            .Where(a => EF.Functions.PhrasePrefix(a.Content, 5, "neural", "net"))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.DoesNotContain(hits, t => t == "Cooking pasta perfectly");
    }
}
