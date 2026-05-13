namespace Equibles.ParadeDB.EntityFrameworkCore.Internal;

internal static class Bm25EnumExtensions {
    public static string ToParadeDbString(this Bm25Tokenizer tokenizer) => tokenizer switch {
        Bm25Tokenizer.Unspecified => throw new ArgumentException("Bm25Tokenizer.Unspecified has no pg_search representation.", nameof(tokenizer)),
        Bm25Tokenizer.Default => "default",
        Bm25Tokenizer.Whitespace => "whitespace",
        Bm25Tokenizer.Raw => "raw",
        Bm25Tokenizer.Keyword => "keyword",
        Bm25Tokenizer.SourceCode => "source_code",
        Bm25Tokenizer.Icu => "icu",
        Bm25Tokenizer.Ngram => "ngram",
        Bm25Tokenizer.Regex => "regex",
        Bm25Tokenizer.ChineseCompatible => "chinese_compatible",
        Bm25Tokenizer.ChineseLindera => "chinese_lindera",
        Bm25Tokenizer.JapaneseLindera => "japanese_lindera",
        Bm25Tokenizer.KoreanLindera => "korean_lindera",
        Bm25Tokenizer.Jieba => "jieba",
        _ => throw new ArgumentOutOfRangeException(nameof(tokenizer), tokenizer, null),
    };

    public static string ToParadeDbString(this Bm25Language language) => language switch {
        Bm25Language.Unspecified => throw new ArgumentException("Bm25Language.Unspecified has no pg_search representation.", nameof(language)),
        Bm25Language.Arabic => "Arabic",
        Bm25Language.Czech => "Czech",
        Bm25Language.Danish => "Danish",
        Bm25Language.Dutch => "Dutch",
        Bm25Language.English => "English",
        Bm25Language.Finnish => "Finnish",
        Bm25Language.French => "French",
        Bm25Language.German => "German",
        Bm25Language.Greek => "Greek",
        Bm25Language.Hungarian => "Hungarian",
        Bm25Language.Italian => "Italian",
        Bm25Language.Norwegian => "Norwegian",
        Bm25Language.Polish => "Polish",
        Bm25Language.Portuguese => "Portuguese",
        Bm25Language.Romanian => "Romanian",
        Bm25Language.Russian => "Russian",
        Bm25Language.Spanish => "Spanish",
        Bm25Language.Swedish => "Swedish",
        Bm25Language.Tamil => "Tamil",
        Bm25Language.Turkish => "Turkish",
        _ => throw new ArgumentOutOfRangeException(nameof(language), language, null),
    };

    public static string ToParadeDbString(this Bm25Record record) => record switch {
        Bm25Record.Unspecified => throw new ArgumentException("Bm25Record.Unspecified has no pg_search representation.", nameof(record)),
        Bm25Record.Basic => "basic",
        Bm25Record.Freq => "freq",
        Bm25Record.Position => "position",
        _ => throw new ArgumentOutOfRangeException(nameof(record), record, null),
    };
}
