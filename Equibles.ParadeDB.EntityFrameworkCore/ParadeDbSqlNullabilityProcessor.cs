using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace Equibles.ParadeDB.EntityFrameworkCore;

public class ParadeDbSqlNullabilityProcessor : NpgsqlSqlNullabilityProcessor {
#if NET8_0
    public ParadeDbSqlNullabilityProcessor(RelationalParameterBasedSqlProcessorDependencies dependencies,
        bool useRelationalNulls)
        : base(dependencies, useRelationalNulls) {
    }
#else
    public ParadeDbSqlNullabilityProcessor(RelationalParameterBasedSqlProcessorDependencies dependencies,
        RelationalParameterBasedSqlProcessorParameters parameters)
        : base(dependencies, parameters) {
    }
#endif

    protected override SqlExpression VisitCustomSqlExpression(SqlExpression sqlExpression,
        bool allowOptimizedExpansion, out bool nullable) {
        if (sqlExpression is ParadeDbModifiedQueryExpression modified) {
            nullable = false;
            var inner = Visit(modified.InnerExpression, allowOptimizedExpansion, out _);
            return inner == modified.InnerExpression
                ? modified
                : new ParadeDbModifiedQueryExpression(inner, modified.ModifierSuffix, modified.Type, modified.TypeMapping);
        }

        if (sqlExpression is ParadeDbNamedArgFunctionExpression namedArg) {
            nullable = true;

            var positionalChanged = false;
            var newPositional = new SqlExpression[namedArg.PositionalArgs.Count];
            for (var i = 0; i < namedArg.PositionalArgs.Count; i++) {
                newPositional[i] = Visit(namedArg.PositionalArgs[i], allowOptimizedExpansion, out _);
                positionalChanged |= newPositional[i] != namedArg.PositionalArgs[i];
            }

            var namedChanged = false;
            var newNamed = new (string Name, SqlExpression Value)[namedArg.NamedArgs.Count];
            for (var i = 0; i < namedArg.NamedArgs.Count; i++) {
                var visited = Visit(namedArg.NamedArgs[i].Value, allowOptimizedExpansion, out _);
                newNamed[i] = (namedArg.NamedArgs[i].Name, visited);
                namedChanged |= visited != namedArg.NamedArgs[i].Value;
            }

            if (!positionalChanged && !namedChanged) return namedArg;
            return new ParadeDbNamedArgFunctionExpression(namedArg.FunctionName, newPositional, newNamed,
                namedArg.Type, namedArg.TypeMapping);
        }

        return base.VisitCustomSqlExpression(sqlExpression, allowOptimizedExpansion, out nullable);
    }
}
