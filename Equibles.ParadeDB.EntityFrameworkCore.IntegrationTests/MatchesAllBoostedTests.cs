using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class MatchesAllBoostedTests(ParadeDbFixture fixture)
{
    // Verifies MatchesAllBoosted translates to: column &&& 'query'::pdb.boost(factor)
    // with AND semantics — distinct from MatchesBoosted (||| / OR). A regression in
    // the method-call translator that drops the boost cast, or swaps the operator,
    // would either return matches with unboosted scores or change the result set.
    [Fact]
    public async Task MatchesAllBoosted_AndOperator_AmplifiesScoreVsUnboostedMatchesAll()
    {
        await using var ctx = fixture.CreateDbContext();
        const double boostFactor = 5.0;

        var boosted = await ctx
            .Articles.Where(a =>
                EF.Functions.MatchesAllBoosted(a.Content, "neural networks", boostFactor)
            )
            .Select(a => new { a.Title, Score = EF.Functions.Score(a.Id) })
            .ToListAsync();
        var unboosted = await ctx
            .Articles.Where(a => EF.Functions.MatchesAll(a.Content, "neural networks"))
            .Select(a => new { a.Title, Score = EF.Functions.Score(a.Id) })
            .ToListAsync();

        // Same AND semantics → same matching rows; only the score should differ.
        Assert.Equal(
            unboosted.Select(x => x.Title).OrderBy(t => t),
            boosted.Select(x => x.Title).OrderBy(t => t)
        );
        Assert.Contains(boosted, x => x.Title == "Introduction to neural networks");
        var boostedHit = boosted.Single(x => x.Title == "Introduction to neural networks");
        var unboostedHit = unboosted.Single(x => x.Title == "Introduction to neural networks");
        Assert.True(
            boostedHit.Score > unboostedHit.Score * 2.0,
            $"Boost factor {boostFactor} should amplify score substantially; "
                + $"boosted={boostedHit.Score}, unboosted={unboostedHit.Score}."
        );
    }
}
