using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonTermSetNumericTests(ParadeDbFixture fixture)
{
    // Verifies ParadeDbJsonQuery.TermSet against an int field uses the int branch of
    // CreateJsonValue (emits a JSON number, not a string). JsonTermSetTests only covers
    // string terms — a regression where CreateJsonValue's int branch fell through to the
    // ToString() fallback would emit "4" instead of 4, and pg_search would reject the
    // numeric_fields term_set or silently miss matches. Rating is an int Bm25Numeric.
    [Fact]
    public async Task TermSet_NumericRatings_MatchesAnyListedValue()
    {
        await using var ctx = fixture.CreateDbContext();

        var query = ParadeDbJsonQuery.TermSet("rating", 4, 5);

        var hits = await ctx
            .Articles.JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.Contains(hits, t => t == "Transformer architectures");
        Assert.Contains(hits, t => t == "Cooking pasta perfectly");
        Assert.DoesNotContain(hits, t => t == "Quantum computing fundamentals");
    }
}
