using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Equibles.ParadeDB.EntityFrameworkCore;

public sealed class ParadeDbConventionSetPlugin : IConventionSetPlugin {
    public ConventionSet ModifyConventions(ConventionSet conventionSet) {
        conventionSet.ModelFinalizingConventions.Add(new ParadeDbModelFinalizingConvention());
        return conventionSet;
    }
}
