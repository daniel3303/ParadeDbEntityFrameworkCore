using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class IndexConfigurationTests(ParadeDbFixture fixture) {
    [Fact]
    public async Task PgSearchExtension_IsInstalled() {
        await using var conn = new NpgsqlConnection(fixture.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM pg_extension WHERE extname = 'pg_search'";

        var result = await cmd.ExecuteScalarAsync();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Bm25Index_IsCreatedWithStorageParameters() {
        await using var conn = new NpgsqlConnection(fixture.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT indexdef FROM pg_indexes
            WHERE tablename = 'articles' AND indexname LIKE 'IX_%'
            """;

        var indexDef = (string?)await cmd.ExecuteScalarAsync();

        Assert.NotNull(indexDef);
        Assert.Contains("USING bm25", indexDef);
        Assert.Contains("key_field=", indexDef);
        Assert.Contains("text_fields=", indexDef);
        Assert.Contains("numeric_fields=", indexDef);
    }

    [Fact]
    public async Task EnglishStemmer_MatchesWordVariants() {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx.Articles
            .Where(a => EF.Functions.Matches(a.Content, "run"))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.Contains(hits, t => t == "Transformer architectures");
    }

    [Fact]
    public async Task RawTokenizer_KeepsExactCategoryValue() {
        await using var ctx = fixture.CreateDbContext();

        var exactHits = await ctx.Articles
            .Where(a => EF.Functions.MatchesTerm(a.Category, "machine-learning"))
            .CountAsync();
        var tokenizedAttempt = await ctx.Articles
            .Where(a => EF.Functions.MatchesTerm(a.Category, "machine"))
            .CountAsync();

        Assert.Equal(2, exactHits);
        Assert.Equal(0, tokenizedAttempt);
    }
}
