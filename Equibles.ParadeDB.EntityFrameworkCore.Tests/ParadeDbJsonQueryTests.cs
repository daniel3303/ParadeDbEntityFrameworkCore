using System.Text.Json;

namespace Equibles.ParadeDB.EntityFrameworkCore.Tests;

/// <summary>
/// Pure CLR tests validating JSON output from <see cref="ParadeDbJsonQuery"/> factory methods.
/// </summary>
public class ParadeDbJsonQueryTests {
    private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

    // ── Parse ────────────────────────────────────────────────────────

    [Fact]
    public void Parse_creates_correct_json() {
        var json = ParadeDbJsonQuery.Parse("revenue growth").ToJson();
        var doc = Parse(json);
        Assert.Equal("revenue growth", doc.GetProperty("parse").GetProperty("query_string").GetString());
    }

    [Fact]
    public void Parse_with_options_creates_correct_json() {
        var json = ParadeDbJsonQuery.Parse("revenue growth", true, true).ToJson();
        var doc = Parse(json);
        var parse = doc.GetProperty("parse");
        Assert.Equal("revenue growth", parse.GetProperty("query_string").GetString());
        Assert.True(parse.GetProperty("lenient").GetBoolean());
        Assert.True(parse.GetProperty("conjunction_mode").GetBoolean());
    }

    // ── Term ─────────────────────────────────────────────────────────

    [Fact]
    public void Term_with_string_value_creates_correct_json() {
        var json = ParadeDbJsonQuery.Term("DocumentId", "abc-123").ToJson();
        var doc = Parse(json);
        var term = doc.GetProperty("term");
        Assert.Equal("DocumentId", term.GetProperty("field").GetString());
        Assert.Equal("abc-123", term.GetProperty("value").GetString());
    }

    [Fact]
    public void Term_with_int_value_creates_correct_json() {
        var json = ParadeDbJsonQuery.Term("DocumentType", 10).ToJson();
        var doc = Parse(json);
        var term = doc.GetProperty("term");
        Assert.Equal("DocumentType", term.GetProperty("field").GetString());
        Assert.Equal(10, term.GetProperty("value").GetInt32());
    }

    [Fact]
    public void Term_with_guid_value_creates_correct_json() {
        var guid = Guid.Parse("1d56ce60-1234-5678-9abc-def012345678");
        var json = ParadeDbJsonQuery.Term("DocumentId", guid).ToJson();
        var doc = Parse(json);
        Assert.Equal(guid.ToString(), doc.GetProperty("term").GetProperty("value").GetString());
    }

    // ── Term Set ─────────────────────────────────────────────────────

    [Fact]
    public void TermSet_creates_correct_json() {
        var json = ParadeDbJsonQuery.TermSet("Tags", "gpu", "tpu").ToJson();
        var doc = Parse(json);
        var termSet = doc.GetProperty("term_set");
        Assert.Equal("Tags", termSet.GetProperty("field").GetString());
        var terms = termSet.GetProperty("terms");
        Assert.Equal(2, terms.GetArrayLength());
        Assert.Equal("gpu", terms[0].GetString());
        Assert.Equal("tpu", terms[1].GetString());
    }

    // ── Match ────────────────────────────────────────────────────────

    [Fact]
    public void Match_creates_correct_json() {
        var json = ParadeDbJsonQuery.Match("shoes").ToJson();
        var doc = Parse(json);
        Assert.Equal("shoes", doc.GetProperty("match").GetProperty("value").GetString());
    }

    [Fact]
    public void Match_with_field_and_options_creates_correct_json() {
        var json = ParadeDbJsonQuery.Match("shoes", "Content", 2, true).ToJson();
        var doc = Parse(json);
        var match = doc.GetProperty("match");
        Assert.Equal("Content", match.GetProperty("field").GetString());
        Assert.Equal("shoes", match.GetProperty("value").GetString());
        Assert.Equal(2, match.GetProperty("distance").GetInt32());
        Assert.True(match.GetProperty("conjunction_mode").GetBoolean());
    }

    // ── Fuzzy Term ───────────────────────────────────────────────────

    [Fact]
    public void FuzzyTerm_creates_correct_json() {
        var json = ParadeDbJsonQuery.FuzzyTerm("Content", "machin", 2).ToJson();
        var doc = Parse(json);
        var ft = doc.GetProperty("fuzzy_term");
        Assert.Equal("Content", ft.GetProperty("field").GetString());
        Assert.Equal("machin", ft.GetProperty("value").GetString());
        Assert.Equal(2, ft.GetProperty("distance").GetInt32());
    }

    [Fact]
    public void FuzzyTerm_with_options_creates_correct_json() {
        var json = ParadeDbJsonQuery.FuzzyTerm("Content", "machin", 2, true, true).ToJson();
        var doc = Parse(json);
        var ft = doc.GetProperty("fuzzy_term");
        Assert.True(ft.GetProperty("prefix").GetBoolean());
        Assert.True(ft.GetProperty("transposition_cost_one").GetBoolean());
    }

    // ── Phrase ────────────────────────────────────────────────────────

    [Fact]
    public void Phrase_creates_correct_json() {
        var json = ParadeDbJsonQuery.Phrase("Content", "neural", "networks").ToJson();
        var doc = Parse(json);
        var phrase = doc.GetProperty("phrase");
        Assert.Equal("Content", phrase.GetProperty("field").GetString());
        var phrases = phrase.GetProperty("phrases");
        Assert.Equal(2, phrases.GetArrayLength());
        Assert.Equal("neural", phrases[0].GetString());
    }

    [Fact]
    public void Phrase_with_slop_creates_correct_json() {
        var json = ParadeDbJsonQuery.Phrase("Content", 2, "neural", "networks").ToJson();
        var doc = Parse(json);
        var phrase = doc.GetProperty("phrase");
        Assert.Equal(2, phrase.GetProperty("slop").GetInt32());
    }

    // ── Phrase Prefix ────────────────────────────────────────────────

    [Fact]
    public void PhrasePrefix_creates_correct_json() {
        var json = ParadeDbJsonQuery.PhrasePrefix("Content", "running", "sh").ToJson();
        var doc = Parse(json);
        var pp = doc.GetProperty("phrase_prefix");
        Assert.Equal("Content", pp.GetProperty("field").GetString());
        Assert.Equal(2, pp.GetProperty("phrases").GetArrayLength());
    }

    // ── Regex ────────────────────────────────────────────────────────

    [Fact]
    public void Regex_creates_correct_json() {
        var json = ParadeDbJsonQuery.Regex("Content", "neuro.*").ToJson();
        var doc = Parse(json);
        var regex = doc.GetProperty("regex");
        Assert.Equal("Content", regex.GetProperty("field").GetString());
        Assert.Equal("neuro.*", regex.GetProperty("pattern").GetString());
    }

    // ── Range ────────────────────────────────────────────────────────

    [Fact]
    public void Range_with_both_bounds_creates_correct_json() {
        var json = ParadeDbJsonQuery.Range("Price", 10, 100, lowerInclusive: true, upperInclusive: false).ToJson();
        var doc = Parse(json);
        var range = doc.GetProperty("range");
        Assert.Equal("Price", range.GetProperty("field").GetString());
        Assert.Equal(10, range.GetProperty("lower_bound").GetProperty("included").GetInt32());
        Assert.Equal(100, range.GetProperty("upper_bound").GetProperty("excluded").GetInt32());
    }

    [Fact]
    public void Range_with_lower_bound_only_creates_correct_json() {
        var json = ParadeDbJsonQuery.Range("Price", 10, null).ToJson();
        var doc = Parse(json);
        var range = doc.GetProperty("range");
        Assert.True(range.TryGetProperty("lower_bound", out _));
        Assert.False(range.TryGetProperty("upper_bound", out _));
    }

    [Fact]
    public void Range_with_is_datetime_creates_correct_json() {
        var dt = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var json = ParadeDbJsonQuery.Range("ReportingDate", dt, null, isDatetime: true).ToJson();
        var doc = Parse(json);
        var range = doc.GetProperty("range");
        Assert.True(range.GetProperty("is_datetime").GetBoolean());
        Assert.Equal("2025-01-15T00:00:00Z", range.GetProperty("lower_bound").GetProperty("included").GetString());
    }

    // ── Boost ────────────────────────────────────────────────────────

    [Fact]
    public void Boost_wraps_inner_query_correctly() {
        var inner = ParadeDbJsonQuery.Parse("shoes");
        var json = ParadeDbJsonQuery.Boost(inner, 2.5).ToJson();
        var doc = Parse(json);
        var boost = doc.GetProperty("boost");
        Assert.Equal(2.5, boost.GetProperty("factor").GetDouble());
        Assert.True(boost.GetProperty("query").TryGetProperty("parse", out _));
    }

    // ── Const Score ──────────────────────────────────────────────────

    [Fact]
    public void ConstScore_wraps_inner_query_correctly() {
        var inner = ParadeDbJsonQuery.Parse("shoes");
        var json = ParadeDbJsonQuery.ConstScore(inner, 1.0).ToJson();
        var doc = Parse(json);
        var cs = doc.GetProperty("const_score");
        Assert.Equal(1.0, cs.GetProperty("score").GetDouble());
        Assert.True(cs.GetProperty("query").TryGetProperty("parse", out _));
    }

    // ── Exists ───────────────────────────────────────────────────────

    [Fact]
    public void Exists_creates_correct_json() {
        var json = ParadeDbJsonQuery.Exists("Content").ToJson();
        var doc = Parse(json);
        Assert.Equal("Content", doc.GetProperty("exists").GetProperty("field").GetString());
    }

    // ── All ──────────────────────────────────────────────────────────

    [Fact]
    public void All_creates_correct_json() {
        var json = ParadeDbJsonQuery.All().ToJson();
        var doc = Parse(json);
        Assert.Equal(JsonValueKind.Null, doc.GetProperty("all").ValueKind);
    }

    // ── Disjunction Max ──────────────────────────────────────────────

    [Fact]
    public void DisjunctionMax_creates_correct_json() {
        var json = ParadeDbJsonQuery.DisjunctionMax(
            ParadeDbJsonQuery.Parse("shoes"),
            ParadeDbJsonQuery.Match("boots")).ToJson();
        var doc = Parse(json);
        var dm = doc.GetProperty("disjunction_max");
        Assert.Equal(2, dm.GetProperty("disjuncts").GetArrayLength());
    }

    // ── More Like This ───────────────────────────────────────────────

    [Fact]
    public void MoreLikeThis_creates_correct_json() {
        var json = ParadeDbJsonQuery.MoreLikeThis(42).ToJson();
        var doc = Parse(json);
        Assert.Equal(42, doc.GetProperty("more_like_this").GetProperty("document_id").GetInt32());
    }

    // ── Boolean ──────────────────────────────────────────────────────

    [Fact]
    public void Boolean_must_creates_correct_json() {
        var json = ParadeDbJsonQuery.Boolean(b => b
            .Must(ParadeDbJsonQuery.Parse("shoes"))).ToJson();
        var doc = Parse(json);
        var boolean = doc.GetProperty("boolean");
        Assert.Equal(1, boolean.GetProperty("must").GetArrayLength());
        Assert.False(boolean.TryGetProperty("should", out _));
        Assert.False(boolean.TryGetProperty("must_not", out _));
    }

    [Fact]
    public void Boolean_should_creates_correct_json() {
        var json = ParadeDbJsonQuery.Boolean(b => b
            .Should(ParadeDbJsonQuery.Parse("shoes"), ParadeDbJsonQuery.Parse("boots"))).ToJson();
        var doc = Parse(json);
        Assert.Equal(2, doc.GetProperty("boolean").GetProperty("should").GetArrayLength());
    }

    [Fact]
    public void Boolean_must_not_creates_correct_json() {
        var json = ParadeDbJsonQuery.Boolean(b => b
            .MustNot(ParadeDbJsonQuery.Term("Status", "archived"))).ToJson();
        var doc = Parse(json);
        Assert.Equal(1, doc.GetProperty("boolean").GetProperty("must_not").GetArrayLength());
    }

    [Fact]
    public void Boolean_combined_creates_correct_json() {
        var json = ParadeDbJsonQuery.Boolean(b => b
            .Must(ParadeDbJsonQuery.Parse("revenue growth"))
            .Should(ParadeDbJsonQuery.Term("DocumentType", 10))
            .MustNot(ParadeDbJsonQuery.Term("Status", "archived"))).ToJson();
        var doc = Parse(json);
        var boolean = doc.GetProperty("boolean");
        Assert.Equal(1, boolean.GetProperty("must").GetArrayLength());
        Assert.Equal(1, boolean.GetProperty("should").GetArrayLength());
        Assert.Equal(1, boolean.GetProperty("must_not").GetArrayLength());
    }

    [Fact]
    public void Nested_boolean_creates_correct_json() {
        var json = ParadeDbJsonQuery.Boolean(b => b
            .Must(
                ParadeDbJsonQuery.Parse("revenue growth"),
                ParadeDbJsonQuery.Boolean(inner => inner
                    .Should(
                        ParadeDbJsonQuery.Term("DocumentType", 10),
                        ParadeDbJsonQuery.Term("DocumentType", 20))))).ToJson();
        var doc = Parse(json);
        var must = doc.GetProperty("boolean").GetProperty("must");
        Assert.Equal(2, must.GetArrayLength());
        Assert.True(must[1].TryGetProperty("boolean", out var nested));
        Assert.Equal(2, nested.GetProperty("should").GetArrayLength());
    }
}
