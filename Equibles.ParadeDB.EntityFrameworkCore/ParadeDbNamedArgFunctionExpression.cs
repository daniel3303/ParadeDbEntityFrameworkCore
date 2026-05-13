using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Equibles.ParadeDB.EntityFrameworkCore;

/// <summary>
/// SQL expression for functions with PostgreSQL named parameters (name => value).
/// Example: pdb.snippet("Content", start_tag => '&lt;b&gt;', max_num_chars => 100)
/// </summary>
public sealed class ParadeDbNamedArgFunctionExpression : SqlExpression {
    public string FunctionName { get; }
    public IReadOnlyList<SqlExpression> PositionalArgs { get; }
    public IReadOnlyList<(string Name, SqlExpression Value)> NamedArgs { get; }

    public ParadeDbNamedArgFunctionExpression(string functionName,
        IReadOnlyList<SqlExpression> positionalArgs,
        IReadOnlyList<(string Name, SqlExpression Value)> namedArgs,
        Type type, RelationalTypeMapping typeMapping)
        : base(type, typeMapping) {
        FunctionName = functionName;
        PositionalArgs = positionalArgs;
        NamedArgs = namedArgs;
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor) {
        var positionalChanged = false;
        var newPositional = new SqlExpression[PositionalArgs.Count];
        for (var i = 0; i < PositionalArgs.Count; i++) {
            newPositional[i] = (SqlExpression)visitor.Visit(PositionalArgs[i]);
            positionalChanged |= newPositional[i] != PositionalArgs[i];
        }

        var namedChanged = false;
        var newNamed = new (string Name, SqlExpression Value)[NamedArgs.Count];
        for (var i = 0; i < NamedArgs.Count; i++) {
            var visitedValue = (SqlExpression)visitor.Visit(NamedArgs[i].Value);
            newNamed[i] = (NamedArgs[i].Name, visitedValue);
            namedChanged |= visitedValue != NamedArgs[i].Value;
        }

        if (!positionalChanged && !namedChanged) return this;

        return new ParadeDbNamedArgFunctionExpression(FunctionName, newPositional, newNamed, Type, TypeMapping);
    }

    protected override void Print(ExpressionPrinter expressionPrinter) {
        expressionPrinter.Append(FunctionName).Append("(");

        for (var i = 0; i < PositionalArgs.Count; i++) {
            if (i > 0) expressionPrinter.Append(", ");
            expressionPrinter.Visit(PositionalArgs[i]);
        }

        for (var i = 0; i < NamedArgs.Count; i++) {
            if (i > 0 || PositionalArgs.Count > 0) expressionPrinter.Append(", ");
            expressionPrinter.Append(NamedArgs[i].Name).Append(" => ");
            expressionPrinter.Visit(NamedArgs[i].Value);
        }

        expressionPrinter.Append(")");
    }

    public override bool Equals(object obj) {
        if (obj is not ParadeDbNamedArgFunctionExpression other) return false;
        if (FunctionName != other.FunctionName) return false;
        if (PositionalArgs.Count != other.PositionalArgs.Count) return false;
        if (NamedArgs.Count != other.NamedArgs.Count) return false;

        for (var i = 0; i < PositionalArgs.Count; i++) {
            if (!PositionalArgs[i].Equals(other.PositionalArgs[i])) return false;
        }

        for (var i = 0; i < NamedArgs.Count; i++) {
            if (NamedArgs[i].Name != other.NamedArgs[i].Name) return false;
            if (!NamedArgs[i].Value.Equals(other.NamedArgs[i].Value)) return false;
        }

        return true;
    }

#if NET9_0_OR_GREATER
    public override Expression Quote() =>
        throw new NotSupportedException("ParadeDB expressions do not support precompiled queries.");
#endif

    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(FunctionName);
        foreach (var arg in PositionalArgs) hash.Add(arg);
        foreach (var (name, value) in NamedArgs) {
            hash.Add(name);
            hash.Add(value);
        }
        return hash.ToHashCode();
    }
}
