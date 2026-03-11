using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;

namespace Equibles.ParadeDB.EntityFrameworkCore;

public sealed class ParadeDbMethodCallTranslator : IMethodCallTranslator {
    private readonly ISqlExpressionFactory _sql;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    // ── Basic Search ──────────────────────────────────────────────────
    private static readonly MethodInfo MatchesMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.Matches), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private static readonly MethodInfo MatchesAllMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesAll), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    // ── Phrase Search ─────────────────────────────────────────────────
    private static readonly MethodInfo MatchesPhraseMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesPhrase), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private static readonly MethodInfo MatchesPhraseSlopMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesPhrase), [typeof(DbFunctions), typeof(string), typeof(string), typeof(int)])!;

    // ── Term Search ───────────────────────────────────────────────────
    private static readonly MethodInfo MatchesTermMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesTerm), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private static readonly MethodInfo MatchesTermSetMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesTermSet), [typeof(DbFunctions), typeof(string), typeof(string[])])!;

    // ── Fuzzy Search ──────────────────────────────────────────────────
    private static readonly MethodInfo MatchesFuzzyMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesFuzzy), [typeof(DbFunctions), typeof(string), typeof(string), typeof(int)])!;

    private static readonly MethodInfo MatchesFuzzyFullMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesFuzzy), [typeof(DbFunctions), typeof(string), typeof(string), typeof(int), typeof(bool), typeof(bool)])!;

    private static readonly MethodInfo MatchesAllFuzzyMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesAllFuzzy), [typeof(DbFunctions), typeof(string), typeof(string), typeof(int)])!;

    private static readonly MethodInfo MatchesAllFuzzyFullMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesAllFuzzy), [typeof(DbFunctions), typeof(string), typeof(string), typeof(int), typeof(bool), typeof(bool)])!;

    private static readonly MethodInfo MatchesTermFuzzyMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesTermFuzzy), [typeof(DbFunctions), typeof(string), typeof(string), typeof(int)])!;

    private static readonly MethodInfo MatchesTermFuzzyFullMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesTermFuzzy), [typeof(DbFunctions), typeof(string), typeof(string), typeof(int), typeof(bool), typeof(bool)])!;

    // ── Boost ─────────────────────────────────────────────────────────
    private static readonly MethodInfo MatchesBoostedMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesBoosted), [typeof(DbFunctions), typeof(string), typeof(string), typeof(double)])!;

    private static readonly MethodInfo MatchesAllBoostedMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesAllBoosted), [typeof(DbFunctions), typeof(string), typeof(string), typeof(double)])!;

    // ── Fuzzy + Boost Combined ────────────────────────────────────────
    private static readonly MethodInfo MatchesFuzzyBoostedMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesFuzzyBoosted), [typeof(DbFunctions), typeof(string), typeof(string), typeof(int), typeof(double)])!;

    private static readonly MethodInfo MatchesAllFuzzyBoostedMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MatchesAllFuzzyBoosted), [typeof(DbFunctions), typeof(string), typeof(string), typeof(int), typeof(double)])!;

    // ── BM25 Scoring ──────────────────────────────────────────────────
    private static readonly MethodInfo ScoreMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.Score), [typeof(DbFunctions), typeof(object)])!;

    // ── Snippets ──────────────────────────────────────────────────────
    private static readonly MethodInfo SnippetMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.Snippet), [typeof(DbFunctions), typeof(string)])!;

    private static readonly MethodInfo SnippetParamsMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.Snippet), [typeof(DbFunctions), typeof(string), typeof(string), typeof(string), typeof(int)])!;

    private static readonly MethodInfo SnippetsMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.Snippets), [typeof(DbFunctions), typeof(string), typeof(int), typeof(int), typeof(int)])!;

    // ── Parse Query ───────────────────────────────────────────────────
    private static readonly MethodInfo ParseMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.Parse), [typeof(DbFunctions), typeof(object), typeof(string)])!;

    private static readonly MethodInfo ParseFullMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.Parse), [typeof(DbFunctions), typeof(object), typeof(string), typeof(bool), typeof(bool)])!;

    // ── Regex Search ──────────────────────────────────────────────────
    private static readonly MethodInfo RegexMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.Regex), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    // ── Phrase Prefix ─────────────────────────────────────────────────
    private static readonly MethodInfo PhrasePrefixMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.PhrasePrefix), [typeof(DbFunctions), typeof(string), typeof(string[])])!;

    private static readonly MethodInfo PhrasePrefixMaxMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.PhrasePrefix), [typeof(DbFunctions), typeof(string), typeof(int), typeof(string[])])!;

    // ── More Like This ────────────────────────────────────────────────
    private static readonly MethodInfo MoreLikeThisMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MoreLikeThis), [typeof(DbFunctions), typeof(object), typeof(int)])!;

    private static readonly MethodInfo MoreLikeThisFieldsMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.MoreLikeThis), [typeof(DbFunctions), typeof(object), typeof(int), typeof(string[])])!;

    public ParadeDbMethodCallTranslator(ISqlExpressionFactory sql, IRelationalTypeMappingSource typeMappingSource) {
        _sql = sql;
        _typeMappingSource = typeMappingSource;
    }

    public SqlExpression Translate(SqlExpression instance, MethodInfo method, IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger) {
        // arguments[0] is always DbFunctions (ignored)

        // ── Basic Search ──────────────────────────────────────────
        if (method == MatchesMethod)
            return MakeBinaryBool(arguments[1], "|||", arguments[2]);

        if (method == MatchesAllMethod)
            return MakeBinaryBool(arguments[1], "&&&", arguments[2]);

        // ── Phrase Search ─────────────────────────────────────────
        if (method == MatchesPhraseMethod)
            return MakeBinaryBool(arguments[1], "###", arguments[2]);

        if (method == MatchesPhraseSlopMethod)
            return MakeBinaryBool(arguments[1], "###", WithModifier(arguments[2], BuildSlopSuffix(arguments[3])));

        // ── Term Search ───────────────────────────────────────────
        if (method == MatchesTermMethod)
            return MakeBinaryBool(arguments[1], "===", arguments[2]);

        if (method == MatchesTermSetMethod)
            return MakeBinaryBool(arguments[1], "===", arguments[2]);

        // ── Fuzzy Search ──────────────────────────────────────────
        if (method == MatchesFuzzyMethod)
            return MakeBinaryBool(arguments[1], "|||", WithModifier(arguments[2], BuildFuzzySuffix(arguments[3])));

        if (method == MatchesFuzzyFullMethod)
            return MakeBinaryBool(arguments[1], "|||", WithModifier(arguments[2], BuildFuzzyFullSuffix(arguments[3], arguments[4], arguments[5])));

        if (method == MatchesAllFuzzyMethod)
            return MakeBinaryBool(arguments[1], "&&&", WithModifier(arguments[2], BuildFuzzySuffix(arguments[3])));

        if (method == MatchesAllFuzzyFullMethod)
            return MakeBinaryBool(arguments[1], "&&&", WithModifier(arguments[2], BuildFuzzyFullSuffix(arguments[3], arguments[4], arguments[5])));

        if (method == MatchesTermFuzzyMethod)
            return MakeBinaryBool(arguments[1], "===", WithModifier(arguments[2], BuildFuzzySuffix(arguments[3])));

        if (method == MatchesTermFuzzyFullMethod)
            return MakeBinaryBool(arguments[1], "===", WithModifier(arguments[2], BuildFuzzyFullSuffix(arguments[3], arguments[4], arguments[5])));

        // ── Boost ─────────────────────────────────────────────────
        if (method == MatchesBoostedMethod)
            return MakeBinaryBool(arguments[1], "|||", WithModifier(arguments[2], BuildBoostSuffix(arguments[3])));

        if (method == MatchesAllBoostedMethod)
            return MakeBinaryBool(arguments[1], "&&&", WithModifier(arguments[2], BuildBoostSuffix(arguments[3])));

        // ── Fuzzy + Boost Combined ────────────────────────────────
        if (method == MatchesFuzzyBoostedMethod)
            return MakeBinaryBool(arguments[1], "|||",
                WithModifier(arguments[2], BuildFuzzySuffix(arguments[3]) + BuildBoostSuffix(arguments[4])));

        if (method == MatchesAllFuzzyBoostedMethod)
            return MakeBinaryBool(arguments[1], "&&&",
                WithModifier(arguments[2], BuildFuzzySuffix(arguments[3]) + BuildBoostSuffix(arguments[4])));

        // ── BM25 Scoring ──────────────────────────────────────────
        if (method == ScoreMethod)
            return _sql.Function("pdb.score", [arguments[1]],
                nullable: true, argumentsPropagateNullability: [true],
                typeof(double), _typeMappingSource.FindMapping(typeof(double)));

        // ── Snippets ──────────────────────────────────────────────
        if (method == SnippetMethod)
            return _sql.Function("pdb.snippet", [arguments[1]],
                nullable: true, argumentsPropagateNullability: [true],
                typeof(string), _typeMappingSource.FindMapping(typeof(string)));

        if (method == SnippetParamsMethod)
            return new ParadeDbNamedArgFunctionExpression("pdb.snippet",
                [arguments[1]],
                [
                    ("start_tag", arguments[2]),
                    ("end_tag", arguments[3]),
                    ("max_num_chars", arguments[4])
                ],
                typeof(string), _typeMappingSource.FindMapping(typeof(string)));

        if (method == SnippetsMethod)
            return new ParadeDbNamedArgFunctionExpression("pdb.snippets",
                [arguments[1]],
                [
                    ("max_num_chars", arguments[2]),
                    ("\"limit\"", arguments[3]),
                    ("\"offset\"", arguments[4])
                ],
                typeof(string), _typeMappingSource.FindMapping(typeof(string)));

        // ── Parse Query ───────────────────────────────────────────
        if (method == ParseMethod) {
            var parseFunc = _sql.Function("pdb.parse", [arguments[2]],
                nullable: true, argumentsPropagateNullability: [true],
                typeof(bool), _typeMappingSource.FindMapping(typeof(bool)));
            return MakeBinaryBool(arguments[1], "@@@", parseFunc);
        }

        if (method == ParseFullMethod) {
            var parseFunc = new ParadeDbNamedArgFunctionExpression("pdb.parse",
                [arguments[2]],
                [
                    ("lenient", arguments[3]),
                    ("conjunction_mode", arguments[4])
                ],
                typeof(bool), _typeMappingSource.FindMapping(typeof(bool)));
            return MakeBinaryBool(arguments[1], "@@@", parseFunc);
        }

        // ── Regex Search ──────────────────────────────────────────
        if (method == RegexMethod) {
            var regexFunc = _sql.Function("pdb.regex", [arguments[2]],
                nullable: true, argumentsPropagateNullability: [true],
                typeof(bool), _typeMappingSource.FindMapping(typeof(bool)));
            return MakeBinaryBool(arguments[1], "@@@", regexFunc);
        }

        // ── Phrase Prefix ─────────────────────────────────────────
        if (method == PhrasePrefixMethod) {
            var phrasePrefixFunc = _sql.Function("pdb.phrase_prefix", [arguments[2]],
                nullable: true, argumentsPropagateNullability: [true],
                typeof(bool), _typeMappingSource.FindMapping(typeof(bool)));
            return MakeBinaryBool(arguments[1], "@@@", phrasePrefixFunc);
        }

        if (method == PhrasePrefixMaxMethod) {
            var phrasePrefixFunc = new ParadeDbNamedArgFunctionExpression("pdb.phrase_prefix",
                [arguments[3]],
                [("max_expansions", arguments[2])],
                typeof(bool), _typeMappingSource.FindMapping(typeof(bool)));
            return MakeBinaryBool(arguments[1], "@@@", phrasePrefixFunc);
        }

        // ── More Like This ────────────────────────────────────────
        if (method == MoreLikeThisMethod) {
            var mltFunc = _sql.Function("pdb.more_like_this", [arguments[2]],
                nullable: true, argumentsPropagateNullability: [true],
                typeof(bool), _typeMappingSource.FindMapping(typeof(bool)));
            return MakeBinaryBool(arguments[1], "@@@", mltFunc);
        }

        if (method == MoreLikeThisFieldsMethod) {
            var mltFunc = _sql.Function("pdb.more_like_this", [arguments[2], arguments[3]],
                nullable: true, argumentsPropagateNullability: [true, true],
                typeof(bool), _typeMappingSource.FindMapping(typeof(bool)));
            return MakeBinaryBool(arguments[1], "@@@", mltFunc);
        }

        return null;
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private PgUnknownBinaryExpression MakeBinaryBool(SqlExpression left, string op, SqlExpression right) =>
        new(left, right, op, typeof(bool), _typeMappingSource.FindMapping(typeof(bool)));

    private SqlExpression WithModifier(SqlExpression inner, string suffix) =>
        new ParadeDbModifiedQueryExpression(inner, suffix, inner.Type, inner.TypeMapping);

    private static int ExtractInt(SqlExpression expr) =>
        expr is SqlConstantExpression { Value: int value }
            ? value
            : throw new InvalidOperationException("ParadeDB modifier parameters (distance, slop, maxExpansions) must be compile-time constants.");

    private static double ExtractDouble(SqlExpression expr) =>
        expr is SqlConstantExpression { Value: double value }
            ? value
            : throw new InvalidOperationException("ParadeDB modifier parameters (boost factor) must be compile-time constants.");

    private static bool ExtractBool(SqlExpression expr) =>
        expr is SqlConstantExpression { Value: bool value }
            ? value
            : throw new InvalidOperationException("ParadeDB modifier parameters (prefix, transpositionCostOne) must be compile-time constants.");

    private static string BuildFuzzySuffix(SqlExpression distanceExpr) {
        var distance = ExtractInt(distanceExpr);
        return $"::pdb.fuzzy({distance})";
    }

    private static string BuildFuzzyFullSuffix(SqlExpression distanceExpr, SqlExpression prefixExpr, SqlExpression transpExpr) {
        var distance = ExtractInt(distanceExpr);
        var prefix = ExtractBool(prefixExpr);
        var transp = ExtractBool(transpExpr);
        return $"::pdb.fuzzy({distance}, {BoolLit(prefix)}, {BoolLit(transp)})";
    }

    private static string BuildBoostSuffix(SqlExpression boostExpr) {
        var boost = ExtractDouble(boostExpr);
        return FormattableString.Invariant($"::pdb.boost({boost})");
    }

    private static string BuildSlopSuffix(SqlExpression slopExpr) {
        var slop = ExtractInt(slopExpr);
        return $"::pdb.slop({slop})";
    }

    private static string BoolLit(bool value) => value ? "true" : "false";
}
