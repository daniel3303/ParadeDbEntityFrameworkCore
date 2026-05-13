using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonQueryTests(ParadeDbFixture fixture) {
    [Fact]
    public async Task JsonSearch_NumericRange_FiltersByRating() {
        await using var ctx = fixture.CreateDbContext();

        var query = ParadeDbJsonQuery.Range("rating", lowerBound: 4, upperBound: 5,
            lowerInclusive: true, upperInclusive: true);

        var hits = await ctx.Articles
            .JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Transformer architectures");
        Assert.DoesNotContain(hits, t => t == "Quantum computing fundamentals");
    }

    [Fact]
    public async Task JsonSearch_Should_OrSemantics() {
        await using var ctx = fixture.CreateDbContext();

        var query = ParadeDbJsonQuery.Boolean(b => b.Should(
            ParadeDbJsonQuery.Term("category", "cooking"),
            ParadeDbJsonQuery.Term("category", "physics")));

        var hits = await ctx.Articles
            .JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Cooking pasta perfectly");
        Assert.Contains(hits, t => t == "Quantum computing fundamentals");
        Assert.DoesNotContain(hits, t => t == "Introduction to neural networks");
    }

    [Fact]
    public async Task JsonSearch_MustNot_ExcludesMatchingDocs() {
        await using var ctx = fixture.CreateDbContext();

        var query = ParadeDbJsonQuery.Boolean(b => b
            .Must(ParadeDbJsonQuery.All())
            .MustNot(ParadeDbJsonQuery.Term("category", "cooking")));

        var hits = await ctx.Articles
            .JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.DoesNotContain(hits, t => t == "Cooking pasta perfectly");
        Assert.Contains(hits, t => t == "Introduction to neural networks");
    }

    [Fact]
    public async Task JsonSearch_InlineBuilder_OverloadWorks() {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx.Articles
            .JsonSearch(a => a.Id, b => b
                .Must(
                    ParadeDbJsonQuery.Parse("neural"),
                    ParadeDbJsonQuery.Term("category", "machine-learning")))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
    }
}
