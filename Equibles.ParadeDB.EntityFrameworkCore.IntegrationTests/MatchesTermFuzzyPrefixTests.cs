using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class MatchesTermFuzzyPrefixTests(ParadeDbFixture fixture) {
    // Verifies the 5-arg MatchesTermFuzzy overload's "prefix" flag — routes
    // through BuildFuzzyTermFunc which emits pdb.fuzzy_term(value, distance,
    // transposition_cost_one, prefix) as a POSITIONAL call. The existing
    // MatchesTermFuzzyOptionsTests only varies transpositionCostOne (fixing
    // prefix at false), so a regression that swaps positional args 3 and 4
    // would slip through it but be caught here.
    [Fact]
    public async Task MatchesTermFuzzy_WithPrefixTrue_ExtendsMatchPastEditDistance() {
        await using var ctx = fixture.CreateDbContext();

        // "neurla" → "neural" is Levenshtein distance 2. At distance: 1 with
        // prefix: false, no match (distance too small). With prefix: true the
        // initial substring is exempt from edit distance, allowing the match.
        var prefixOff = await ctx.Articles
            .Where(a => EF.Functions.MatchesTermFuzzy(a.Content, "neurla", 1,
                prefix: false, transpositionCostOne: false))
            .Select(a => a.Title)
            .ToListAsync();
        var prefixOn = await ctx.Articles
            .Where(a => EF.Functions.MatchesTermFuzzy(a.Content, "neurla", 1,
                prefix: true, transpositionCostOne: false))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.DoesNotContain(prefixOff, t => t == "Introduction to neural networks");
        Assert.Contains(prefixOn, t => t == "Introduction to neural networks");
    }
}
