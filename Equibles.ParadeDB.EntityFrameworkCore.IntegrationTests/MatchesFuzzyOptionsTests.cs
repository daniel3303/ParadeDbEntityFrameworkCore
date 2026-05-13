using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class MatchesFuzzyOptionsTests(ParadeDbFixture fixture) {
    // Verifies the 5-arg MatchesFuzzy overload translates the transpositionCostOne flag
    // into pdb.fuzzy(distance, prefix, transposition_cost_one) so an adjacent-character
    // swap counts as 1 edit (default: 2). "nueral" vs "neural" has Levenshtein distance 2
    // but transposition distance 1 — at distance=1, transpositionCostOne is the only thing
    // that makes the match succeed. A regression that drops the extra args (or swaps them)
    // would flip both assertions.
    [Fact(Skip = "GH-15 — 5-arg MatchesFuzzy emits invalid SQL: type modifiers must be simple constants")]
    public async Task MatchesFuzzy_WithTranspositionCostOne_MatchesAdjacentSwapAtDistance1() {
        await using var ctx = fixture.CreateDbContext();

        var withTransposition = await ctx.Articles
            .Where(a => EF.Functions.MatchesFuzzy(a.Content, "nueral", 1,
                prefix: false, transpositionCostOne: true))
            .Select(a => a.Title)
            .ToListAsync();
        var withoutTransposition = await ctx.Articles
            .Where(a => EF.Functions.MatchesFuzzy(a.Content, "nueral", 1,
                prefix: false, transpositionCostOne: false))
            .Select(a => a.Title)
            .ToListAsync();

        // With transposition cost = 1, "nueral" ↔ "neural" is a single edit → matches.
        Assert.Contains(withTransposition, t => t == "Introduction to neural networks");
        // Without transposition cost = 1, the same query exceeds distance=1 → no match.
        Assert.DoesNotContain(withoutTransposition, t => t == "Introduction to neural networks");
    }
}
