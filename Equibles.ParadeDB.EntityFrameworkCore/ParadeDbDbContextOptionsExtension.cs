using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Equibles.ParadeDB.EntityFrameworkCore;

public sealed class ParadeDbDbContextOptionsExtension : IDbContextOptionsExtension {
    public DbContextOptionsExtensionInfo Info => new ParadeDbExtensionInfo(this);

    public void ApplyServices(IServiceCollection services) {
        new EntityFrameworkRelationalServicesBuilder(services)
            .TryAdd<IConventionSetPlugin, ParadeDbConventionSetPlugin>();
    }

    public void Validate(IDbContextOptions options) { }

    private sealed class ParadeDbExtensionInfo : DbContextOptionsExtensionInfo {
        public ParadeDbExtensionInfo(IDbContextOptionsExtension extension) : base(extension) { }

        public override bool IsDatabaseProvider => false;
        public override string LogFragment => "using ParadeDB ";
        public override int GetServiceProviderHashCode() => 0;
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ParadeDbExtensionInfo;
        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) => debugInfo["ParadeDB:BM25"] = "1";
    }
}
