using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonDisjunctionMaxTests(ParadeDbFixture fixture)
{
    // Verifies ParadeDbJsonQuery.DisjunctionMax produces {"disjunction_max":{"disjuncts":[...]}}
    // with OR matching semantics — a document matching ANY disjunct appears once, even when
    // it matches multiple. Distinct from Boolean.Should (which sums scores). A regression in
    // CloneNode usage across the disjuncts array, or the disjuncts being misnamed, would
    // either fail to deserialize or change the matching set.
    [Fact]
    public async Task DisjunctionMax_OverlappingDisjuncts_ReturnsDistinctUnionOfMatches()
    {
        await using var ctx = fixture.CreateDbContext();

        // "neural" matches Article 1 only; "machine-learning" category matches Articles 1 and 2.
        // Disjunction → union (Articles 1, 2). Article 1 is in both disjuncts but must appear once.
        var query = ParadeDbJsonQuery.DisjunctionMax(
            ParadeDbJsonQuery.Parse("neural"),
            ParadeDbJsonQuery.Term("category", "machine-learning")
        );

        var hits = await ctx
            .Articles.JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Equal(2, hits.Count);
        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.Contains(hits, t => t == "Transformer architectures");
        Assert.DoesNotContain(hits, t => t == "Quantum computing fundamentals");
        Assert.DoesNotContain(hits, t => t == "Cooking pasta perfectly");
    }
}
