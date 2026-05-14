using System.Reflection;
using System.Text.Json.Nodes;
using Equibles.ParadeDB.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Equibles.ParadeDB.EntityFrameworkCore;

public sealed class ParadeDbModelFinalizingConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context
    )
    {
        var hasBm25Index = false;

        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            var attribute = entityType.ClrType.GetCustomAttribute<Bm25IndexAttribute>();

            ValidateNoOrphanFieldAttributes(entityType, attribute);

            if (attribute == null)
                continue;

            var indexBuilder = entityType.Builder.HasIndex(
                attribute.Columns,
                fromDataAnnotation: true
            );
            if (indexBuilder == null)
                continue;

            indexBuilder.HasAnnotation("Npgsql:IndexMethod", "bm25", fromDataAnnotation: true);

            var keyProperty = entityType.FindProperty(attribute.KeyField);
            var keyColumnName = keyProperty?.GetColumnName() ?? attribute.KeyField;
            indexBuilder.HasAnnotation(
                "Npgsql:StorageParameter:key_field",
                keyColumnName,
                fromDataAnnotation: true
            );

            BuildFieldTypeAnnotations(entityType, attribute, indexBuilder);

            hasBm25Index = true;
        }

        if (hasBm25Index)
        {
            modelBuilder.HasAnnotation(
                "Npgsql:PostgresExtension:pg_search",
                ",,",
                fromDataAnnotation: true
            );
        }
    }

    private static void ValidateNoOrphanFieldAttributes(
        IConventionEntityType entityType,
        Bm25IndexAttribute attribute
    )
    {
        foreach (var property in entityType.GetProperties())
        {
            var propertyInfo = property.PropertyInfo;
            if (propertyInfo == null)
                continue;

            var hasFieldAttr =
                propertyInfo.GetCustomAttribute<Bm25TextAttribute>() != null
                || propertyInfo.GetCustomAttribute<Bm25NumericAttribute>() != null
                || propertyInfo.GetCustomAttribute<Bm25BooleanAttribute>() != null
                || propertyInfo.GetCustomAttribute<Bm25DateTimeAttribute>() != null
                || propertyInfo.GetCustomAttribute<Bm25JsonAttribute>() != null;

            if (!hasFieldAttr)
                continue;

            if (attribute == null)
            {
                throw new InvalidOperationException(
                    $"Property '{entityType.ClrType.Name}.{property.Name}' has a BM25 field attribute, "
                        + "but the entity has no [Bm25Index] attribute."
                );
            }

            if (Array.IndexOf(attribute.Columns, property.Name) < 0)
            {
                throw new InvalidOperationException(
                    $"Property '{entityType.ClrType.Name}.{property.Name}' has a BM25 field attribute, "
                        + "but it is not listed in the [Bm25Index] columns."
                );
            }
        }
    }

    private static void BuildFieldTypeAnnotations(
        IConventionEntityType entityType,
        Bm25IndexAttribute attribute,
        IConventionIndexBuilder indexBuilder
    )
    {
        var textFields = new JsonObject();
        var numericFields = new JsonObject();
        var booleanFields = new JsonObject();
        var datetimeFields = new JsonObject();
        var jsonFields = new JsonObject();

        foreach (var propertyName in attribute.Columns.Where(c => c != attribute.KeyField))
        {
            var property = entityType.FindProperty(propertyName);
            var propertyInfo = property?.PropertyInfo;
            if (propertyInfo == null)
                continue;

            var columnName = property.GetColumnName() ?? propertyName;

            var textAttr = propertyInfo.GetCustomAttribute<Bm25TextAttribute>();
            if (textAttr != null)
            {
                textFields[columnName] = Bm25StorageParameterBuilder.BuildTextField(
                    textAttr,
                    propertyName
                );
                continue;
            }
            var numAttr = propertyInfo.GetCustomAttribute<Bm25NumericAttribute>();
            if (numAttr != null)
            {
                numericFields[columnName] = Bm25StorageParameterBuilder.BuildNumericField(numAttr);
                continue;
            }
            var boolAttr = propertyInfo.GetCustomAttribute<Bm25BooleanAttribute>();
            if (boolAttr != null)
            {
                booleanFields[columnName] = Bm25StorageParameterBuilder.BuildBooleanField(boolAttr);
                continue;
            }
            var dtAttr = propertyInfo.GetCustomAttribute<Bm25DateTimeAttribute>();
            if (dtAttr != null)
            {
                datetimeFields[columnName] = Bm25StorageParameterBuilder.BuildDateTimeField(dtAttr);
                continue;
            }
            var jsonAttr = propertyInfo.GetCustomAttribute<Bm25JsonAttribute>();
            if (jsonAttr != null)
            {
                jsonFields[columnName] = Bm25StorageParameterBuilder.BuildJsonField(
                    jsonAttr,
                    propertyName
                );
            }
        }

        AddFieldGroupParameter(indexBuilder, "text_fields", textFields);
        AddFieldGroupParameter(indexBuilder, "numeric_fields", numericFields);
        AddFieldGroupParameter(indexBuilder, "boolean_fields", booleanFields);
        AddFieldGroupParameter(indexBuilder, "datetime_fields", datetimeFields);
        AddFieldGroupParameter(indexBuilder, "json_fields", jsonFields);
    }

    private static void AddFieldGroupParameter(
        IConventionIndexBuilder indexBuilder,
        string name,
        JsonObject group
    )
    {
        if (group.Count == 0)
            return;
        indexBuilder.HasAnnotation(
            $"Npgsql:StorageParameter:{name}",
            Bm25StorageParameterBuilder.Serialize(group),
            fromDataAnnotation: true
        );
    }
}
