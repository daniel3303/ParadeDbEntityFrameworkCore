using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace Equibles.ParadeDB.EntityFrameworkCore;

public class ParadeDbQuerySqlGenerator : NpgsqlQuerySqlGenerator
{
    public ParadeDbQuerySqlGenerator(
        QuerySqlGeneratorDependencies dependencies,
        IRelationalTypeMappingSource typeMappingSource,
        bool reverseNullOrderingEnabled,
        Version postgresVersion
    )
        : base(dependencies, typeMappingSource, reverseNullOrderingEnabled, postgresVersion) { }

    protected override Expression VisitExtension(Expression expression)
    {
        if (expression is ParadeDbModifiedQueryExpression modified)
        {
            Visit(modified.InnerExpression);
            Sql.Append(modified.ModifierSuffix);
            return expression;
        }

        if (expression is ParadeDbNamedArgFunctionExpression namedArg)
        {
            Sql.Append(namedArg.FunctionName).Append("(");

            for (var i = 0; i < namedArg.PositionalArgs.Count; i++)
            {
                if (i > 0)
                    Sql.Append(", ");
                Visit(namedArg.PositionalArgs[i]);
            }

            for (var i = 0; i < namedArg.NamedArgs.Count; i++)
            {
                if (i > 0 || namedArg.PositionalArgs.Count > 0)
                    Sql.Append(", ");
                Sql.Append(namedArg.NamedArgs[i].Name).Append(" => ");
                Visit(namedArg.NamedArgs[i].Value);
            }

            Sql.Append(")");
            return expression;
        }

        return base.VisitExtension(expression);
    }
}
