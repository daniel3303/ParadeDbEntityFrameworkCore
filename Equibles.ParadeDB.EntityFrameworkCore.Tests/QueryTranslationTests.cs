using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.Tests;

public class QueryTranslationTests : IDisposable {
    private readonly TestDbContext _db = new();

    public void Dispose() => _db.Dispose();

    private string Sql(IQueryable query) => query.ToQueryString();

    // ── Basic Search ──────────────────────────────────────────────────

    [Fact]
    public void Matches_generates_or_operator() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.Matches(a.Content, "shoes")));
        Assert.Contains("|||", sql);
    }

    [Fact]
    public void MatchesAll_generates_and_operator() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesAll(a.Content, "shoes")));
        Assert.Contains("&&&", sql);
    }

    // ── Phrase Search ─────────────────────────────────────────────────

    [Fact]
    public void MatchesPhrase_generates_phrase_operator() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesPhrase(a.Content, "neural networks")));
        Assert.Contains("###", sql);
    }

    [Fact]
    public void MatchesPhrase_with_slop_generates_slop_modifier() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesPhrase(a.Content, "neural networks", 2)));
        Assert.Contains("###", sql);
        Assert.Contains("::pdb.slop(2)", sql);
    }

    // ── Term Search ───────────────────────────────────────────────────

    [Fact]
    public void MatchesTerm_generates_term_operator() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesTerm(a.Content, "gpu")));
        Assert.Contains("===", sql);
    }

    [Fact]
    public void MatchesTermSet_generates_term_operator_with_array() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesTermSet(a.Content, "gpu", "tpu")));
        Assert.Contains("===", sql);
        Assert.Contains("ARRAY", sql);
    }

    // ── Fuzzy Search ──────────────────────────────────────────────────

    [Fact]
    public void MatchesFuzzy_generates_fuzzy_modifier() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesFuzzy(a.Content, "machin", 2)));
        Assert.Contains("|||", sql);
        Assert.Contains("::pdb.fuzzy(2)", sql);
    }

    [Fact]
    public void MatchesFuzzy_full_generates_fuzzy_with_options() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesFuzzy(a.Content, "machin", 2, true, false)));
        Assert.Contains("|||", sql);
        Assert.Contains("::pdb.fuzzy(2, true, false)", sql);
    }

    [Fact]
    public void MatchesAllFuzzy_generates_and_with_fuzzy() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesAllFuzzy(a.Content, "machin", 2)));
        Assert.Contains("&&&", sql);
        Assert.Contains("::pdb.fuzzy(2)", sql);
    }

    [Fact]
    public void MatchesAllFuzzy_full_generates_and_with_fuzzy_options() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesAllFuzzy(a.Content, "machin", 1, false, true)));
        Assert.Contains("&&&", sql);
        Assert.Contains("::pdb.fuzzy(1, false, true)", sql);
    }

    [Fact]
    public void MatchesTermFuzzy_generates_term_with_fuzzy() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesTermFuzzy(a.Content, "machin", 1)));
        Assert.Contains("===", sql);
        Assert.Contains("::pdb.fuzzy(1)", sql);
    }

    [Fact]
    public void MatchesTermFuzzy_full_generates_term_with_fuzzy_options() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesTermFuzzy(a.Content, "machin", 2, true, true)));
        Assert.Contains("===", sql);
        Assert.Contains("::pdb.fuzzy(2, true, true)", sql);
    }

    // ── Boost ─────────────────────────────────────────────────────────

    [Fact]
    public void MatchesBoosted_generates_boost_modifier() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesBoosted(a.Content, "shoes", 2.0)));
        Assert.Contains("|||", sql);
        Assert.Contains("::pdb.boost(2)", sql);
    }

    [Fact]
    public void MatchesAllBoosted_generates_and_with_boost() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesAllBoosted(a.Content, "shoes", 1.5)));
        Assert.Contains("&&&", sql);
        Assert.Contains("::pdb.boost(1.5)", sql);
    }

    // ── Fuzzy + Boost Combined ────────────────────────────────────────

    [Fact]
    public void MatchesFuzzyBoosted_generates_fuzzy_and_boost() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesFuzzyBoosted(a.Content, "shoes", 2, 2.0)));
        Assert.Contains("|||", sql);
        Assert.Contains("::pdb.fuzzy(2)::pdb.boost(2)", sql);
    }

    [Fact]
    public void MatchesAllFuzzyBoosted_generates_and_fuzzy_and_boost() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesAllFuzzyBoosted(a.Content, "shoes", 1, 3.0)));
        Assert.Contains("&&&", sql);
        Assert.Contains("::pdb.fuzzy(1)::pdb.boost(3)", sql);
    }

    // ── BM25 Scoring ──────────────────────────────────────────────────

    [Fact]
    public void Score_generates_pdb_score_function() {
        var sql = Sql(_db.Articles
            .Where(a => EF.Functions.Matches(a.Content, "test"))
            .Select(a => new { Score = EF.Functions.Score(a.Id) }));
        Assert.Contains("pdb.score(", sql);
    }

    // ── Snippets ──────────────────────────────────────────────────────

    [Fact]
    public void Snippet_generates_pdb_snippet_function() {
        var sql = Sql(_db.Articles
            .Where(a => EF.Functions.Matches(a.Content, "test"))
            .Select(a => new { Snip = EF.Functions.Snippet(a.Content) }));
        Assert.Contains("pdb.snippet(", sql);
    }

    [Fact]
    public void Snippet_with_params_generates_named_args() {
        var sql = Sql(_db.Articles
            .Where(a => EF.Functions.Matches(a.Content, "test"))
            .Select(a => new { Snip = EF.Functions.Snippet(a.Content, "<b>", "</b>", 100) }));
        Assert.Contains("pdb.snippet(", sql);
        Assert.Contains("start_tag =>", sql);
        Assert.Contains("end_tag =>", sql);
        Assert.Contains("max_num_chars =>", sql);
    }

    [Fact]
    public void Snippets_generates_named_args_with_quoted_limit_offset() {
        var sql = Sql(_db.Articles
            .Where(a => EF.Functions.Matches(a.Content, "test"))
            .Select(a => new { Snips = EF.Functions.Snippets(a.Content, 15, 5, 0) }));
        Assert.Contains("pdb.snippets(", sql);
        Assert.Contains("max_num_chars =>", sql);
        Assert.Contains("\"limit\" =>", sql);
        Assert.Contains("\"offset\" =>", sql);
    }

    // ── Parse Query ───────────────────────────────────────────────────

    [Fact]
    public void Parse_generates_parse_with_at_operator() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.Parse(a.Id, "title:shoes")));
        Assert.Contains("@@@", sql);
        Assert.Contains("pdb.parse(", sql);
    }

    [Fact]
    public void Parse_with_options_generates_named_args() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.Parse(a.Id, "shoes", true, true)));
        Assert.Contains("@@@", sql);
        Assert.Contains("pdb.parse(", sql);
        Assert.Contains("lenient =>", sql);
        Assert.Contains("conjunction_mode =>", sql);
    }

    // ── Regex Search ──────────────────────────────────────────────────

    [Fact]
    public void Regex_generates_regex_with_at_operator() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.Regex(a.Content, "neuro.*")));
        Assert.Contains("@@@", sql);
        Assert.Contains("pdb.regex(", sql);
    }

    // ── Phrase Prefix ─────────────────────────────────────────────────

    [Fact]
    public void PhrasePrefix_generates_phrase_prefix_with_array() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.PhrasePrefix(a.Content, "running", "sh")));
        Assert.Contains("@@@", sql);
        Assert.Contains("pdb.phrase_prefix(", sql);
    }

    [Fact]
    public void PhrasePrefix_with_max_expansions_generates_named_arg() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.PhrasePrefix(a.Content, 10, "running", "sh")));
        Assert.Contains("@@@", sql);
        Assert.Contains("pdb.phrase_prefix(", sql);
        Assert.Contains("max_expansions =>", sql);
    }

    // ── More Like This ────────────────────────────────────────────────

    [Fact]
    public void MoreLikeThis_generates_more_like_this_function() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MoreLikeThis(a.Id, 3)));
        Assert.Contains("@@@", sql);
        Assert.Contains("pdb.more_like_this(", sql);
    }

    [Fact]
    public void MoreLikeThis_with_fields_generates_array_arg() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MoreLikeThis(a.Id, 3, "description")));
        Assert.Contains("@@@", sql);
        Assert.Contains("pdb.more_like_this(", sql);
    }

    // ── JSON Query Search ──────────────────────────────────────────────

    [Fact]
    public void JsonSearch_generates_at_operator_with_pdb_query_cast() {
        var json = ParadeDbJsonQuery.Parse("revenue growth").ToJson();
        var sql = Sql(_db.Chunks.Where(c => EF.Functions.JsonSearch(c.Id, json)));
        Assert.Contains("@@@", sql);
        Assert.Contains("::pdb.query", sql);
    }

    [Fact]
    public void JsonSearch_boolean_query_generates_json_with_cast() {
        var query = ParadeDbJsonQuery.Boolean(b => b
            .Must(
                ParadeDbJsonQuery.Parse("revenue growth"),
                ParadeDbJsonQuery.Term("DocumentType", 10)));
        var sql = Sql(_db.Chunks.Where(c => EF.Functions.JsonSearch(c.Id, query.ToJson())));
        Assert.Contains("@@@", sql);
        Assert.Contains("::pdb.query", sql);
    }

    [Fact]
    public void JsonSearch_composes_with_order_by_score() {
        var json = ParadeDbJsonQuery.Parse("test").ToJson();
        var sql = Sql(_db.Chunks
            .Where(c => EF.Functions.JsonSearch(c.Id, json))
            .OrderByDescending(c => EF.Functions.Score(c.Id)));
        Assert.Contains("@@@", sql);
        Assert.Contains("::pdb.query", sql);
        Assert.Contains("pdb.score(", sql);
        Assert.Contains("ORDER BY", sql);
    }

    [Fact]
    public void JsonSearch_composes_with_take() {
        var json = ParadeDbJsonQuery.Parse("test").ToJson();
        var sql = Sql(_db.Chunks
            .Where(c => EF.Functions.JsonSearch(c.Id, json))
            .Take(5));
        Assert.Contains("@@@", sql);
        Assert.Contains("LIMIT", sql);
    }

    [Fact]
    public void JsonSearch_composes_with_standard_linq_where() {
        var json = ParadeDbJsonQuery.Parse("test").ToJson();
        var sql = Sql(_db.Chunks
            .Where(c => EF.Functions.JsonSearch(c.Id, json) && c.DocumentType > 5));
        Assert.Contains("@@@", sql);
        Assert.Contains("::pdb.query", sql);
        Assert.Contains(">", sql);
    }

    [Fact]
    public void JsonSearch_extension_generates_same_as_ef_functions() {
        var query = ParadeDbJsonQuery.Parse("test");
        var sqlExt = Sql(_db.Chunks.JsonSearch(c => c.Id, query));
        var sqlDirect = Sql(_db.Chunks.Where(c => EF.Functions.JsonSearch(c.Id, query.ToJson())));
        Assert.Contains("@@@", sqlExt);
        Assert.Contains("::pdb.query", sqlExt);
        Assert.Contains("@@@", sqlDirect);
    }

    // ── Combining with LINQ ───────────────────────────────────────────

    [Fact]
    public void Search_composes_with_standard_linq_where() {
        var sql = Sql(_db.Articles
            .Where(a => EF.Functions.Matches(a.Content, "test") && a.Id > 5));
        Assert.Contains("|||", sql);
        Assert.Contains(">", sql);
    }

    [Fact]
    public void Search_composes_with_order_by_score() {
        var sql = Sql(_db.Articles
            .Where(a => EF.Functions.Matches(a.Content, "test"))
            .OrderByDescending(a => EF.Functions.Score(a.Id)));
        Assert.Contains("|||", sql);
        Assert.Contains("pdb.score(", sql);
        Assert.Contains("ORDER BY", sql);
    }
}
