using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonParseOptionsTests(ParadeDbFixture fixture)
{
    // Verifies ParadeDbJsonQuery.Parse(string, bool, bool) emits the JSON keys
    // "lenient" and "conjunction_mode" — distinct from the 1-arg overload that
    // emits only "query_string". The CLR sibling EF.Functions.Parse(...) is
    // tested separately; the JSON path is what feeds @@@ 'jsonb' and a
    // regression renaming either key (e.g. "lenient" → "strict") would silently
    // flip the AND/OR meaning of every unquoted multi-term parse query.
    [Fact]
    public async Task Parse_WithConjunctionMode_RequiresAllTermsInsteadOfAny()
    {
        await using var ctx = fixture.CreateDbContext();

        // "neural" appears only in Article 1; "quantum" only in Article 3 — no article has both.
        var orQuery = ParadeDbJsonQuery.Parse(
            "neural quantum",
            lenient: true,
            conjunctionMode: false
        );
        var andQuery = ParadeDbJsonQuery.Parse(
            "neural quantum",
            lenient: true,
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

        // OR semantics (conjunction_mode omitted): both articles match.
        Assert.Contains(orHits, t => t == "Introduction to neural networks");
        Assert.Contains(orHits, t => t == "Quantum computing fundamentals");
        // AND semantics (conjunction_mode true): neither article has both terms.
        Assert.DoesNotContain(andHits, t => t == "Introduction to neural networks");
        Assert.DoesNotContain(andHits, t => t == "Quantum computing fundamentals");
    }
}
