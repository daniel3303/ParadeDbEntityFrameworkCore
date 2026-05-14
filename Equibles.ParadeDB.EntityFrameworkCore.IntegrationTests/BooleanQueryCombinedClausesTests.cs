using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class BooleanQueryCombinedClausesTests(ParadeDbFixture fixture)
{
    // Verifies ParadeDbBooleanQuery emits all three clause kinds (must/should/must_not)
    // in a single boolean JSON node and pg_search accepts them together. Existing
    // JsonQueryTests cover Must+MustNot and Should in isolation; if a refactor of the
    // three parallel branches in ParadeDbBooleanQuery.ToJsonNode() dropped any one
    // clause type, those tests would still pass — only a combined query exercises
    // the interaction. Must (rating >= 4) keeps Neural/Transformer/Cooking;
    // MustNot (category=cooking) drops Cooking; Should boosts machine-learning
    // matches but is optional alongside Must.
    [Fact]
    public async Task JsonSearch_BooleanWithMustShouldMustNot_ReturnsRowsSatisfyingMustAndNotMustNot()
    {
        await using var ctx = fixture.CreateDbContext();

        var query = ParadeDbJsonQuery.Boolean(b =>
            b.Must(
                    ParadeDbJsonQuery.Range(
                        "rating",
                        lowerBound: 4,
                        upperBound: 5,
                        lowerInclusive: true,
                        upperInclusive: true
                    )
                )
                .Should(ParadeDbJsonQuery.Term("category", "machine-learning"))
                .MustNot(ParadeDbJsonQuery.Term("category", "cooking"))
        );

        var hits = await ctx
            .Articles.JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.Contains(hits, t => t == "Transformer architectures");
        Assert.DoesNotContain(hits, t => t == "Cooking pasta perfectly");
        Assert.DoesNotContain(hits, t => t == "Quantum computing fundamentals");
    }
}
