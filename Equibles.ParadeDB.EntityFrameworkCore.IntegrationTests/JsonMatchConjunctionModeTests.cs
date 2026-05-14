using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonMatchConjunctionModeTests(ParadeDbFixture fixture)
{
    // Verifies ParadeDbJsonQuery.Match 4-arg actually toggles AND/OR semantics
    // via the conditional "conjunction_mode" key. The existing JsonMatchTests
    // uses "nueral netwroks" — both fuzzy-terms only co-occur in Article 1 — so
    // AND and OR return the same set and the conjunction_mode flag isn't
    // actually validated. This test uses "neural quantum" against the seed
    // where the two tokens live in disjoint articles, so flipping the flag MUST
    // change the hit set.
    [Fact]
    public async Task Match_WithConjunctionModeFalse_AppliesOrSemantics()
    {
        await using var ctx = fixture.CreateDbContext();

        // "neural" appears in Article 1's content; "pasta" in Article 4's content;
        // no single article's content has both — so OR and AND must differ.
        var orQuery = ParadeDbJsonQuery.Match(
            "neural pasta",
            "content",
            distance: 0,
            conjunctionMode: false
        );
        var andQuery = ParadeDbJsonQuery.Match(
            "neural pasta",
            "content",
            distance: 0,
            conjunctionMode: true
        );

        var orHits = await ctx
            .Articles.JsonSearch(a => a.Id, orQuery)
            .Select(a => a.Title)
            .ToListAsync();
        var andHits = await ctx
            .Articles.JsonSearch(a => a.Id, andQuery)
            .Select(a => a.Title)
            .ToListAsync();

        // OR semantics: both articles match — each contains exactly one of the terms.
        Assert.Contains(orHits, t => t == "Introduction to neural networks");
        Assert.Contains(orHits, t => t == "Cooking pasta perfectly");
        // AND semantics: neither article has both terms.
        Assert.DoesNotContain(andHits, t => t == "Introduction to neural networks");
        Assert.DoesNotContain(andHits, t => t == "Cooking pasta perfectly");
    }
}
