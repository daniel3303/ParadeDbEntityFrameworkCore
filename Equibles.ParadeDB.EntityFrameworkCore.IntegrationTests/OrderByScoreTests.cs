using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class OrderByScoreTests(ParadeDbFixture fixture)
{
    // Verifies ParadeDbSearchExtensions.OrderByScoreDescending composes a Score() ORDER BY
    // equivalent to writing .OrderByDescending(a => EF.Functions.Score(a.Id)) by hand.
    // A regression in the reflection-built expression (wrong Queryable overload, missing
    // Convert for a value-type key, broken StripConvert) would either throw at translation
    // or produce a different ordering than the explicit form.
    [Fact]
    public async Task OrderByScoreDescending_RanksMatchingArticles_SameOrderAsExplicitScoreSelector()
    {
        await using var ctx = fixture.CreateDbContext();

        var viaExtension = await ctx
            .Articles.Where(a => EF.Functions.Matches(a.Content, "models machine learning"))
            .OrderByScoreDescending(a => a.Id)
            .Select(a => a.Title)
            .ToListAsync();

        var viaExplicit = await ctx
            .Articles.Where(a => EF.Functions.Matches(a.Content, "models machine learning"))
            .OrderByDescending(a => EF.Functions.Score(a.Id))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.True(
            viaExtension.Count >= 2,
            $"Need at least 2 matches for ordering to be observable; got {viaExtension.Count}."
        );
        Assert.Equal(viaExplicit, viaExtension);
    }
}
