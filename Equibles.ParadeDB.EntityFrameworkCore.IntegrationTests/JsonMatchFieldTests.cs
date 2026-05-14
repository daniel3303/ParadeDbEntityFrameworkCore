using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonMatchFieldTests(ParadeDbFixture fixture) {
    // Verifies the 2-arg ParadeDbJsonQuery.Match(value, field) overload restricts
    // the match to the named field. The C# parameter order (value, field) is the
    // opposite of the resulting JSON key order ({"field":..., "value":...}); a
    // refactor that "aligns" param order with the JSON would silently invert
    // every caller. The 4-arg overload is the only currently-tested Match shape.
    [Fact]
    public async Task Match_WithFieldAndValue_RestrictsToNamedField() {
        await using var ctx = fixture.CreateDbContext();

        // "models" appears in Article 1 and Article 2 content (stems to "model"),
        // and in NO article title. A param swap would search field "models"
        // (non-existent) and error, or search title and return zero from content.
        var titleQuery = ParadeDbJsonQuery.Match("models", "title");
        var contentQuery = ParadeDbJsonQuery.Match("models", "content");

        var titleHits = await ctx.Articles
            .JsonSearch(a => a.Id, titleQuery)
            .Select(a => a.Title)
            .ToListAsync();
        var contentHits = await ctx.Articles
            .JsonSearch(a => a.Id, contentQuery)
            .Select(a => a.Title)
            .ToListAsync();

        // Title-restricted: zero hits — "models" doesn't appear in any title.
        Assert.Empty(titleHits);
        // Content-restricted: Articles 1 and 2 both have "models" in content.
        Assert.Contains(contentHits, t => t == "Introduction to neural networks");
        Assert.Contains(contentHits, t => t == "Transformer architectures");
    }
}
