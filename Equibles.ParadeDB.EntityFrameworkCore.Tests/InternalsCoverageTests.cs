using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Equibles.ParadeDB.EntityFrameworkCore.Tests;

/// <summary>
/// Direct-invocation tests for internals not reachable through public LINQ surface:
///   - <see cref="ParadeDbMethodCallTranslator.Translate"/> fall-through (unknown method).
///   - <see cref="ParadeDbSqlNullabilityProcessor.VisitCustomSqlExpression"/> rebuild path
///     (children change during processing).
/// </summary>
public class InternalsCoverageTests
{
    private static IServiceProvider Services()
    {
        var ctx = new TestDbContext();
        return ((IInfrastructure<IServiceProvider>)ctx).Instance;
    }

    // ── Translator fall-through ───────────────────────────────────────

    [Fact]
    public void Translate_returns_null_for_unknown_method()
    {
        var services = Services();
        var plugins = services.GetService<IEnumerable<IMethodCallTranslatorPlugin>>()!;
        var translator = plugins
            .OfType<ParadeDbMethodCallTranslatorPlugin>()
            .Single()
            .Translators.OfType<ParadeDbMethodCallTranslator>()
            .Single();

        var unknown = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;
        var sql = services.GetService<ISqlExpressionFactory>()!;
        var instance = sql.Constant("foo");
        var arg = sql.Constant("f");

        // The diagnostics logger is registered in the service container by EF Core.
        var logger = services.GetService<IDiagnosticsLogger<DbLoggerCategory.Query>>()!;

        var result = translator.Translate(instance, unknown, [arg], logger);

        Assert.Null(result);
    }

    // ── Nullability processor: child-pass-through path ────────────────

    private static ParadeDbSqlNullabilityProcessor CreateProcessor()
    {
        var services = Services();
        var factory = (ParadeDbParameterBasedSqlProcessorFactory)
            services.GetService<IRelationalParameterBasedSqlProcessorFactory>()!;
        var depsField = typeof(ParadeDbParameterBasedSqlProcessorFactory).GetField(
            "_dependencies",
            BindingFlags.NonPublic | BindingFlags.Instance
        )!;
        var deps = (RelationalParameterBasedSqlProcessorDependencies)depsField.GetValue(factory)!;

#if NET8_0
        return new ParadeDbSqlNullabilityProcessor(deps, useRelationalNulls: false);
#elif NET9_0
        // EF Core 9 ctor: (bool useRelationalNulls, IReadOnlySet<string> parametersToConstantize)
        var parameters = new RelationalParameterBasedSqlProcessorParameters(
            false,
            new HashSet<string>()
        );
        return new ParadeDbSqlNullabilityProcessor(deps, parameters);
#else
        // EF Core 10 ctor: (bool useRelationalNulls, ParameterTranslationMode)
        var parameters = new RelationalParameterBasedSqlProcessorParameters(
            false,
            ParameterTranslationMode.Constant
        );
        return new ParadeDbSqlNullabilityProcessor(deps, parameters);
#endif
    }

    private static SqlExpression InvokeVisitCustom(
        ParadeDbSqlNullabilityProcessor processor,
        SqlExpression expr,
        bool allowOptimizedExpansion = false
    )
    {
        var method = typeof(ParadeDbSqlNullabilityProcessor).GetMethod(
            "VisitCustomSqlExpression",
            BindingFlags.NonPublic | BindingFlags.Instance
        )!;
        var args = new object?[] { expr, allowOptimizedExpansion, null };
        return (SqlExpression)method.Invoke(processor, args)!;
    }

    [Fact]
    public void NullabilityProcessor_ModifiedQueryExpression_keeps_instance_when_inner_unchanged()
    {
        var processor = CreateProcessor();
        var sql = Services().GetService<ISqlExpressionFactory>()!;
        var inner = sql.Constant("hello");
        var expr = new ParadeDbModifiedQueryExpression(
            inner,
            "::pdb.boost(2)",
            inner.Type,
            inner.TypeMapping
        );

        var result = InvokeVisitCustom(processor, expr);

        // Constant visits return the same instance, so the wrapper is preserved as well.
        Assert.Same(expr, result);
    }

    [Fact]
    public void NullabilityProcessor_NamedArgFunctionExpression_keeps_instance_when_unchanged()
    {
        var processor = CreateProcessor();
        var sql = Services().GetService<ISqlExpressionFactory>()!;
        var positional = sql.Constant("col");
        var namedValue = sql.Constant("<b>");
        var expr = new ParadeDbNamedArgFunctionExpression(
            "pdb.snippet",
            [positional],
            [("start_tag", namedValue)],
            typeof(string),
            positional.TypeMapping
        );

        var result = InvokeVisitCustom(processor, expr);

        Assert.Same(expr, result);
    }

    /// <summary>
    /// <c>IsNotNull(Constant("hi"))</c> is folded to <c>true</c> by the nullability processor —
    /// the visited child is a different <see cref="SqlExpression"/> instance, which triggers the
    /// "positional changed → rebuild" branch on <see cref="ParadeDbNamedArgFunctionExpression"/>.
    /// </summary>
    [Fact]
    public void NullabilityProcessor_NamedArgFunctionExpression_rebuilds_when_positional_simplified()
    {
        var processor = CreateProcessor();
        var sql = Services().GetService<ISqlExpressionFactory>()!;
        var positional = sql.IsNotNull(sql.Constant("hi"));
        var namedValue = sql.Constant("<b>");
        var expr = new ParadeDbNamedArgFunctionExpression(
            "pdb.snippet",
            [positional],
            [("start_tag", namedValue)],
            typeof(string),
            namedValue.TypeMapping
        );

        var result = InvokeVisitCustom(processor, expr);

        var rebuilt = Assert.IsType<ParadeDbNamedArgFunctionExpression>(result);
        Assert.NotSame(expr, rebuilt);
        Assert.Equal("pdb.snippet", rebuilt.FunctionName);
    }

    [Fact]
    public void NullabilityProcessor_NamedArgFunctionExpression_rebuilds_when_named_simplified()
    {
        var processor = CreateProcessor();
        var sql = Services().GetService<ISqlExpressionFactory>()!;
        var positional = sql.Constant("col");
        var namedValue = sql.IsNotNull(sql.Constant("hi"));
        var expr = new ParadeDbNamedArgFunctionExpression(
            "pdb.snippet",
            [positional],
            [("flag", namedValue)],
            typeof(string),
            positional.TypeMapping
        );

        var result = InvokeVisitCustom(processor, expr);

        var rebuilt = Assert.IsType<ParadeDbNamedArgFunctionExpression>(result);
        Assert.NotSame(expr, rebuilt);
    }

    /// <summary>
    /// Mirror of the named-arg rebuild test for <see cref="ParadeDbModifiedQueryExpression"/>:
    /// when the wrapped inner gets folded by the processor, the wrapper has to be rebuilt
    /// around the new instance — otherwise downstream visitors keep seeing the stale child.
    /// </summary>
    [Fact]
    public void NullabilityProcessor_ModifiedQueryExpression_rebuilds_when_inner_simplified()
    {
        var processor = CreateProcessor();
        var sql = Services().GetService<ISqlExpressionFactory>()!;
        var inner = sql.IsNotNull(sql.Constant("hi"));
        var expr = new ParadeDbModifiedQueryExpression(
            inner,
            "::pdb.boost(2)",
            inner.Type,
            inner.TypeMapping
        );

        var result = InvokeVisitCustom(processor, expr);

        var rebuilt = Assert.IsType<ParadeDbModifiedQueryExpression>(result);
        Assert.NotSame(expr, rebuilt);
        Assert.Equal("::pdb.boost(2)", rebuilt.ModifierSuffix);
    }
}
