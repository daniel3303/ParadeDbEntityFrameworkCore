using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace Equibles.ParadeDB.EntityFrameworkCore;

public class ParadeDbParameterBasedSqlProcessor : NpgsqlParameterBasedSqlProcessor
{
    private readonly RelationalParameterBasedSqlProcessorDependencies _dependencies;

#if NET8_0
    private readonly bool _useRelationalNulls;

    public ParadeDbParameterBasedSqlProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        bool useRelationalNulls
    )
        : base(dependencies, useRelationalNulls)
    {
        _dependencies = dependencies;
        _useRelationalNulls = useRelationalNulls;
    }

    protected override Expression ProcessSqlNullability(
        Expression selectExpression,
        IReadOnlyDictionary<string, object> parametersValues,
        out bool canCache
    )
    {
        return new ParadeDbSqlNullabilityProcessor(_dependencies, _useRelationalNulls).Process(
            selectExpression,
            parametersValues,
            out canCache
        );
    }
#elif NET9_0
    private readonly RelationalParameterBasedSqlProcessorParameters _parameters;

    public ParadeDbParameterBasedSqlProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        RelationalParameterBasedSqlProcessorParameters parameters
    )
        : base(dependencies, parameters)
    {
        _dependencies = dependencies;
        _parameters = parameters;
    }

    protected override Expression ProcessSqlNullability(
        Expression selectExpression,
        IReadOnlyDictionary<string, object> parametersValues,
        out bool canCache
    )
    {
        return new ParadeDbSqlNullabilityProcessor(_dependencies, _parameters).Process(
            selectExpression,
            parametersValues,
            out canCache
        );
    }
#else
    private readonly RelationalParameterBasedSqlProcessorParameters _parameters;

    public ParadeDbParameterBasedSqlProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        RelationalParameterBasedSqlProcessorParameters parameters
    )
        : base(dependencies, parameters)
    {
        _dependencies = dependencies;
        _parameters = parameters;
    }

    protected override Expression ProcessSqlNullability(
        Expression expression,
        ParametersCacheDecorator parametersDecorator
    )
    {
        return new ParadeDbSqlNullabilityProcessor(_dependencies, _parameters).Process(
            expression,
            parametersDecorator
        );
    }
#endif
}
