using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonMatchTests(ParadeDbFixture fixture) {
    // Verifies ParadeDbJsonQuery.Match 4-arg overload emits the full options shape:
    // {"match":{"field":"...","value":"...","distance":N,"conjunction_mode":true}}
    // — combines fuzzy matching with AND semantics in one JSON node. A regression in
    // any of the four keys, or the conditional emission of conjunction_mode, would
    // either fail to deserialize or change the match set.
    [Fact]
    public async Task Match_WithDistanceAndConjunctionMode_MatchesAllFuzzyTerms() {
        await using var ctx = fixture.CreateDbContext();

        // "nueral" and "netwroks" both typo'd; distance=2 + conjunction=true means
        // every term must fuzzy-match. Article 1's content has both "neural" and "networks".
        var query = ParadeDbJsonQuery.Match("nueral netwroks", "content", 2, conjunctionMode: true);

        var hits = await ctx.Articles
            .JsonSearch(a => a.Id, query)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.DoesNotContain(hits, t => t == "Cooking pasta perfectly");
        Assert.DoesNotContain(hits, t => t == "Quantum computing fundamentals");
    }
}
