using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class MatchesAllFuzzyOptionsTests(ParadeDbFixture fixture) {
    // Verifies the 5-arg MatchesAllFuzzy overload emits conjunction_mode => true on the
    // shared pdb.match(...) translation path (see BuildFuzzyMatchFunc in the translator).
    // The OR-mode sibling — MatchesFuzzy 5-arg — goes through the same helper without that
    // flag and is already covered by MatchesFuzzyOptionsTests. A regression that copies the
    // OR branch (or forgets the conjunction_mode arg) would let docs match on a single
    // fuzzy term, making AND-fuzzy silently behave like OR-fuzzy.
    [Fact]
    public async Task MatchesAllFuzzy_WithFullOptions_RequiresEveryTermToFuzzyMatch() {
        await using var ctx = fixture.CreateDbContext();

        // "nueral" ↔ "neural" is an adjacent-character swap (transposition distance 1).
        // "xyzzzzz" has no edit-distance-1 neighbor anywhere in the seeded content.
        // OR-fuzzy must still match Article 1 on the "nueral" hit alone.
        var orHits = await ctx.Articles
            .Where(a => EF.Functions.MatchesFuzzy(a.Content, "nueral xyzzzzz", 1,
                prefix: false, transpositionCostOne: true))
            .Select(a => a.Title)
            .ToListAsync();
        // AND-fuzzy must require every term — "xyzzzzz" fuzzy-matches nothing → no hits.
        var andHits = await ctx.Articles
            .Where(a => EF.Functions.MatchesAllFuzzy(a.Content, "nueral xyzzzzz", 1,
                prefix: false, transpositionCostOne: true))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(orHits, t => t == "Introduction to neural networks");
        Assert.Empty(andHits);
    }
}
