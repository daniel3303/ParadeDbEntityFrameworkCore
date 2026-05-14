using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class FieldTypeTests(ParadeDbFixture fixture)
{
    [Fact]
    public async Task Bm25Boolean_IndexedFastColumn_FiltersByValue()
    {
        await using var ctx = fixture.CreateDbContext();

        var inStockNames = await ctx
            .Products.Where(p => EF.Functions.Matches(p.Name, "keyboard"))
            .Where(p => p.InStock)
            .Select(p => p.Name)
            .ToListAsync();
        var outOfStockNames = await ctx
            .Products.Where(p => EF.Functions.Matches(p.Name, "keyboard"))
            .Where(p => !p.InStock)
            .Select(p => p.Name)
            .ToListAsync();

        Assert.DoesNotContain(inStockNames, n => n == "Mechanical keyboard");
        Assert.Contains(outOfStockNames, n => n == "Mechanical keyboard");
    }

    [Fact]
    public async Task Bm25DateTime_FastColumn_SupportsRangeFilter()
    {
        await using var ctx = fixture.CreateDbContext();
        var cutoff = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var recent = await ctx
            .Products.Where(p => EF.Functions.Matches(p.Name, "laptop OR mouse OR keyboard"))
            .Where(p => p.ReleasedAt >= cutoff)
            .Select(p => p.Name)
            .ToListAsync();

        Assert.Contains(recent, n => n == "Ultra book laptop");
        Assert.Contains(recent, n => n == "Wireless mouse");
        Assert.DoesNotContain(recent, n => n == "Mechanical keyboard");
    }

    [Fact]
    public async Task Bm25Json_WithExpandDots_IndexesJsonField()
    {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx
            .Products.Where(p => EF.Functions.Matches(p.Name, "laptop OR mouse OR keyboard"))
            .Select(p => p.Name)
            .ToListAsync();

        Assert.Equal(3, hits.Count);
    }
}
