using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonRegexTests(ParadeDbFixture fixture) {
    // Verifies ParadeDbJsonQuery.Regex builds {"regex":{"field":"...","pattern":"..."}} and
    // pg_search accepts it via the @@@ jsonb path. Distinct from the CLR Regex translation
    // (which uses pdb.regex(...)). A regression in the "regex"/"field"/"pattern" key names
    // would either fail to deserialize or return zero matches.
    [Fact]
    public async Task Regex_Pattern_MatchesIndexedTokensMatchingExpression() {
        await using var ctx = fixture.CreateDbContext();

        var query = ParadeDbJsonQuery.Regex("content", "neur.*");

        var hits = await ctx.Articles
            .JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.DoesNotContain(hits, t => t == "Cooking pasta perfectly");
    }
}
