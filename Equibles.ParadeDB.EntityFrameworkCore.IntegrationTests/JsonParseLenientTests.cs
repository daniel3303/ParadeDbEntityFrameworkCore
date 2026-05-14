using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class JsonParseLenientTests(ParadeDbFixture fixture) {
    // Verifies ParadeDbJsonQuery.Parse's "lenient" JSON-key emission actually
    // takes effect — the existing JsonParseOptionsTests pins lenient: true in
    // both halves to isolate conjunction_mode, so the lenient branch is never
    // contrasted end-to-end. Uses a malformed query string ("neural AND")
    // that pg_search rejects in strict mode and tolerates in lenient mode —
    // a regression dropping the "lenient" key would let the strict case parse.
    [Fact]
    public async Task Parse_WithLenientFalse_RejectsMalformedQueryThatLenientTrueAccepts() {
        await using var ctx = fixture.CreateDbContext();

        // Trailing AND with no right-hand operand — syntactically invalid.
        var lenientQuery = ParadeDbJsonQuery.Parse("neural AND",
            lenient: true, conjunctionMode: false);
        var strictQuery = ParadeDbJsonQuery.Parse("neural AND",
            lenient: false, conjunctionMode: false);

        // Lenient: parser ignores the trailing operator and matches "neural" → Article 1.
        var lenientHits = await ctx.Articles
            .JsonSearch(a => a.Id, lenientQuery)
            .Select(a => a.Title)
            .ToListAsync();
        Assert.Contains(lenientHits, t => t == "Introduction to neural networks");

        // Strict: pg_search rejects the malformed query at execution time.
        await Assert.ThrowsAsync<PostgresException>(async () =>
            await ctx.Articles
                .JsonSearch(a => a.Id, strictQuery)
                .Select(a => a.Title)
                .ToListAsync());
    }
}
