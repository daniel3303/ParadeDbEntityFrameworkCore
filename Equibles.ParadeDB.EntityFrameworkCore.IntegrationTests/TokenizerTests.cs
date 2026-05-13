using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class TokenizerTests(ParadeDbFixture fixture) {
    [Fact]
    public async Task KeywordTokenizer_TreatsCodeAsSingleToken() {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx.KeywordRecords
            .Where(k => EF.Functions.MatchesTerm(k.Code, "ABC-123"))
            .Select(k => k.Code)
            .ToListAsync();

        Assert.Single(hits);
        Assert.Equal("ABC-123", hits[0]);
    }

    [Fact]
    public async Task NgramTokenizer_MatchesSubstring() {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx.NgramRecords
            .Where(n => EF.Functions.MatchesTerm(n.Body, "frag"))
            .Select(n => n.Body)
            .ToListAsync();

        Assert.Contains(hits, b => b == "supercalifragilistic");
    }

    [Fact]
    public async Task IcuTokenizer_HandlesUnicodeText() {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx.IcuRecords
            .Where(i => EF.Functions.Matches(i.Body, "café"))
            .Select(i => i.Body)
            .ToListAsync();

        Assert.Contains(hits, b => b.Contains("Café"));
    }

    [Fact]
    public async Task SourceCodeTokenizer_SplitsCamelCase() {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx.SourceCodeRecords
            .Where(s => EF.Functions.Matches(s.Snippet, "user"))
            .Select(s => s.Snippet)
            .ToListAsync();

        Assert.Contains(hits, s => s == "GetUserById");
    }

    [Fact]
    public async Task RegexTokenizer_TokenizesByPattern() {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx.RegexRecords
            .Where(r => EF.Functions.Matches(r.Body, "alpha"))
            .Select(r => r.Body)
            .ToListAsync();

        Assert.Contains(hits, b => b == "alpha beta gamma");
    }

    [Fact]
    public async Task GermanStemmer_MatchesGermanWordVariants() {
        await using var ctx = fixture.CreateDbContext();

        // 'Häuser' stems to a German root; 'Haus' should match via the same stem.
        var hits = await ctx.GermanArticles
            .Where(g => EF.Functions.Matches(g.Content, "Haus"))
            .Select(g => g.Content)
            .ToListAsync();

        Assert.Contains(hits, c => c.Contains("Häuser"));
    }
}
