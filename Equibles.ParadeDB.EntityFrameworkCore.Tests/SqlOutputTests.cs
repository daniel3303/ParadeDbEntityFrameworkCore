using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.Tests;

/// <summary>
/// Prints generated SQL for visual inspection. Not assertions — just output.
/// </summary>
public class SqlOutputTests : IDisposable {
    private readonly TestDbContext _db = new();

    public void Dispose() => _db.Dispose();

    private string Sql(IQueryable query) => query.ToQueryString();

    private IQueryable<Chunk> JsonSearchQuery() {
        var json = ParadeDbJsonQuery.Parse("revenue growth").ToJson();
        return _db.Chunks.Where(c => EF.Functions.JsonSearch(c.Id, json));
    }

    private IQueryable<Chunk> JsonSearchBooleanQuery() {
        var json = ParadeDbJsonQuery.Boolean(b => b
            .Must(
                ParadeDbJsonQuery.Parse("revenue growth"),
                ParadeDbJsonQuery.Term("DocumentType", 10))).ToJson();
        return _db.Chunks.Where(c => EF.Functions.JsonSearch(c.Id, json));
    }

    [Fact]
    public void Print_all_query_translations() {
        var queries = new (string Label, string Sql)[] {
            ("Matches", Sql(_db.Articles.Where(a => EF.Functions.Matches(a.Content, "shoes")))),
            ("MatchesAll", Sql(_db.Articles.Where(a => EF.Functions.MatchesAll(a.Content, "shoes")))),
            ("MatchesPhrase", Sql(_db.Articles.Where(a => EF.Functions.MatchesPhrase(a.Content, "neural networks")))),
            ("MatchesPhrase+slop", Sql(_db.Articles.Where(a => EF.Functions.MatchesPhrase(a.Content, "neural networks", 2)))),
            ("MatchesTerm", Sql(_db.Articles.Where(a => EF.Functions.MatchesTerm(a.Content, "gpu")))),
            ("MatchesTermSet", Sql(_db.Articles.Where(a => EF.Functions.MatchesTermSet(a.Content, "gpu", "tpu")))),
            ("MatchesFuzzy", Sql(_db.Articles.Where(a => EF.Functions.MatchesFuzzy(a.Content, "machin", 2)))),
            ("MatchesFuzzy+opts", Sql(_db.Articles.Where(a => EF.Functions.MatchesFuzzy(a.Content, "machin", 2, true, false)))),
            ("MatchesAllFuzzy", Sql(_db.Articles.Where(a => EF.Functions.MatchesAllFuzzy(a.Content, "machin", 2)))),
            ("MatchesTermFuzzy", Sql(_db.Articles.Where(a => EF.Functions.MatchesTermFuzzy(a.Content, "machin", 1)))),
            ("MatchesBoosted", Sql(_db.Articles.Where(a => EF.Functions.MatchesBoosted(a.Content, "shoes", 2.0)))),
            ("MatchesAllBoosted", Sql(_db.Articles.Where(a => EF.Functions.MatchesAllBoosted(a.Content, "shoes", 1.5)))),
            ("MatchesFuzzyBoosted", Sql(_db.Articles.Where(a => EF.Functions.MatchesFuzzyBoosted(a.Content, "shoes", 2, 2.0)))),
            ("MatchesAllFuzzyBoosted", Sql(_db.Articles.Where(a => EF.Functions.MatchesAllFuzzyBoosted(a.Content, "shoes", 1, 3.0)))),
            ("Score", Sql(_db.Articles.Where(a => EF.Functions.Matches(a.Content, "test")).Select(a => new { Score = EF.Functions.Score(a.Id) }))),
            ("Snippet", Sql(_db.Articles.Where(a => EF.Functions.Matches(a.Content, "test")).Select(a => new { Snip = EF.Functions.Snippet(a.Content) }))),
            ("Snippet+params", Sql(_db.Articles.Where(a => EF.Functions.Matches(a.Content, "test")).Select(a => new { Snip = EF.Functions.Snippet(a.Content, "<b>", "</b>", 100) }))),
            ("Snippets", Sql(_db.Articles.Where(a => EF.Functions.Matches(a.Content, "test")).Select(a => new { Snips = EF.Functions.Snippets(a.Content, 15, 5, 0) }))),
            ("Parse", Sql(_db.Articles.Where(a => EF.Functions.Parse(a.Id, "title:shoes")))),
            ("Parse+opts", Sql(_db.Articles.Where(a => EF.Functions.Parse(a.Id, "shoes", true, true)))),
            ("Regex", Sql(_db.Articles.Where(a => EF.Functions.Regex(a.Content, "neuro.*")))),
            ("PhrasePrefix", Sql(_db.Articles.Where(a => EF.Functions.PhrasePrefix(a.Content, "running", "sh")))),
            ("PhrasePrefix+max", Sql(_db.Articles.Where(a => EF.Functions.PhrasePrefix(a.Content, 10, "running", "sh")))),
            ("MoreLikeThis", Sql(_db.Articles.Where(a => EF.Functions.MoreLikeThis(a.Id, 3)))),
            ("MoreLikeThis+fields", Sql(_db.Articles.Where(a => EF.Functions.MoreLikeThis(a.Id, 3, "description")))),
            ("JsonSearch", Sql(JsonSearchQuery())),
            ("JsonSearch+boolean", Sql(JsonSearchBooleanQuery())),
            ("JsonSearch+extension", Sql(_db.Chunks.JsonSearch(c => c.Id, ParadeDbJsonQuery.Parse("test")))),
            ("JsonSearch+score+limit", Sql(_db.Chunks
                .JsonSearch(c => c.Id, ParadeDbJsonQuery.Parse("test"))
                .OrderByScoreDescending(c => c.Id)
                .Take(5))),
        };

        foreach (var (label, sql) in queries) {
            Console.WriteLine($"── {label} ──");
            Console.WriteLine(sql);
            Console.WriteLine();
        }
    }
}
