using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore;

/// <summary>
/// IQueryable extension methods for ParadeDB JSON query search and BM25 score ordering.
/// </summary>
public static class ParadeDbSearchExtensions {
    private static readonly MethodInfo JsonSearchMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.JsonSearch), [typeof(DbFunctions), typeof(object), typeof(string)])!;

    private static readonly MethodInfo ScoreMethod = typeof(ParadeDbFunctions)
        .GetMethod(nameof(ParadeDbFunctions.Score), [typeof(DbFunctions), typeof(object)])!;

    /// <summary>
    /// Adds a WHERE clause with a ParadeDB JSON query.
    /// Translates to: <c>keyField @@@ 'json'::pdb.query</c>.
    /// </summary>
    public static IQueryable<T> JsonSearch<T>(this IQueryable<T> source,
        Expression<Func<T, object>> keyField, ParadeDbJsonQuery query) where T : class {
        var efFunctionsExpr = Expression.Property(null, typeof(EF), nameof(EF.Functions));
        var keyBody = BoxIfNeeded(StripConvert(keyField.Body));
        var jsonConstant = Expression.Constant(query.ToJson(), typeof(string));

        var callExpr = Expression.Call(JsonSearchMethod, efFunctionsExpr, keyBody, jsonConstant);
        var predicate = Expression.Lambda<Func<T, bool>>(callExpr, keyField.Parameters);

        return source.Where(predicate);
    }

    /// <summary>
    /// Orders by BM25 score descending (highest relevance first).
    /// </summary>
    public static IOrderedQueryable<T> OrderByScoreDescending<T>(this IQueryable<T> source,
        Expression<Func<T, object>> keyField) where T : class {
        return ApplyScoreOrdering(source, keyField, nameof(Queryable.OrderByDescending));
    }

    /// <summary>
    /// Orders by BM25 score ascending.
    /// </summary>
    public static IOrderedQueryable<T> OrderByScore<T>(this IQueryable<T> source,
        Expression<Func<T, object>> keyField) where T : class {
        return ApplyScoreOrdering(source, keyField, nameof(Queryable.OrderBy));
    }

    private static IOrderedQueryable<T> ApplyScoreOrdering<T>(IQueryable<T> source,
        Expression<Func<T, object>> keyField, string methodName) where T : class {
        var efFunctionsExpr = Expression.Property(null, typeof(EF), nameof(EF.Functions));
        var keyBody = BoxIfNeeded(StripConvert(keyField.Body));

        var scoreCall = Expression.Call(ScoreMethod, efFunctionsExpr, keyBody);
        var scoreSelector = Expression.Lambda<Func<T, double>>(scoreCall, keyField.Parameters);

        var orderByMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), typeof(double));

        var result = orderByMethod.Invoke(null, [source, scoreSelector]);
        return (IOrderedQueryable<T>)result!;
    }

    private static Expression StripConvert(Expression expression) =>
        expression is UnaryExpression { NodeType: ExpressionType.Convert } unary
            ? unary.Operand
            : expression;

    private static Expression BoxIfNeeded(Expression expression) =>
        expression.Type.IsValueType
            ? Expression.Convert(expression, typeof(object))
            : expression;
}
