using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Equibles.ParadeDB.EntityFrameworkCore.Tests;

/// <summary>
/// Coverage for the <see cref="ParadeDbDbContextOptionsExtension"/> info members. These are
/// invoked by EF Core when describing the active provider/plugins; the LINQ tests don't
/// touch all of them so we exercise them explicitly here.
/// </summary>
public class ParadeDbDbContextOptionsExtensionTests {
    private static DbContextOptionsExtensionInfo Info() {
        using var ctx = new TestDbContext();
        var ext = ctx.GetService<IDbContextOptions>()
            .FindExtension<ParadeDbDbContextOptionsExtension>()!;
        return ext.Info;
    }

    [Fact]
    public void IsDatabaseProvider_is_false() {
        Assert.False(Info().IsDatabaseProvider);
    }

    [Fact]
    public void LogFragment_mentions_paradedb() {
        Assert.Contains("ParadeDB", Info().LogFragment, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetServiceProviderHashCode_is_stable() {
        var info = Info();
        Assert.Equal(info.GetServiceProviderHashCode(), info.GetServiceProviderHashCode());
    }

    [Fact]
    public void ShouldUseSameServiceProvider_is_true_for_same_extension_info_type() {
        var info = Info();
        Assert.True(info.ShouldUseSameServiceProvider(Info()));
    }

    [Fact]
    public void PopulateDebugInfo_writes_paradedb_marker() {
        var debug = new Dictionary<string, string>();
        Info().PopulateDebugInfo(debug);
        Assert.Equal("1", debug["ParadeDB:BM25"]);
    }
}
