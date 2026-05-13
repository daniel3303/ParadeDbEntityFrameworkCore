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
    public void MatchesFuzzy_full_generates_pdb_match_with_options() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesFuzzy(a.Content, "machin", 2, true, false)));
        Assert.Contains("@@@", sql);
        Assert.Contains("pdb.match(", sql);
        Assert.Contains("distance =>", sql);
        Assert.Contains("transposition_cost_one =>", sql);
        Assert.Contains("prefix =>", sql);
        Assert.DoesNotContain("conjunction_mode =>", sql);
    }

    [Fact]
    public void MatchesAllFuzzy_generates_and_with_fuzzy() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesAllFuzzy(a.Content, "machin", 2)));
        Assert.Contains("&&&", sql);
        Assert.Contains("::pdb.fuzzy(2)", sql);
    }

    [Fact]
    public void MatchesAllFuzzy_full_generates_pdb_match_with_conjunction() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesAllFuzzy(a.Content, "machin", 1, false, true)));
        Assert.Contains("@@@", sql);
        Assert.Contains("pdb.match(", sql);
        Assert.Contains("conjunction_mode =>", sql);
    }

    [Fact]
    public void MatchesTermFuzzy_generates_term_with_fuzzy() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesTermFuzzy(a.Content, "machin", 1)));
        Assert.Contains("===", sql);
        Assert.Contains("::pdb.fuzzy(1)", sql);
    }

    [Fact]
    public void MatchesTermFuzzy_full_generates_pdb_fuzzy_term_function() {
        var sql = Sql(_db.Articles.Where(a => EF.Functions.MatchesTermFuzzy(a.Content, "machin", 2, true, true)));
        Assert.Contains("@@@", sql);
        Assert.Contains("pdb.fuzzy_term(", sql);
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
        Assert.Contains("max_expansion =>", sql);
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
        Assert.Contains("::jsonb", sql);
    }

    [Fact]
    public void JsonSearch_boolean_query_generates_json_with_cast() {
        var query = ParadeDbJsonQuery.Boolean(b => b
            .Must(
                ParadeDbJsonQuery.Parse("revenue growth"),
                ParadeDbJsonQuery.Term("DocumentType", 10)));
        var sql = Sql(_db.Chunks.Where(c => EF.Functions.JsonSearch(c.Id, query.ToJson())));
        Assert.Contains("@@@", sql);
        Assert.Contains("::jsonb", sql);
    }

    [Fact]
    public void JsonSearch_composes_with_order_by_score() {
        var json = ParadeDbJsonQuery.Parse("test").ToJson();
        var sql = Sql(_db.Chunks
            .Where(c => EF.Functions.JsonSearch(c.Id, json))
            .OrderByDescending(c => EF.Functions.Score(c.Id)));
        Assert.Contains("@@@", sql);
        Assert.Contains("::jsonb", sql);
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
        Assert.Contains("::jsonb", sql);
        Assert.Contains(">", sql);
    }

    [Fact]
    public void JsonSearch_extension_generates_same_as_ef_functions() {
        var query = ParadeDbJsonQuery.Parse("test");
        var sqlExt = Sql(_db.Chunks.JsonSearch(c => c.Id, query));
        var sqlDirect = Sql(_db.Chunks.Where(c => EF.Functions.JsonSearch(c.Id, query.ToJson())));
        Assert.Contains("@@@", sqlExt);
        Assert.Contains("::jsonb", sqlExt);
        Assert.Contains("@@@", sqlDirect);
    }

    [Fact]
    public void JsonSearch_inline_boolean_generates_correct_sql() {
        var sql = Sql(_db.Chunks.JsonSearch(c => c.Id, b => b
            .Must(
                ParadeDbJsonQuery.Parse("revenue growth"),
                ParadeDbJsonQuery.Term("DocumentType", 10))));
        Assert.Contains("@@@", sql);
        Assert.Contains("::jsonb", sql);
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

    // ── Score Ordering Extensions ─────────────────────────────────────

    [Fact]
    public void OrderByScoreDescending_extension_emits_pdb_score_with_desc() {
        var sql = Sql(_db.Chunks
            .JsonSearch(c => c.Id, ParadeDbJsonQuery.Parse("test"))
            .OrderByScoreDescending(c => c.Id));
        Assert.Contains("pdb.score(", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("DESC", sql);
    }

    [Fact]
    public void OrderByScore_extension_emits_pdb_score_without_desc() {
        var sql = Sql(_db.Chunks
            .JsonSearch(c => c.Id, ParadeDbJsonQuery.Parse("test"))
            .OrderByScore(c => c.Id));
        Assert.Contains("pdb.score(", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.DoesNotContain("DESC", sql);
    }

    // ── Translator fall-through ───────────────────────────────────────

    /// <summary>
    /// Forces <see cref="ParadeDbMethodCallTranslator.Translate"/> down its fall-through
    /// path: a non-ParadeDB method call (string.StartsWith) is offered to every translator
    /// plugin, including ours — ours should return null so Npgsql's translator handles it.
    /// </summary>
    [Fact]
    public void NonParadeDbMethod_is_translated_by_other_plugins() {
        var sql = Sql(_db.Articles.Where(a => a.Title.StartsWith("foo")));
        Assert.DoesNotContain("@@@", sql);
        Assert.DoesNotContain("|||", sql);
        Assert.DoesNotContain("&&&", sql);
        Assert.DoesNotContain("###", sql);
        Assert.DoesNotContain("===", sql);
    }

    // ── Nullability processor mutation path ───────────────────────────

    /// <summary>
    /// Snippet uses a <see cref="ParadeDbNamedArgFunctionExpression"/>. With captured
    /// (parameterized) tag/limit/offset arguments, the SqlNullabilityProcessor visits
    /// every named arg, and the parameter visits typically yield different SqlExpression
    /// instances — exercising the "args changed → build new expression" branch.
    /// </summary>
    [Fact]
    public void Snippet_with_captured_parameters_still_emits_named_args() {
        var startTag = "<b>";
        var endTag = "</b>";
        var maxChars = 100;
        var sql = Sql(_db.Articles
            .Where(a => EF.Functions.Matches(a.Content, "test"))
            .Select(a => new { Snip = EF.Functions.Snippet(a.Content, startTag, endTag, maxChars) }));
        Assert.Contains("pdb.snippet(", sql);
        Assert.Contains("start_tag =>", sql);
        Assert.Contains("end_tag =>", sql);
        Assert.Contains("max_num_chars =>", sql);
    }

    [Fact]
    public void Snippets_with_captured_parameters_still_emits_named_args() {
        var maxChars = 15;
        var limit = 5;
        var offset = 0;
        var sql = Sql(_db.Articles
            .Where(a => EF.Functions.Matches(a.Content, "test"))
            .Select(a => new { Snips = EF.Functions.Snippets(a.Content, maxChars, limit, offset) }));
        Assert.Contains("pdb.snippets(", sql);
        Assert.Contains("max_num_chars =>", sql);
        Assert.Contains("\"limit\" =>", sql);
        Assert.Contains("\"offset\" =>", sql);
    }

    // ── Compile-time-constant guard ────────────────────────────────────

    /// <summary>
    /// Modifier params (distance, slop, boost) must be compile-time constants; the translator
    /// rejects captured parameters because it needs the literal value to bake into the SQL
    /// suffix (e.g. <c>::pdb.fuzzy(2)</c>). Captured int → SqlParameterExpression → throws.
    /// </summary>
    [Fact]
    public void MatchesFuzzy_with_captured_distance_throws_at_translation() {
        var distance = 2;
        var ex = Assert.Throws<InvalidOperationException>(
            () => Sql(_db.Articles.Where(a => EF.Functions.MatchesFuzzy(a.Content, "x", distance))));
        Assert.Contains("compile-time constants", ex.Message);
    }

    [Fact]
    public void MatchesBoosted_with_captured_boost_throws_at_translation() {
        var boost = 2.0;
        var ex = Assert.Throws<InvalidOperationException>(
            () => Sql(_db.Articles.Where(a => EF.Functions.MatchesBoosted(a.Content, "x", boost))));
        Assert.Contains("compile-time constants", ex.Message);
    }

    // ── JsonSearch with a reference-type key ───────────────────────────

    /// <summary>
    /// When the key selector resolves to a reference-typed property the lambda body is a
    /// plain MemberAccess — not a Convert — so <c>StripConvert</c> and <c>BoxIfNeeded</c>
    /// take their pass-through branches. The int-keyed tests don't exercise either path.
    /// </summary>
    [Fact]
    public void JsonSearch_with_reference_type_key_emits_at_operator() {
        var sql = Sql(_db.Chunks.JsonSearch(c => c.Content, ParadeDbJsonQuery.Parse("revenue")));
        Assert.Contains("@@@", sql);
        Assert.Contains("::jsonb", sql);
    }
}
