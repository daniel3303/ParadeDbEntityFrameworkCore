using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Equibles.ParadeDB.EntityFrameworkCore;

/// <summary>
/// Wraps an inner SQL expression with a ParadeDB modifier suffix.
/// Example: 'shoes'::pdb.fuzzy(2)::pdb.boost(2.0)
/// </summary>
public sealed class ParadeDbModifiedQueryExpression : SqlExpression
{
    public SqlExpression InnerExpression { get; }
    public string ModifierSuffix { get; }

    public ParadeDbModifiedQueryExpression(
        SqlExpression innerExpression,
        string modifierSuffix,
        Type type,
        RelationalTypeMapping typeMapping
    )
        : base(type, typeMapping)
    {
        InnerExpression = innerExpression;
        ModifierSuffix = modifierSuffix;
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var visited = (SqlExpression)visitor.Visit(InnerExpression);
        return visited == InnerExpression
            ? this
            : new ParadeDbModifiedQueryExpression(visited, ModifierSuffix, Type, TypeMapping);
    }

    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(InnerExpression);
        expressionPrinter.Append(ModifierSuffix);
    }

    public override bool Equals(object obj) =>
        obj is ParadeDbModifiedQueryExpression other
        && InnerExpression.Equals(other.InnerExpression)
        && ModifierSuffix == other.ModifierSuffix;

#if NET9_0_OR_GREATER
    public override Expression Quote() =>
        throw new NotSupportedException("ParadeDB expressions do not support precompiled queries.");
#endif

    public override int GetHashCode() =>
        HashCode.Combine(base.GetHashCode(), InnerExpression, ModifierSuffix);
}
