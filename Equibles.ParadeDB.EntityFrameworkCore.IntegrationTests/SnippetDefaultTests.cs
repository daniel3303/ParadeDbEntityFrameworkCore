using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class SnippetDefaultTests(ParadeDbFixture fixture)
{
    // Verifies the no-arg Snippet overload translates to bare pdb.snippet(column) — distinct
    // from the parameterised pdb.snippet(column, start_tag => ..., end_tag => ..., max_num_chars => ...)
    // form that's already tested. Default ParadeDB tags are <b>...</b>. A regression that
    // routed through the named-arg path would either fail or change the highlight markers.
    [Fact]
    public async Task Snippet_NoArgs_HighlightsWithDefaultBoldTags()
    {
        await using var ctx = fixture.CreateDbContext();

        var snippets = await ctx
            .Articles.Where(a => EF.Functions.Matches(a.Content, "neural networks"))
            .Select(a => EF.Functions.Snippet(a.Content))
            .ToListAsync();

        Assert.NotEmpty(snippets);
        Assert.Contains(snippets, s => s != null && s.Contains("<b>") && s.Contains("</b>"));
    }
}
