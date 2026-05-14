using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonFuzzyTermTests(ParadeDbFixture fixture)
{
    // Verifies ParadeDbJsonQuery.FuzzyTerm builds {"fuzzy_term":{"field":"...","value":"...","distance":N}}
    // and pg_search accepts it via the @@@ jsonb path. Distinct from the CLR MatchesTermFuzzy
    // translation (which goes through pdb.fuzzy(...) casts). A regression in the JSON keys
    // ("fuzzy_term"/"field"/"value"/"distance") would either fail to deserialize or zero matches.
    [Fact]
    public async Task FuzzyTerm_WithDistance2_MatchesTypoedToken()
    {
        await using var ctx = fixture.CreateDbContext();

        // "nueral" → "neural" is a Levenshtein distance of 2; distance: 2 should match.
        var query = ParadeDbJsonQuery.FuzzyTerm("content", "nueral", 2);

        var hits = await ctx
            .Articles.JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.DoesNotContain(hits, t => t == "Cooking pasta perfectly");
    }
}
