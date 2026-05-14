using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class BasicSearchTests(ParadeDbFixture fixture)
{
    [Fact]
    public async Task Matches_OrOperator_ReturnsMatchingDocuments()
    {
        await using var ctx = fixture.CreateDbContext();

        var results = await ctx
            .Articles.Where(a => EF.Functions.Matches(a.Content, "neural networks"))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(results, t => t == "Introduction to neural networks");
        Assert.DoesNotContain(results, t => t == "Cooking pasta perfectly");
    }

    [Fact]
    public async Task MatchesAll_AndOperator_RequiresAllTerms()
    {
        await using var ctx = fixture.CreateDbContext();

        var results = await ctx
            .Articles.Where(a => EF.Functions.MatchesAll(a.Content, "attention transformers"))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Single(results);
        Assert.Equal("Transformer architectures", results[0]);
    }

    [Fact]
    public async Task MatchesPhrase_RequiresExactOrder()
    {
        await using var ctx = fixture.CreateDbContext();

        var withExactPhrase = await ctx
            .Articles.Where(a => EF.Functions.MatchesPhrase(a.Content, "neural networks"))
            .CountAsync();
        var withReversedPhrase = await ctx
            .Articles.Where(a => EF.Functions.MatchesPhrase(a.Content, "networks neural"))
            .CountAsync();

        Assert.Equal(1, withExactPhrase);
        Assert.Equal(0, withReversedPhrase);
    }

    [Fact]
    public async Task MatchesFuzzy_ToleratesTypos()
    {
        await using var ctx = fixture.CreateDbContext();

        var results = await ctx
            .Articles.Where(a => EF.Functions.MatchesFuzzy(a.Content, "nueral", 2))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(results, t => t == "Introduction to neural networks");
    }

    [Fact]
    public async Task MatchesTerm_RequiresExactToken()
    {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx
            .Articles.Where(a => EF.Functions.MatchesTerm(a.Content, "gpus"))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
    }
}
