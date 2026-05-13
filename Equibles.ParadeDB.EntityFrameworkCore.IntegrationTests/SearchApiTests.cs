using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class SearchApiTests(ParadeDbFixture fixture) {
    [Fact]
    public async Task MatchesBoosted_RaisesScoreOfBoostedTerm() {
        await using var ctx = fixture.CreateDbContext();

        var withBoost = await ctx.Articles
            .Where(a => EF.Functions.MatchesBoosted(a.Content, "neural", 5.0))
            .Select(a => new { a.Title, Score = EF.Functions.Score(a.Id) })
            .OrderByDescending(a => a.Score)
            .ToListAsync();

        Assert.NotEmpty(withBoost);
        Assert.True(withBoost[0].Score > 1.0,
            $"Boosted score should be amplified above default 1.0; was {withBoost[0].Score}.");
    }

    [Fact]
    public async Task MatchesTermSet_MatchesAnyToken() {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx.Articles
            .Where(a => EF.Functions.MatchesTermSet(a.Content, "gpus", "tpus"))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
        Assert.Contains(hits, t => t == "Transformer architectures");
    }

    [Fact]
    public async Task MatchesPhraseWithSlop_AllowsWordsBetweenTerms() {
        await using var ctx = fixture.CreateDbContext();

        var exact = await ctx.Articles
            .Where(a => EF.Functions.MatchesPhrase(a.Content, "deep models"))
            .CountAsync();
        var withSlop = await ctx.Articles
            .Where(a => EF.Functions.MatchesPhrase(a.Content, "deep models", 2))
            .CountAsync();

        Assert.Equal(0, exact);
        Assert.True(withSlop >= 1, "Slop 2 should match 'Deep learning models'.");
    }

    [Fact]
    public async Task MatchesAllFuzzy_AndOperatorToleratesTypos() {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx.Articles
            .Where(a => EF.Functions.MatchesAllFuzzy(a.Content, "nueral netwroks", 2))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
    }

    [Fact]
    public async Task MatchesTermFuzzy_SingleTermToleratesTypos() {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx.Articles
            .Where(a => EF.Functions.MatchesTermFuzzy(a.Content, "neurla", 2))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
    }

    [Fact]
    public async Task Snippets_ReturnsHighlightedExcerptArray() {
        await using var ctx = fixture.CreateDbContext();

        var snippets = await ctx.Articles
            .Where(a => EF.Functions.Matches(a.Content, "neural"))
            .Select(a => EF.Functions.Snippets(a.Content, 80, 3, 0))
            .ToListAsync();

        Assert.NotEmpty(snippets);
        Assert.Contains(snippets, arr => arr.Any(s => s.Contains("<b>") && s.Contains("</b>")));
    }

    [Fact]
    public async Task Regex_FindsTokensMatchingPattern() {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx.Articles
            .Where(a => EF.Functions.Regex(a.Content, "neur.*"))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
    }

    [Fact]
    public async Task PhrasePrefix_MatchesPartialLastWord() {
        await using var ctx = fixture.CreateDbContext();

        var hits = await ctx.Articles
            .Where(a => EF.Functions.PhrasePrefix(a.Content, "neural", "net"))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
    }

    [Fact]
    public async Task Parse_LenientMode_IgnoresMalformedSyntax() {
        await using var ctx = fixture.CreateDbContext();

        // Trailing operator would normally throw; lenient + conjunctionMode lets it parse.
        var hits = await ctx.Articles
            .Where(a => EF.Functions.Parse(a.Id, "neural networks", lenient: true, conjunctionMode: true))
            .Select(a => a.Title)
            .ToListAsync();

        Assert.Contains(hits, t => t == "Introduction to neural networks");
    }

    [Fact]
    public async Task ComposedLinq_SearchPlusWhereAndProjection_Works() {
        await using var ctx = fixture.CreateDbContext();

        var top = await ctx.Articles
            .Where(a => EF.Functions.Matches(a.Content, "neural networks") && a.Rating >= 4)
            .Select(a => new {
                a.Title,
                Snippet = EF.Functions.Snippet(a.Content, "<mark>", "</mark>", 60),
                Score = EF.Functions.Score(a.Id),
            })
            .OrderByDescending(x => x.Score)
            .Take(5)
            .ToListAsync();

        Assert.NotEmpty(top);
        Assert.All(top, x => Assert.False(string.IsNullOrEmpty(x.Title)));
        Assert.All(top, x => Assert.True(x.Score > 0));
    }
}
