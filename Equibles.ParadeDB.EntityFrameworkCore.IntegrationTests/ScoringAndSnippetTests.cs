using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class ScoringAndSnippetTests(ParadeDbFixture fixture)
{
    [Fact]
    public async Task Score_OrdersByRelevance()
    {
        await using var ctx = fixture.CreateDbContext();

        var ranked = await ctx
            .Articles.Where(a => EF.Functions.Matches(a.Content, "neural networks"))
            .Select(a => new { a.Title, Score = EF.Functions.Score(a.Id) })
            .OrderByDescending(x => x.Score)
            .ToListAsync();

        Assert.NotEmpty(ranked);
        Assert.True(ranked[0].Score > 0);
        if (ranked.Count > 1)
        {
            Assert.True(
                ranked[0].Score >= ranked[1].Score,
                "Results should be ordered by descending score."
            );
        }
    }

    [Fact]
    public async Task Snippet_HighlightsMatchedTerms()
    {
        await using var ctx = fixture.CreateDbContext();

        var snippets = await ctx
            .Articles.Where(a => EF.Functions.Matches(a.Content, "neural networks"))
            .Select(a => EF.Functions.Snippet(a.Content, "<mark>", "</mark>", 100))
            .ToListAsync();

        Assert.NotEmpty(snippets);
        Assert.Contains(snippets, s => s != null && s.Contains("<mark>") && s.Contains("</mark>"));
    }
}
