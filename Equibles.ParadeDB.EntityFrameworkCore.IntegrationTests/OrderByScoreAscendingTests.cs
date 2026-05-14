using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class OrderByScoreAscendingTests(ParadeDbFixture fixture)
{
    // Verifies ParadeDbSearchExtensions.OrderByScore composes an ascending Score()
    // ORDER BY equivalent to .OrderBy(a => EF.Functions.Score(a.Id)) by hand. Pairs
    // with the already-covered OrderByScoreDescending — both branches of the shared
    // reflection-built ApplyScoreOrdering helper need a regression guard.
    [Fact]
    public async Task OrderByScore_RanksMatchingArticles_SameOrderAsExplicitScoreSelector()
    {
        await using var ctx = fixture.CreateDbContext();

        var viaExtension = await ctx
            .Articles.Where(a => EF.Functions.Matches(a.Content, "models machine learning"))
            .OrderByScore(a => a.Id)
            .Select(a => a.Title)
            .ToListAsync();

        var viaExplicit = await ctx
            .Articles.Where(a => EF.Functions.Matches(a.Content, "models machine learning"))
            .OrderBy(a => EF.Functions.Score(a.Id))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.True(
            viaExtension.Count >= 2,
            $"Need at least 2 matches for ordering to be observable; got {viaExtension.Count}."
        );
        Assert.Equal(viaExplicit, viaExtension);
    }
}
