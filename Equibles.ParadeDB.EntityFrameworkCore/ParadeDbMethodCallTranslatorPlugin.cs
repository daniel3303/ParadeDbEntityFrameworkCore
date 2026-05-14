using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Equibles.ParadeDB.EntityFrameworkCore;

public sealed class ParadeDbMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
{
    public IEnumerable<IMethodCallTranslator> Translators { get; }

    public ParadeDbMethodCallTranslatorPlugin(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource
    )
    {
        Translators = [new ParadeDbMethodCallTranslator(sqlExpressionFactory, typeMappingSource)];
    }
}
