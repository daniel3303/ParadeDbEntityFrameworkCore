namespace Equibles.ParadeDB.EntityFrameworkCore;

/// <summary>
/// Configures a json/jsonb column inside a BM25 index. Accepts the same text-field
/// settings as <see cref="Bm25TextAttribute"/> plus <see cref="ExpandDots"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class Bm25JsonAttribute : Attribute {
    public Bm25Tokenizer Tokenizer { get; set; }

    /// <summary>Required when <see cref="Tokenizer"/> is <see cref="Bm25Tokenizer.Ngram"/>.</summary>
    public int MinGram { get; set; }

    /// <summary>Required when <see cref="Tokenizer"/> is <see cref="Bm25Tokenizer.Ngram"/>.</summary>
    public int MaxGram { get; set; }

    /// <summary>Optional. Only valid when <see cref="Tokenizer"/> is <see cref="Bm25Tokenizer.Ngram"/>.</summary>
    public bool PrefixOnly { get; set; }

    /// <summary>Required when <see cref="Tokenizer"/> is <see cref="Bm25Tokenizer.Regex"/>.</summary>
    public string RegexPattern { get; set; }

    public Bm25Language Stemmer { get; set; }
    public Bm25Language StopwordsLanguage { get; set; }

    public bool Fast { get; set; }
    public Bm25Record Record { get; set; }
    public bool Indexed { get; set; } = true;
    public bool Fieldnorms { get; set; } = true;

    /// <summary>
    /// When true, dotted JSON keys such as "metadata.color" are expanded into nested objects
    /// for indexing. Defaults to pg_search's own default (false) when not set.
    /// </summary>
    public bool ExpandDots { get; set; }
}
