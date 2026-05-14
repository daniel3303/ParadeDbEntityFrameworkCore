namespace Equibles.ParadeDB.EntityFrameworkCore;

/// <summary>
/// Configures a numeric column inside a BM25 index.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class Bm25NumericAttribute : Attribute
{
    public bool Fast { get; set; }
    public bool Indexed { get; set; } = true;
}
