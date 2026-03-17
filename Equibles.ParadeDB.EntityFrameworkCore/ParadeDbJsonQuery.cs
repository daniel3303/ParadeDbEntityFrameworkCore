using System.Text.Json.Nodes;

namespace Equibles.ParadeDB.EntityFrameworkCore;

/// <summary>
/// Builds a ParadeDB JSON query for use with the <c>@@@</c> operator.
/// Use static factory methods to create query nodes, then call <see cref="ToJson"/> to serialize.
/// </summary>
public sealed class ParadeDbJsonQuery {
    private readonly JsonNode _node;

    internal ParadeDbJsonQuery(JsonNode node) {
        _node = node;
    }

    /// <summary>Serializes the query to a compact JSON string.</summary>
    public string ToJson() => _node.ToJsonString(new System.Text.Json.JsonSerializerOptions {
        WriteIndented = false
    });

    /// <inheritdoc/>
    public override string ToString() => ToJson();

    internal JsonNode CloneNode() => JsonNode.Parse(_node.ToJsonString())!;

    // ── Parse ────────────────────────────────────────────────────────

    /// <summary>
    /// Parse query. Produces: <c>{"parse":{"query_string":"..."}}</c>.
    /// Full query parser supporting field:value, boolean operators, ranges, and wildcards.
    /// </summary>
    public static ParadeDbJsonQuery Parse(string queryString) =>
        new(new JsonObject { ["parse"] = new JsonObject { ["query_string"] = queryString } });

    /// <summary>
    /// Parse query with options. Produces: <c>{"parse":{"query_string":"...","lenient":true,"conjunction_mode":true}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery Parse(string queryString, bool lenient, bool conjunctionMode) {
        var inner = new JsonObject { ["query_string"] = queryString };
        if (lenient) inner["lenient"] = true;
        if (conjunctionMode) inner["conjunction_mode"] = true;
        return new(new JsonObject { ["parse"] = inner });
    }

    // ── Term ─────────────────────────────────────────────────────────

    /// <summary>
    /// Exact term match. Produces: <c>{"term":{"field":"...","value":...}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery Term(string field, object value) =>
        new(new JsonObject { ["term"] = new JsonObject { ["field"] = field, ["value"] = CreateJsonValue(value) } });

    // ── Term Set ─────────────────────────────────────────────────────

    /// <summary>
    /// Multi-term match. Produces: <c>{"term_set":{"field":"...","terms":[...]}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery TermSet(string field, params object[] terms) {
        var arr = new JsonArray();
        foreach (var t in terms) arr.Add(CreateJsonValue(t));
        return new(new JsonObject { ["term_set"] = new JsonObject { ["field"] = field, ["terms"] = arr } });
    }

    // ── Match ────────────────────────────────────────────────────────

    /// <summary>
    /// Match query. Produces: <c>{"match":{"value":"..."}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery Match(string value) =>
        new(new JsonObject { ["match"] = new JsonObject { ["value"] = value } });

    /// <summary>
    /// Match query with field. Produces: <c>{"match":{"field":"...","value":"..."}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery Match(string value, string field) =>
        new(new JsonObject { ["match"] = new JsonObject { ["field"] = field, ["value"] = value } });

    /// <summary>
    /// Match query with field and options.
    /// </summary>
    public static ParadeDbJsonQuery Match(string value, string field, int distance, bool conjunctionMode) {
        var inner = new JsonObject { ["field"] = field, ["value"] = value, ["distance"] = distance };
        if (conjunctionMode) inner["conjunction_mode"] = true;
        return new(new JsonObject { ["match"] = inner });
    }

    // ── Fuzzy Term ───────────────────────────────────────────────────

    /// <summary>
    /// Fuzzy term match. Produces: <c>{"fuzzy_term":{"field":"...","value":"...","distance":N}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery FuzzyTerm(string field, string value, int distance) =>
        new(new JsonObject { ["fuzzy_term"] = new JsonObject { ["field"] = field, ["value"] = value, ["distance"] = distance } });

    /// <summary>
    /// Fuzzy term match with full options.
    /// </summary>
    public static ParadeDbJsonQuery FuzzyTerm(string field, string value, int distance, bool prefix, bool transpositionCostOne) {
        var inner = new JsonObject { ["field"] = field, ["value"] = value, ["distance"] = distance };
        if (prefix) inner["prefix"] = true;
        if (transpositionCostOne) inner["transposition_cost_one"] = true;
        return new(new JsonObject { ["fuzzy_term"] = inner });
    }

    // ── Phrase ────────────────────────────────────────────────────────

    /// <summary>
    /// Phrase match. Produces: <c>{"phrase":{"field":"...","phrases":[...]}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery Phrase(string field, params string[] phrases) {
        var arr = new JsonArray();
        foreach (var p in phrases) arr.Add(JsonValue.Create(p));
        return new(new JsonObject { ["phrase"] = new JsonObject { ["field"] = field, ["phrases"] = arr } });
    }

    /// <summary>
    /// Phrase match with slop.
    /// </summary>
    public static ParadeDbJsonQuery Phrase(string field, int slop, params string[] phrases) {
        var arr = new JsonArray();
        foreach (var p in phrases) arr.Add(JsonValue.Create(p));
        return new(new JsonObject { ["phrase"] = new JsonObject { ["field"] = field, ["phrases"] = arr, ["slop"] = slop } });
    }

    // ── Phrase Prefix ────────────────────────────────────────────────

    /// <summary>
    /// Phrase prefix match. Produces: <c>{"phrase_prefix":{"field":"...","phrases":[...]}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery PhrasePrefix(string field, params string[] phrases) {
        var arr = new JsonArray();
        foreach (var p in phrases) arr.Add(JsonValue.Create(p));
        return new(new JsonObject { ["phrase_prefix"] = new JsonObject { ["field"] = field, ["phrases"] = arr } });
    }

    // ── Regex ────────────────────────────────────────────────────────

    /// <summary>
    /// Regex match. Produces: <c>{"regex":{"field":"...","pattern":"..."}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery Regex(string field, string pattern) =>
        new(new JsonObject { ["regex"] = new JsonObject { ["field"] = field, ["pattern"] = pattern } });

    // ── Range ────────────────────────────────────────────────────────

    /// <summary>
    /// Range query. Produces bounds as <c>{"included":N}</c> or <c>{"excluded":N}</c>.
    /// Pass <c>null</c> for either bound to omit it.
    /// Set <paramref name="isDatetime"/> to true for DateTime range queries (adds <c>"is_datetime":true</c>).
    /// </summary>
    public static ParadeDbJsonQuery Range(string field, object lowerBound, object upperBound,
        bool lowerInclusive = true, bool upperInclusive = false, bool isDatetime = false) {
        var inner = new JsonObject { ["field"] = field };
        if (lowerBound != null) {
            var key = lowerInclusive ? "included" : "excluded";
            inner["lower_bound"] = new JsonObject { [key] = CreateJsonValue(lowerBound) };
        }
        if (upperBound != null) {
            var key = upperInclusive ? "included" : "excluded";
            inner["upper_bound"] = new JsonObject { [key] = CreateJsonValue(upperBound) };
        }
        if (isDatetime) inner["is_datetime"] = true;
        return new(new JsonObject { ["range"] = inner });
    }

    // ── Boost ────────────────────────────────────────────────────────

    /// <summary>
    /// Wraps a query with a boost factor. Produces: <c>{"boost":{"query":{...},"factor":N}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery Boost(ParadeDbJsonQuery query, double factor) =>
        new(new JsonObject { ["boost"] = new JsonObject { ["query"] = query.CloneNode(), ["factor"] = factor } });

    // ── Const Score ──────────────────────────────────────────────────

    /// <summary>
    /// Wraps a query with a constant score. Produces: <c>{"const_score":{"query":{...},"score":N}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery ConstScore(ParadeDbJsonQuery query, double score) =>
        new(new JsonObject { ["const_score"] = new JsonObject { ["query"] = query.CloneNode(), ["score"] = score } });

    // ── Exists ───────────────────────────────────────────────────────

    /// <summary>
    /// Exists query. Produces: <c>{"exists":{"field":"..."}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery Exists(string field) =>
        new(new JsonObject { ["exists"] = new JsonObject { ["field"] = field } });

    // ── All ──────────────────────────────────────────────────────────

    /// <summary>
    /// Match all documents. Produces: <c>{"all":null}</c>.
    /// </summary>
    public static ParadeDbJsonQuery All() =>
        new(new JsonObject { ["all"] = null });

    // ── Disjunction Max ──────────────────────────────────────────────

    /// <summary>
    /// Disjunction max query. Produces: <c>{"disjunction_max":{"disjuncts":[...]}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery DisjunctionMax(params ParadeDbJsonQuery[] queries) {
        var arr = new JsonArray();
        foreach (var q in queries) arr.Add(q.CloneNode());
        return new(new JsonObject { ["disjunction_max"] = new JsonObject { ["disjuncts"] = arr } });
    }

    // ── More Like This ───────────────────────────────────────────────

    /// <summary>
    /// More-like-this query. Produces: <c>{"more_like_this":{"document_id":N}}</c>.
    /// </summary>
    public static ParadeDbJsonQuery MoreLikeThis(int documentId) =>
        new(new JsonObject { ["more_like_this"] = new JsonObject { ["document_id"] = documentId } });

    // ── Boolean ──────────────────────────────────────────────────────

    /// <summary>
    /// Boolean query combining must/should/must_not clauses.
    /// </summary>
    public static ParadeDbJsonQuery Boolean(Action<ParadeDbBooleanQuery> configure) {
        var builder = new ParadeDbBooleanQuery();
        configure(builder);
        return new(builder.ToJsonNode());
    }

    // ── Helpers ──────────────────────────────────────────────────────

    internal static JsonNode CreateJsonValue(object value) => value switch {
        string s => JsonValue.Create(s),
        int i => JsonValue.Create(i),
        long l => JsonValue.Create(l),
        double d => JsonValue.Create(d),
        float f => JsonValue.Create(f),
        bool b => JsonValue.Create(b),
        Guid g => JsonValue.Create(g.ToString()),
        DateTime dt => JsonValue.Create(dt.Kind == DateTimeKind.Utc
            ? dt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            : dt.ToString("O")),
        Enum e => JsonValue.Create(Convert.ToInt32(e)),
        _ => JsonValue.Create(value.ToString())
    };
}
