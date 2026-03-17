using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore;

/// <summary>
/// Provides CLR methods that translate to ParadeDB pg_search SQL operators and functions.
/// These methods can only be used in EF Core LINQ queries — they have no in-memory implementation.
/// </summary>
public static class ParadeDbFunctions {
    private const string OnlyInLinq = "This method can only be used in EF Core LINQ queries.";

    // ── Basic Search ──────────────────────────────────────────────────

    /// <summary>
    /// BM25 disjunction match (OR). Translates to: column ||| 'query'.
    /// Matches documents containing any of the query terms.
    /// </summary>
    public static bool Matches(this DbFunctions _, string column, string query)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// BM25 conjunction match (AND). Translates to: column &&& 'query'.
    /// Matches documents containing all of the query terms.
    /// </summary>
    public static bool MatchesAll(this DbFunctions _, string column, string query)
        => throw new InvalidOperationException(OnlyInLinq);

    // ── Phrase Search ─────────────────────────────────────────────────

    /// <summary>
    /// Phrase match. Translates to: column ### 'query'.
    /// Matches terms in exact order.
    /// </summary>
    public static bool MatchesPhrase(this DbFunctions _, string column, string query)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// Phrase match with slop. Translates to: column ### 'query'::pdb.slop(N).
    /// Allows N words between terms or transposition of adjacent terms.
    /// </summary>
    public static bool MatchesPhrase(this DbFunctions _, string column, string query, int slop)
        => throw new InvalidOperationException(OnlyInLinq);

    // ── Term Search ───────────────────────────────────────────────────

    /// <summary>
    /// Exact term match. Translates to: column === 'query'.
    /// The query is NOT tokenized — no stemming or lowercasing is applied.
    /// </summary>
    public static bool MatchesTerm(this DbFunctions _, string column, string query)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// Multi-term match. Translates to: column === ARRAY['a', 'b'].
    /// Matches any of the exact terms.
    /// </summary>
    public static bool MatchesTermSet(this DbFunctions _, string column, params string[] terms)
        => throw new InvalidOperationException(OnlyInLinq);

    // ── Fuzzy Search (Levenshtein Distance) ───────────────────────────

    /// <summary>
    /// Fuzzy OR match. Translates to: column ||| 'query'::pdb.fuzzy(distance).
    /// Tolerates typos by allowing up to N single-character edits. Max distance is 2.
    /// </summary>
    public static bool MatchesFuzzy(this DbFunctions _, string column, string query, int distance)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// Fuzzy OR match with options. Translates to: column ||| 'query'::pdb.fuzzy(distance, prefix, transpositionCostOne).
    /// <paramref name="prefix"/> exempts the initial substring from edit distance.
    /// <paramref name="transpositionCostOne"/> counts swapping two adjacent characters as one edit.
    /// </summary>
    public static bool MatchesFuzzy(this DbFunctions _, string column, string query, int distance,
        bool prefix, bool transpositionCostOne)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// Fuzzy AND match. Translates to: column &&& 'query'::pdb.fuzzy(distance).
    /// </summary>
    public static bool MatchesAllFuzzy(this DbFunctions _, string column, string query, int distance)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// Fuzzy AND match with options. Translates to: column &&& 'query'::pdb.fuzzy(distance, prefix, transpositionCostOne).
    /// </summary>
    public static bool MatchesAllFuzzy(this DbFunctions _, string column, string query, int distance,
        bool prefix, bool transpositionCostOne)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// Fuzzy term match. Translates to: column === 'query'::pdb.fuzzy(distance).
    /// </summary>
    public static bool MatchesTermFuzzy(this DbFunctions _, string column, string query, int distance)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// Fuzzy term match with options. Translates to: column === 'query'::pdb.fuzzy(distance, prefix, transpositionCostOne).
    /// </summary>
    public static bool MatchesTermFuzzy(this DbFunctions _, string column, string query, int distance,
        bool prefix, bool transpositionCostOne)
        => throw new InvalidOperationException(OnlyInLinq);

    // ── Boost ─────────────────────────────────────────────────────────

    /// <summary>
    /// Boosted OR match. Translates to: column ||| 'query'::pdb.boost(factor).
    /// Increases the BM25 relevance weight. Factor range: -2048 to 2048.
    /// </summary>
    public static bool MatchesBoosted(this DbFunctions _, string column, string query, double boost)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// Boosted AND match. Translates to: column &&& 'query'::pdb.boost(factor).
    /// </summary>
    public static bool MatchesAllBoosted(this DbFunctions _, string column, string query, double boost)
        => throw new InvalidOperationException(OnlyInLinq);

    // ── Fuzzy + Boost Combined ────────────────────────────────────────

    /// <summary>
    /// Fuzzy OR match with boost. Translates to: column ||| 'query'::pdb.fuzzy(distance)::pdb.boost(factor).
    /// </summary>
    public static bool MatchesFuzzyBoosted(this DbFunctions _, string column, string query, int distance, double boost)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// Fuzzy AND match with boost. Translates to: column &&& 'query'::pdb.fuzzy(distance)::pdb.boost(factor).
    /// </summary>
    public static bool MatchesAllFuzzyBoosted(this DbFunctions _, string column, string query, int distance, double boost)
        => throw new InvalidOperationException(OnlyInLinq);

    // ── BM25 Scoring ──────────────────────────────────────────────────

    /// <summary>
    /// BM25 relevance score. Translates to: pdb.score(key_field).
    /// Returns the BM25 relevance score for the matched document.
    /// </summary>
    public static double Score(this DbFunctions _, object keyField)
        => throw new InvalidOperationException(OnlyInLinq);

    // ── Snippets ──────────────────────────────────────────────────────

    /// <summary>
    /// Basic snippet. Translates to: pdb.snippet(column).
    /// Returns a text excerpt with matched terms highlighted.
    /// </summary>
    public static string Snippet(this DbFunctions _, string column)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// Parameterized snippet. Translates to: pdb.snippet(column, start_tag => '...', end_tag => '...', max_num_chars => N).
    /// </summary>
    public static string Snippet(this DbFunctions _, string column, string startTag, string endTag, int maxNumChars)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// Multiple snippets. Translates to: pdb.snippets(column, max_num_chars => N, "limit" => L, "offset" => O).
    /// </summary>
    public static string Snippets(this DbFunctions _, string column, int maxNumChars, int limit, int offset)
        => throw new InvalidOperationException(OnlyInLinq);

    // ── Parse Query (Tantivy Syntax) ──────────────────────────────────

    /// <summary>
    /// Parse query. Translates to: key @@@ pdb.parse('query').
    /// Full query parser supporting field:value, boolean operators, ranges, and wildcards.
    /// </summary>
    public static bool Parse(this DbFunctions _, object keyField, string query)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// Parse query with options. Translates to: key @@@ pdb.parse('query', lenient => true, conjunction_mode => true).
    /// <paramref name="lenient"/>: ignores syntax errors. <paramref name="conjunctionMode"/>: defaults terms to AND.
    /// </summary>
    public static bool Parse(this DbFunctions _, object keyField, string query, bool lenient, bool conjunctionMode)
        => throw new InvalidOperationException(OnlyInLinq);

    // ── Regex Search ──────────────────────────────────────────────────

    /// <summary>
    /// Regex match. Translates to: column @@@ pdb.regex('pattern').
    /// Matches indexed tokens against a regular expression (Rust regex syntax).
    /// </summary>
    public static bool Regex(this DbFunctions _, string column, string pattern)
        => throw new InvalidOperationException(OnlyInLinq);

    // ── Phrase Prefix ─────────────────────────────────────────────────

    /// <summary>
    /// Phrase prefix match. Translates to: column @@@ pdb.phrase_prefix(ARRAY['term1', 'term2']).
    /// The last term is treated as a prefix — useful for autocomplete/type-ahead.
    /// </summary>
    public static bool PhrasePrefix(this DbFunctions _, string column, params string[] terms)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// Phrase prefix match with max expansions. Translates to: column @@@ pdb.phrase_prefix(ARRAY[...], max_expansions => N).
    /// </summary>
    public static bool PhrasePrefix(this DbFunctions _, string column, int maxExpansions, params string[] terms)
        => throw new InvalidOperationException(OnlyInLinq);

    // ── More Like This ────────────────────────────────────────────────

    /// <summary>
    /// More-like-this search. Translates to: key @@@ pdb.more_like_this(documentId).
    /// Finds documents similar to a given document.
    /// </summary>
    public static bool MoreLikeThis(this DbFunctions _, object keyField, int documentId)
        => throw new InvalidOperationException(OnlyInLinq);

    /// <summary>
    /// More-like-this with field restriction. Translates to: key @@@ pdb.more_like_this(documentId, ARRAY['field1', ...]).
    /// </summary>
    public static bool MoreLikeThis(this DbFunctions _, object keyField, int documentId, params string[] fields)
        => throw new InvalidOperationException(OnlyInLinq);

    // ── JSON Query Search ────────────────────────────────────────────

    /// <summary>
    /// JSON query search. Translates to: keyField @@@ 'jsonQuery'::pdb.query.
    /// Executes a structured query using ParadeDB's JSON query syntax.
    /// Build the query string using <see cref="ParadeDbJsonQuery"/>.
    /// </summary>
    public static bool JsonSearch(this DbFunctions _, object keyField, string jsonQuery)
        => throw new InvalidOperationException(OnlyInLinq);
}
