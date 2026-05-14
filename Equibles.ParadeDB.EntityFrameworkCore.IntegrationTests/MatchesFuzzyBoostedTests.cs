using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class MatchesFuzzyBoostedTests(ParadeDbFixture fixture)
{
    // Verifies MatchesFuzzyBoosted translates to: column ||| 'query'::pdb.fuzzy(d)::pdb.boost(f).
    // OR variant of the already-tested MatchesAllFuzzyBoosted — same chained-cast path,
    // different boolean operator (||| vs &&&). A regression that swapped operators or
    // dropped a cast would either change the match set or leave the score un-amplified.
    [Fact]
    public async Task MatchesFuzzyBoosted_OrOperator_AmplifiesScoreVsUnboostedFuzzy()
    {
        await using var ctx = fixture.CreateDbContext();
        const int distance = 2;
        const double boostFactor = 5.0;

        var boosted = await ctx
            .Articles.Where(a =>
                EF.Functions.MatchesFuzzyBoosted(a.Content, "nueral", distance, boostFactor)
            )
            .Select(a => new { a.Title, Score = EF.Functions.Score(a.Id) })
            .ToListAsync();
        var unboosted = await ctx
            .Articles.Where(a => EF.Functions.MatchesFuzzy(a.Content, "nueral", distance))
            .Select(a => new { a.Title, Score = EF.Functions.Score(a.Id) })
            .ToListAsync();

        // Same OR+fuzzy semantics → identical matches; only the score should differ.
        Assert.Equal(
            unboosted.Select(x => x.Title).OrderBy(t => t),
            boosted.Select(x => x.Title).OrderBy(t => t)
        );
        Assert.Contains(boosted, x => x.Title == "Introduction to neural networks");
        var boostedHit = boosted.Single(x => x.Title == "Introduction to neural networks");
        var unboostedHit = unboosted.Single(x => x.Title == "Introduction to neural networks");
        Assert.True(
            boostedHit.Score > unboostedHit.Score * 2.0,
            $"Boost factor {boostFactor} should amplify the fuzzy OR score; "
                + $"boosted={boostedHit.Score}, unboosted={unboostedHit.Score}."
        );
    }
}
