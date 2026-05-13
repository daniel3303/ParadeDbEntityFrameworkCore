using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class MoreLikeThisFieldsTests(ParadeDbFixture fixture) {
    // Verifies MoreLikeThis(keyField, documentId, params string[] fields) translates to
    // pdb.more_like_this(documentId, ARRAY['field1', ...]) — restricting the similarity
    // computation to the named fields. The 2-arg form is already covered; this exercises
    // the array-of-strings parameter, distinct from GH-15/GH-17 translation paths.
    [Fact]
    public async Task MoreLikeThis_WithFieldsRestriction_ExecutesAndReturnsSimilarArticle() {
        await using var ctx = fixture.CreateDbContext();
        var seed = await ctx.Articles.SingleAsync(a => a.Title == "Introduction to neural networks");

        var related = await ctx.Articles
            .Where(a => EF.Functions.MoreLikeThis(a.Id, seed.Id, "content"))
            .Select(a => a.Title)
            .ToListAsync();

        // pg_search similarity threshold is weak with so few seeded rows, so we don't
        // assert exclusions — only that the function executes and the most-similar
        // article (Transformers — shares "models", "running", etc.) is present.
        Assert.Contains(related, t => t == "Transformer architectures");
    }
}
