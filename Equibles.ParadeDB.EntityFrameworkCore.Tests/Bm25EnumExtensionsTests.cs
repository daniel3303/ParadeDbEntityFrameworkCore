using Equibles.ParadeDB.EntityFrameworkCore.Internal;

namespace Equibles.ParadeDB.EntityFrameworkCore.Tests;

/// <summary>
/// Tests for <see cref="Bm25EnumExtensions"/> — every enum value must map to its pg_search
/// string. <c>Unspecified</c> throws because the indexer expects an explicit mapping.
/// </summary>
public class Bm25EnumExtensionsTests {
    // ── Bm25Tokenizer ─────────────────────────────────────────────────

    [Theory]
    [InlineData(Bm25Tokenizer.Default, "default")]
    [InlineData(Bm25Tokenizer.Whitespace, "whitespace")]
    [InlineData(Bm25Tokenizer.Raw, "raw")]
    [InlineData(Bm25Tokenizer.Keyword, "keyword")]
    [InlineData(Bm25Tokenizer.SourceCode, "source_code")]
    [InlineData(Bm25Tokenizer.Icu, "icu")]
    [InlineData(Bm25Tokenizer.Ngram, "ngram")]
    [InlineData(Bm25Tokenizer.Regex, "regex")]
    [InlineData(Bm25Tokenizer.ChineseCompatible, "chinese_compatible")]
    [InlineData(Bm25Tokenizer.ChineseLindera, "chinese_lindera")]
    [InlineData(Bm25Tokenizer.JapaneseLindera, "japanese_lindera")]
    [InlineData(Bm25Tokenizer.KoreanLindera, "korean_lindera")]
    [InlineData(Bm25Tokenizer.Jieba, "jieba")]
    public void Tokenizer_maps_to_expected_pg_search_string(Bm25Tokenizer tokenizer, string expected) {
        Assert.Equal(expected, tokenizer.ToParadeDbString());
    }

    [Fact]
    public void Tokenizer_Unspecified_throws() {
        Assert.Throws<ArgumentException>(() => Bm25Tokenizer.Unspecified.ToParadeDbString());
    }

    [Fact]
    public void Tokenizer_out_of_range_throws() {
        var bogus = (Bm25Tokenizer)999;
        Assert.Throws<ArgumentOutOfRangeException>(() => bogus.ToParadeDbString());
    }

    // ── Bm25Language ──────────────────────────────────────────────────

    [Theory]
    [InlineData(Bm25Language.Arabic, "Arabic")]
    [InlineData(Bm25Language.Czech, "Czech")]
    [InlineData(Bm25Language.Danish, "Danish")]
    [InlineData(Bm25Language.Dutch, "Dutch")]
    [InlineData(Bm25Language.English, "English")]
    [InlineData(Bm25Language.Finnish, "Finnish")]
    [InlineData(Bm25Language.French, "French")]
    [InlineData(Bm25Language.German, "German")]
    [InlineData(Bm25Language.Greek, "Greek")]
    [InlineData(Bm25Language.Hungarian, "Hungarian")]
    [InlineData(Bm25Language.Italian, "Italian")]
    [InlineData(Bm25Language.Norwegian, "Norwegian")]
    [InlineData(Bm25Language.Polish, "Polish")]
    [InlineData(Bm25Language.Portuguese, "Portuguese")]
    [InlineData(Bm25Language.Romanian, "Romanian")]
    [InlineData(Bm25Language.Russian, "Russian")]
    [InlineData(Bm25Language.Spanish, "Spanish")]
    [InlineData(Bm25Language.Swedish, "Swedish")]
    [InlineData(Bm25Language.Tamil, "Tamil")]
    [InlineData(Bm25Language.Turkish, "Turkish")]
    public void Language_maps_to_expected_pg_search_string(Bm25Language language, string expected) {
        Assert.Equal(expected, language.ToParadeDbString());
    }

    [Fact]
    public void Language_Unspecified_throws() {
        Assert.Throws<ArgumentException>(() => Bm25Language.Unspecified.ToParadeDbString());
    }

    [Fact]
    public void Language_out_of_range_throws() {
        var bogus = (Bm25Language)999;
        Assert.Throws<ArgumentOutOfRangeException>(() => bogus.ToParadeDbString());
    }

    // ── Bm25Record ────────────────────────────────────────────────────

    [Theory]
    [InlineData(Bm25Record.Basic, "basic")]
    [InlineData(Bm25Record.Freq, "freq")]
    [InlineData(Bm25Record.Position, "position")]
    public void Record_maps_to_expected_pg_search_string(Bm25Record record, string expected) {
        Assert.Equal(expected, record.ToParadeDbString());
    }

    [Fact]
    public void Record_Unspecified_throws() {
        Assert.Throws<ArgumentException>(() => Bm25Record.Unspecified.ToParadeDbString());
    }

    [Fact]
    public void Record_out_of_range_throws() {
        var bogus = (Bm25Record)999;
        Assert.Throws<ArgumentOutOfRangeException>(() => bogus.ToParadeDbString());
    }
}
