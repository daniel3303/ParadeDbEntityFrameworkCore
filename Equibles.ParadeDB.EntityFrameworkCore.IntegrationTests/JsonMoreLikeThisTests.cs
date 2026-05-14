using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonMoreLikeThisTests(ParadeDbFixture fixture)
{
    // Verifies ParadeDbJsonQuery.MoreLikeThis builds {"more_like_this":{"key_value":N}}
    // and pg_search accepts it via the @@@ jsonb path. Distinct from the CLR MoreLikeThis
    // (which translates via pdb.more_like_this(documentId)) — a regression in the JSON
    // key names would still return zero matches without surfacing the underlying mistake.
    [Fact]
    public async Task MoreLikeThis_FromSeedArticle_ExecutesAndReturnsSimilarArticle()
    {
        await using var ctx = fixture.CreateDbContext();
        var seed = await ctx.Articles.SingleAsync(a =>
            a.Title == "Introduction to neural networks"
        );

        var query = ParadeDbJsonQuery.MoreLikeThis(seed.Id);

        var related = await ctx
            .Articles.JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        // pg_search's similarity threshold is weak on the seeded corpus, so we only
        // assert the most-similar candidate is present (matches the existing CLR MLT test).
        Assert.Contains(related, t => t == "Transformer architectures");
    }
}
