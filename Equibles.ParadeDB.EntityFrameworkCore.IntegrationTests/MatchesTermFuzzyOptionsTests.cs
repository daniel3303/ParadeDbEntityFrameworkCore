using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class MatchesTermFuzzyOptionsTests(ParadeDbFixture fixture)
{
    // Verifies the 5-arg MatchesTermFuzzy overload translates to
    // pdb.fuzzy_term(value, distance, transposition_cost_one, prefix) — a POSITIONAL
    // function call (BuildFuzzyTermFunc), distinct from the named-arg pdb.match(...)
    // path used by MatchesFuzzy/MatchesAllFuzzy 5-arg. A regression that swaps
    // the positional order (e.g. emits prefix before transposition_cost_one) would
    // flip the meaning of the two booleans and silently invert both assertions.
    [Fact]
    public async Task MatchesTermFuzzy_WithTranspositionCostOne_MatchesAdjacentSwapAtDistance1()
    {
        await using var ctx = fixture.CreateDbContext();

        // "nueral" ↔ "neural" is an adjacent swap (transposition distance 1, Levenshtein 2).
        var withTransposition = await ctx
            .Articles.Where(a =>
                EF.Functions.MatchesTermFuzzy(
                    a.Content,
                    "nueral",
                    1,
                    prefix: false,
                    transpositionCostOne: true
                )
            )
            .Select(a => a.Title)
            .ToListAsync();
        var withoutTransposition = await ctx
            .Articles.Where(a =>
                EF.Functions.MatchesTermFuzzy(
                    a.Content,
                    "nueral",
                    1,
                    prefix: false,
                    transpositionCostOne: false
                )
            )
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(withTransposition, t => t == "Introduction to neural networks");
        Assert.DoesNotContain(withoutTransposition, t => t == "Introduction to neural networks");
    }
}
