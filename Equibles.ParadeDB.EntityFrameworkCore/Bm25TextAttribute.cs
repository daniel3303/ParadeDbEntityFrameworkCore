namespace Equibles.ParadeDB.EntityFrameworkCore;

/// <summary>
/// Configures a text column inside a BM25 index. The property must also be listed
/// in the entity's <see cref="Bm25IndexAttribute"/> column set.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class Bm25TextAttribute : Attribute
{
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
}
