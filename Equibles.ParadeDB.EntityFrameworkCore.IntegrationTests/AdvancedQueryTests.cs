using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class AdvancedQueryTests(ParadeDbFixture fixture)
{
    [Fact]
    public async Task Parse_TantivySyntax_FiltersByField()
    {
        await using var ctx = fixture.CreateDbContext();

        var results = await ctx
            .Articles.Where(a => EF.Functions.Parse(a.Id, "title:transformer"))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(results, t => t == "Transformer architectures");
        Assert.DoesNotContain(results, t => t == "Cooking pasta perfectly");
    }

    [Fact]
    public async Task MoreLikeThis_ExecutesAndReturnsRelatedArticle()
    {
        await using var ctx = fixture.CreateDbContext();

        var seed = await ctx.Articles.SingleAsync(a =>
            a.Title == "Introduction to neural networks"
        );

        var related = await ctx
            .Articles.Where(a => EF.Functions.MoreLikeThis(a.Id, seed.Id))
            .Select(a => a.Title)
            .ToListAsync();

        // With only a handful of seeded documents, pg_search's similarity threshold is weak,
        // so we don't assert what gets *excluded* — only that the function returns and the
        // most-similar candidate (Transformers article) is present.
        Assert.Contains(related, t => t == "Transformer architectures");
    }

    [Fact]
    public async Task JsonSearch_BooleanQuery_CombinesParseAndTerm()
    {
        await using var ctx = fixture.CreateDbContext();

        var query = ParadeDbJsonQuery.Boolean(b =>
            b.Must(
                ParadeDbJsonQuery.Parse("neural"),
                ParadeDbJsonQuery.Term("category", "machine-learning")
            )
        );

        var results = await ctx
            .Articles.JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(results, t => t == "Introduction to neural networks");
        Assert.DoesNotContain(results, t => t == "Quantum computing fundamentals");
    }
}
