using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonPhraseSlopTests(ParadeDbFixture fixture)
{
    // Verifies ParadeDbJsonQuery.Phrase with slop produces {"phrase":{"field":"...","phrases":[...],"slop":N}}
    // and pg_search allows N words between phrase terms. Article 1's content has "Deep learning models" —
    // strict "deep models" doesn't match (1 word in between), but slop>=1 does.
    // A regression that dropped the "slop" key would fail the test by returning zero matches.
    [Fact]
    public async Task Phrase_WithSlop_AllowsWordsBetweenTerms()
    {
        await using var ctx = fixture.CreateDbContext();

        var strict = await ctx
            .Articles.JsonSearch(a => a.Id, ParadeDbJsonQuery.Phrase("content", "deep", "models"))
            .Select(a => a.Title)
            .ToListAsync();
        var withSlop = await ctx
            .Articles.JsonSearch(
                a => a.Id,
                ParadeDbJsonQuery.Phrase("content", 2, "deep", "models")
            )
            .Select(a => a.Title)
            .ToListAsync();

        Assert.DoesNotContain(strict, t => t == "Introduction to neural networks");
        Assert.Contains(withSlop, t => t == "Introduction to neural networks");
    }
}
