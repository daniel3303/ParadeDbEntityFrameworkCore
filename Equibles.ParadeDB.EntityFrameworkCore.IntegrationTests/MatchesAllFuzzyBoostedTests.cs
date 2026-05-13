using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class MatchesAllFuzzyBoostedTests(ParadeDbFixture fixture) {
    // Verifies MatchesAllFuzzyBoosted translates to: column &&& 'query'::pdb.fuzzy(d)::pdb.boost(f).
    // Chains TWO cast operators on AND (&&&) — the most complex translation path in the
    // function set. A regression that drops either cast (fuzzy or boost), inverts cast
    // order, or swaps &&& for ||| would either change the result set (typos no longer
    // tolerated / OR semantics applied) or leave the score un-amplified.
    [Fact]
    public async Task MatchesAllFuzzyBoosted_TypoTolerantAndOperator_AmplifiesScoreVsPlainFuzzyAll() {
        await using var ctx = fixture.CreateDbContext();
        const int distance = 2;
        const double boostFactor = 5.0;

        var boosted = await ctx.Articles
            .Where(a => EF.Functions.MatchesAllFuzzyBoosted(a.Content, "nueral netwroks", distance, boostFactor))
            .Select(a => new { a.Title, Score = EF.Functions.Score(a.Id) })
            .ToListAsync();
        var unboosted = await ctx.Articles
            .Where(a => EF.Functions.MatchesAllFuzzy(a.Content, "nueral netwroks", distance))
            .Select(a => new { a.Title, Score = EF.Functions.Score(a.Id) })
            .ToListAsync();

        // Same AND+fuzzy semantics → identical matches; only the score should differ.
        Assert.Equal(unboosted.Select(x => x.Title).OrderBy(t => t),
            boosted.Select(x => x.Title).OrderBy(t => t));
        Assert.Contains(boosted, x => x.Title == "Introduction to neural networks");
        var boostedHit = boosted.Single(x => x.Title == "Introduction to neural networks");
        var unboostedHit = unboosted.Single(x => x.Title == "Introduction to neural networks");
        Assert.True(boostedHit.Score > unboostedHit.Score * 2.0,
            $"Boost factor {boostFactor} should amplify the fuzzy AND score; " +
            $"boosted={boostedHit.Score}, unboosted={unboostedHit.Score}.");
    }
}
