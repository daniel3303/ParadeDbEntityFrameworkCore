using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonDateTimeRangeTests(ParadeDbFixture fixture) {
    // Verifies ParadeDbJsonQuery.Range with isDatetime: true serializes UTC bounds as
    // ISO-8601 ("yyyy-MM-ddTHH:mm:ssZ") and adds "is_datetime": true so pg_search parses
    // them as timestamps, not strings. A regression in CreateJsonValue's DateTime branch
    // (wrong format, missing 'Z', non-UTC handling) or a missing is_datetime flag would
    // either fail to filter or return zero matches against a Bm25DateTime-indexed column.
    [Fact]
    public async Task DateTimeRange_FiltersByDateBoundary_IncludesOnlyProductsInRange() {
        await using var ctx = fixture.CreateDbContext();
        var lower = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var upper = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var query = ParadeDbJsonQuery.Range("released_at", lower, upper,
            lowerInclusive: true, upperInclusive: false, isDatetime: true);

        var hits = await ctx.Products
            .JsonSearch(p => p.Id, query)
            .Select(p => p.Name)
            .ToListAsync();

        // Seeded ReleasedAt: laptop=2024-06-01, mouse=2024-11-20 (in range);
        // keyboard=2023-01-15 (out of range).
        Assert.Contains(hits, n => n == "Ultra book laptop");
        Assert.Contains(hits, n => n == "Wireless mouse");
        Assert.DoesNotContain(hits, n => n == "Mechanical keyboard");
    }
}
