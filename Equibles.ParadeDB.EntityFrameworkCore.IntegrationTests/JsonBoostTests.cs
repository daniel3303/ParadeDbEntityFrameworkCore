using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonBoostTests(ParadeDbFixture fixture) {
    // Verifies ParadeDbJsonQuery.Boost wraps an inner query into {"boost":{"query":...,"factor":N}}
    // and that the factor actually amplifies BM25 scores via the JSON @@@ operator path.
    // A regression in CloneNode (e.g., shared mutable JsonNode reuse) or JSON shape would
    // either fail to serialize, fail to match, or return un-amplified scores.
    [Fact]
    public async Task Boost_WrapsInnerQuery_AmplifiesScoreVsUnwrappedQuery() {
        await using var ctx = fixture.CreateDbContext();
        const double boostFactor = 5.0;

        var inner = ParadeDbJsonQuery.Parse("neural");
        var boosted = ParadeDbJsonQuery.Boost(inner, boostFactor);

        var boostedResults = await ctx.Articles
            .JsonSearch(a => a.Id, boosted)
            .Select(a => new { a.Title, Score = EF.Functions.Score(a.Id) })
            .ToListAsync();
        var unboostedResults = await ctx.Articles
            .JsonSearch(a => a.Id, inner)
            .Select(a => new { a.Title, Score = EF.Functions.Score(a.Id) })
            .ToListAsync();

        // Boost only changes scores, not the matching set.
        Assert.Equal(unboostedResults.Select(x => x.Title).OrderBy(t => t),
            boostedResults.Select(x => x.Title).OrderBy(t => t));
        Assert.Contains(boostedResults, x => x.Title == "Introduction to neural networks");

        var boostedHit = boostedResults.Single(x => x.Title == "Introduction to neural networks");
        var unboostedHit = unboostedResults.Single(x => x.Title == "Introduction to neural networks");
        Assert.True(unboostedHit.Score > 0,
            $"Sanity: unboosted Parse score should be positive; was {unboostedHit.Score}.");
        Assert.True(boostedHit.Score > unboostedHit.Score * 2.0,
            $"Boost factor {boostFactor} should substantially amplify the score; " +
            $"boosted={boostedHit.Score}, unboosted={unboostedHit.Score}.");
    }
}
