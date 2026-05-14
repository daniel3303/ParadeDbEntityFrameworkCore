namespace Equibles.ParadeDB.EntityFrameworkCore;

/// <summary>
/// Tokenization strategy for a text or json column in a BM25 index.
/// Names map to pg_search tokenizer type strings.
/// <see cref="Unspecified"/> means the property carries no tokenizer setting; pg_search uses its default.
/// </summary>
public enum Bm25Tokenizer
{
    Unspecified = 0,
    Default,
    Whitespace,
    Raw,
    Keyword,
    SourceCode,
    Icu,
    Ngram,
    Regex,
    ChineseCompatible,
    ChineseLindera,
    JapaneseLindera,
    KoreanLindera,
    Jieba,
}
