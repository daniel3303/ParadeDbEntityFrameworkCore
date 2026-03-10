using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Equibles.ParadeDB.EntityFrameworkCore;

public static class ParadeDbDbContextOptionsBuilderExtensions {
    public static NpgsqlDbContextOptionsBuilder UseParadeDb(this NpgsqlDbContextOptionsBuilder npgsqlBuilder) {
        var builder = ((IRelationalDbContextOptionsBuilderInfrastructure)npgsqlBuilder).OptionsBuilder;

        var extension = builder.Options.FindExtension<ParadeDbDbContextOptionsExtension>()
                        ?? new ParadeDbDbContextOptionsExtension();

        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);

        return npgsqlBuilder;
    }
}
