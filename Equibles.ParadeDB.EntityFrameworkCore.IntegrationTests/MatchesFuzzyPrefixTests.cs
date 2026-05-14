using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class MatchesFuzzyPrefixTests(ParadeDbFixture fixture) {
    // Verifies the 5-arg MatchesFuzzy overload's "prefix" flag — exempts the
    // initial substring from edit distance, equivalent to prefix matching.
    // MatchesFuzzyOptionsTests and MatchesAllFuzzyOptionsTests both only
    // vary transpositionCostOne; the prefix flag routes through the same
    // BuildFuzzyMatchFunc helper but its CLR-to-named-arg wiring is otherwise
    // untested. A regression that swaps the prefix and transpositionCostOne
    // args (both bool, same position-class) would flip this test.
    [Fact]
    public async Task MatchesFuzzy_WithPrefixTrue_MatchesTokensStartingWithQuery() {
        await using var ctx = fixture.CreateDbContext();

        // "neurla" is Levenshtein distance 2 from "neural". With distance: 1
        // and prefix: false → no match (distance too small).
        // With distance: 1 and prefix: true → the prefix flag exempts the
        // initial substring from edit distance, so distance: 1 is enough
        // → matches Article 1.
        var prefixOff = await ctx.Articles
            .Where(a => EF.Functions.MatchesFuzzy(a.Content, "neurla", 1,
                prefix: false, transpositionCostOne: false))
            .Select(a => a.Title)
            .ToListAsync();
        var prefixOn = await ctx.Articles
            .Where(a => EF.Functions.MatchesFuzzy(a.Content, "neurla", 1,
                prefix: true, transpositionCostOne: false))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.DoesNotContain(prefixOff, t => t == "Introduction to neural networks");
        Assert.Contains(prefixOn, t => t == "Introduction to neural networks");
    }
}
