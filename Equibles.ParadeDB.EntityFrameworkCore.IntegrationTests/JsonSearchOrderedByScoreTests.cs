using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonSearchOrderedByScoreTests(ParadeDbFixture fixture)
{
    // Verifies the realistic composition of ParadeDbSearchExtensions.JsonSearch (predicate)
    // with OrderByScoreDescending (ordering) — both build reflection-driven expression trees
    // via the same BoxIfNeeded/StripConvert helpers. A regression in either helper that
    // only manifests when the @@@ predicate and pdb.score() ordering coexist in one query
    // (e.g., a missed Convert for the value-type key when chained) would slip past the
    // existing tests that exercise them in isolation.
    [Fact]
    public async Task JsonSearch_ChainedWithOrderByScoreDescending_OrdersResultsByRelevance()
    {
        await using var ctx = fixture.CreateDbContext();

        var query = ParadeDbJsonQuery.Boolean(b =>
            b.Should(
                ParadeDbJsonQuery.Parse("neural"),
                ParadeDbJsonQuery.Parse("transformer"),
                ParadeDbJsonQuery.Parse("quantum")
            )
        );

        var ranked = await ctx
            .Articles.JsonSearch(a => a.Id, query)
            .OrderByScoreDescending(a => a.Id)
            .Select(a => new { a.Title, Score = EF.Functions.Score(a.Id) })
            .ToListAsync();

        Assert.True(
            ranked.Count >= 2,
            $"Need ≥2 matches for ordering to be observable; got {ranked.Count}."
        );
        Assert.All(ranked, r => Assert.True(r.Score > 0, $"Score should be > 0; was {r.Score}."));
        for (var i = 1; i < ranked.Count; i++)
        {
            Assert.True(
                ranked[i - 1].Score >= ranked[i].Score,
                $"Score at position {i - 1} ({ranked[i - 1].Score}) should be >= score at {i} ({ranked[i].Score})."
            );
        }
    }
}
