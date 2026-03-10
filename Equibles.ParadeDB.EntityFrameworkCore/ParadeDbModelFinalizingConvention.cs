using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Equibles.ParadeDB.EntityFrameworkCore;

public sealed class ParadeDbModelFinalizingConvention : IModelFinalizingConvention {
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context) {
        var hasBm25Index = false;

        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes()) {
            var attribute = entityType.ClrType.GetCustomAttribute<Bm25IndexAttribute>();
            if (attribute == null) continue;

            var indexBuilder = entityType.Builder.HasIndex(attribute.Columns, fromDataAnnotation: true);
            if (indexBuilder == null) continue;

            indexBuilder.HasAnnotation("Npgsql:IndexMethod", "bm25", fromDataAnnotation: true);

            // Resolve the key field to the actual column name
            var keyProperty = entityType.FindProperty(attribute.KeyField);
            var keyColumnName = keyProperty?.GetColumnName() ?? attribute.KeyField;
            indexBuilder.HasAnnotation("Npgsql:StorageParameter:key_field", keyColumnName, fromDataAnnotation: true);

            hasBm25Index = true;
        }

        if (hasBm25Index) {
            modelBuilder.HasAnnotation("Npgsql:PostgresExtension:pg_search", ",,", fromDataAnnotation: true);
        }
    }
}
