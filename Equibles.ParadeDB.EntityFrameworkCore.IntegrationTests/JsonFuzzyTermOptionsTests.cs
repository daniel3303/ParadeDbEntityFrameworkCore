using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonFuzzyTermOptionsTests(ParadeDbFixture fixture) {
    // Verifies ParadeDbJsonQuery.FuzzyTerm(field, value, distance, prefix,
    // transpositionCostOne) conditionally emits the JSON keys "prefix" and
    // "transposition_cost_one" on the {"fuzzy_term":{...}} node. Distinct from
    // the CLR MatchesTermFuzzy 5-arg path (pdb.fuzzy_term positional function).
    // A regression renaming "transposition_cost_one" (or hard-coding it) would
    // silently make distance=1 reject the adjacent-swap that the test relies on.
    [Fact]
    public async Task FuzzyTerm_WithTranspositionCostOne_MatchesAdjacentSwapAtDistance1() {
        await using var ctx = fixture.CreateDbContext();

        // "nueral" ↔ "neural" is an adjacent transposition (transp distance 1, Levenshtein 2).
        var withTransposition = ParadeDbJsonQuery.FuzzyTerm("content", "nueral", 1,
            prefix: false, transpositionCostOne: true);
        var withoutTransposition = ParadeDbJsonQuery.FuzzyTerm("content", "nueral", 1,
            prefix: false, transpositionCostOne: false);

        var hitsWith = await ctx.Articles
            .JsonSearch(a => a.Id, withTransposition)
            .Select(a => a.Title)
            .ToListAsync();
        var hitsWithout = await ctx.Articles
            .JsonSearch(a => a.Id, withoutTransposition)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hitsWith, t => t == "Introduction to neural networks");
        Assert.DoesNotContain(hitsWithout, t => t == "Introduction to neural networks");
    }
}
