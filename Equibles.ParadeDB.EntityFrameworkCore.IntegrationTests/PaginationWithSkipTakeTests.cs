using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

[Collection(nameof(ParadeDbCollection))]
public class PaginationWithSkipTakeTests(ParadeDbFixture fixture)
{
    // Verifies that EF Core .Skip()/.Take() compose with the @@@ predicate and pdb.score()
    // ordering produced by ParadeDB through ParadeDbQuerySqlGenerator. No existing integration
    // test exercises OFFSET; a regression in the SQL generator's LIMIT/OFFSET handling alongside
    // the named-arg score function would only surface for paginated search — the most common
    // real-world shape — and slip past the single-page tests.
    [Fact]
    public async Task Search_PaginatedWithSkipAndTake_ReturnsSliceOfFullOrdering()
    {
        await using var ctx = fixture.CreateDbContext();

        var query = ctx.Articles.Where(a =>
            EF.Functions.Matches(a.Content, "neural OR transformer OR quantum OR cooking")
        );

        var fullOrdering = await query
            .OrderByDescending(a => EF.Functions.Score(a.Id))
            .ThenBy(a => a.Id)
            .Select(a => a.Title)
            .ToListAsync();
        var paginated = await query
            .OrderByDescending(a => EF.Functions.Score(a.Id))
            .ThenBy(a => a.Id)
            .Skip(1)
            .Take(2)
            .Select(a => a.Title)
            .ToListAsync();

        Assert.True(
            fullOrdering.Count >= 3,
            $"Need ≥3 matches for Skip(1).Take(2) to be observable; got {fullOrdering.Count}."
        );
        Assert.Equal(2, paginated.Count);
        Assert.Equal(fullOrdering.Skip(1).Take(2).ToList(), paginated);
    }
}
