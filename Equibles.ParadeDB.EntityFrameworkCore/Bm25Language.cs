namespace Equibles.ParadeDB.EntityFrameworkCore;

/// <summary>
/// Languages supported by pg_search's stemmer and stopwords token filters.
/// <see cref="Unspecified"/> means the property carries no language setting; the filter is not applied.
/// </summary>
public enum Bm25Language
{
    Unspecified = 0,
    Arabic,
    Czech,
    Danish,
    Dutch,
    English,
    Finnish,
    French,
    German,
    Greek,
    Hungarian,
    Italian,
    Norwegian,
    Polish,
    Portuguese,
    Romanian,
    Russian,
    Spanish,
    Swedish,
    Tamil,
    Turkish,
}
