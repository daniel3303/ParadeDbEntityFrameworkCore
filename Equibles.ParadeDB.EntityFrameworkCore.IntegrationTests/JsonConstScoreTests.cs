using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonConstScoreTests(ParadeDbFixture fixture)
{
    // Verifies ParadeDbJsonQuery.ConstScore produces {"const_score":{"query":...,"score":N}}
    // and that pg_search overrides the BM25 score with N for every matching document. This
    // differs from Boost (which multiplies the existing score). A regression that swapped
    // "score" → "factor", or mis-wired CloneNode, would either fail to apply the override
    // or change the result set.
    [Fact]
    public async Task ConstScore_WrapsInnerQuery_FixesScoreToConstantForAllMatches()
    {
        await using var ctx = fixture.CreateDbContext();
        const double constantScore = 3.14;

        var inner = ParadeDbJsonQuery.Term("category", "machine-learning");
        var query = ParadeDbJsonQuery.ConstScore(inner, constantScore);

        var results = await ctx
            .Articles.JsonSearch(a => a.Id, query)
            .Select(a => new { a.Title, Score = EF.Functions.Score(a.Id) })
            .ToListAsync();

        // Two articles share the machine-learning category — both must come back at score=3.14.
        Assert.Equal(2, results.Count);
        Assert.Contains(results, x => x.Title == "Introduction to neural networks");
        Assert.Contains(results, x => x.Title == "Transformer architectures");
        foreach (var hit in results)
        {
            Assert.True(
                Math.Abs(hit.Score - constantScore) < 0.001,
                $"ConstScore should fix score to {constantScore}; got {hit.Score} for '{hit.Title}'."
            );
        }
    }
}
