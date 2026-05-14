using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class PhrasePrefixMaxExpansionsTests(ParadeDbFixture fixture)
{
    // Verifies the 3-arg PhrasePrefix overload (with maxExpansions) translates to
    // pdb.phrase_prefix(ARRAY[...], max_expansion => N) and runs. Guards against drift
    // back to the plural "max_expansions" — pg_search's named arg is singular.
    [Fact]
    public async Task PhrasePrefix_WithMaxExpansions_MatchesContainingDocument()
    {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx
            .Articles.Where(a => EF.Functions.PhrasePrefix(a.Content, 5, "neural", "net"))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.DoesNotContain(hits, t => t == "Cooking pasta perfectly");
    }
}
