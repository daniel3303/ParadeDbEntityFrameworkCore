using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonRangeOpenEndedTests(ParadeDbFixture fixture) {
    // Verifies ParadeDbJsonQuery.Range omits the "upper_bound" key entirely
    // when upperBound is null — the `if (upperBound != null)` branch. The
    // existing JsonQueryTests/JsonDateTimeRangeTests always pass both bounds,
    // so a regression that always emits "upper_bound" (or crashes on null)
    // would only be caught here.
    [Fact]
    public async Task Range_WithNullUpperBound_MatchesAllValuesAtOrAboveLowerBound() {
        await using var ctx = fixture.CreateDbContext();

        // Ratings in the seed: Article 1=5, 2=4, 3=3, 4=5. lower=4 inclusive,
        // no upper → expect Articles 1, 2, 4 (rating >= 4); Article 3 excluded.
        var query = ParadeDbJsonQuery.Range("rating",
            lowerBound: 4, upperBound: null!, lowerInclusive: true);

        var hits = await ctx.Articles
            .JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.Contains(hits, t => t == "Transformer architectures");
        Assert.Contains(hits, t => t == "Cooking pasta perfectly");
        Assert.DoesNotContain(hits, t => t == "Quantum computing fundamentals");
    }
}
