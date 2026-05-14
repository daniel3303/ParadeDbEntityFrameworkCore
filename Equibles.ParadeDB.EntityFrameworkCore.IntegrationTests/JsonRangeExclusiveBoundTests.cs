using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonRangeExclusiveBoundTests(ParadeDbFixture fixture)
{
    // Verifies ParadeDbJsonQuery.Range emits "excluded" (not "included") when
    // lowerInclusive is false. The existing JsonQueryTests numeric-range test
    // only uses both bounds inclusive, so the "excluded" JSON-key branch is
    // never executed. A regression that swaps "included"/"excluded" or flips
    // the boolean condition would silently change the boundary semantics.
    [Fact]
    public async Task Range_WithExclusiveLowerBound_ExcludesBoundaryRating()
    {
        await using var ctx = fixture.CreateDbContext();

        // Article 2 has rating=4 (the boundary value); Articles 1+4 have rating=5.
        var inclusive = ParadeDbJsonQuery.Range(
            "rating",
            lowerBound: 4,
            upperBound: 5,
            lowerInclusive: true,
            upperInclusive: true
        );
        var exclusive = ParadeDbJsonQuery.Range(
            "rating",
            lowerBound: 4,
            upperBound: 5,
            lowerInclusive: false,
            upperInclusive: true
        );

        var inclusiveHits = await ctx
            .Articles.JsonSearch(a => a.Id, inclusive)
            .Select(a => a.Title)
            .ToListAsync();
        var exclusiveHits = await ctx
            .Articles.JsonSearch(a => a.Id, exclusive)
            .Select(a => a.Title)
            .ToListAsync();

        // Inclusive lower: rating=4 (Article 2) is in the hit set.
        Assert.Contains(inclusiveHits, t => t == "Transformer architectures");
        // Exclusive lower: rating=4 (Article 2) is excluded; rating=5 articles remain.
        Assert.DoesNotContain(exclusiveHits, t => t == "Transformer architectures");
        Assert.Contains(exclusiveHits, t => t == "Introduction to neural networks");
    }
}
