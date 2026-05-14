using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonSearchWithSnippetTests(ParadeDbFixture fixture)
{
    // Verifies pdb.snippet() highlights the matched terms when the predicate came from the
    // JSON-DSL @@@ jsonb path rather than the direct |||/&&& operators. All existing snippet
    // tests use EF.Functions.Matches; a regression in how the JSON @@@ predicate registers
    // the matched row with the snippet function would slip past those.
    [Fact]
    public async Task JsonSearch_WithSnippetProjection_HighlightsMatchedTerms()
    {
        await using var ctx = fixture.CreateDbContext();

        var query = ParadeDbJsonQuery.Boolean(b =>
            b.Must(ParadeDbJsonQuery.Match("neural", "content"))
        );

        var snippets = await ctx
            .Articles.JsonSearch(a => a.Id, query)
            .Select(a => EF.Functions.Snippet(a.Content, "<mark>", "</mark>", 100))
            .ToListAsync();

        Assert.NotEmpty(snippets);
        Assert.Contains(snippets, s => s != null && s.Contains("<mark>") && s.Contains("</mark>"));
    }
}
