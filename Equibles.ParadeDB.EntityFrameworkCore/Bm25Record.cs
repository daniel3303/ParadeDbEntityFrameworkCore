namespace Equibles.ParadeDB.EntityFrameworkCore;

/// <summary>
/// What term metadata pg_search stores in the index.
/// Position is required for phrase queries; Basic and Freq use less disk.
/// <see cref="Unspecified"/> means the property carries no record setting; pg_search uses its default.
/// </summary>
public enum Bm25Record
{
    Unspecified = 0,
    Basic,
    Freq,
    Position,
}
