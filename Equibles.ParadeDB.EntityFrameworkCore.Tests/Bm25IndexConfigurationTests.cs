using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.Tests;

public class Bm25IndexConfigurationTests {
    private static string GetCreateScript<TEntity>() where TEntity : class {
        using var ctx = new ConfigTestContext<TEntity>();
        return ctx.Database.GenerateCreateScript();
    }

    [Fact]
    public void EntityWithoutPerColumnAttributes_EmitsOnlyKeyField() {
        var sql = GetCreateScript<PlainEntity>();
        Assert.Contains("key_field='Id'", sql);
        Assert.DoesNotContain("text_fields=", sql);
        Assert.DoesNotContain("numeric_fields=", sql);
        Assert.DoesNotContain("boolean_fields=", sql);
        Assert.DoesNotContain("datetime_fields=", sql);
        Assert.DoesNotContain("json_fields=", sql);
    }

    [Fact]
    public void Bm25Text_WithEnglishStemmer_EmitsStemmerInTokenizer() {
        var sql = GetCreateScript<EnglishStemmerEntity>();
        Assert.Contains("""text_fields='{"Content":{"tokenizer":{"type":"default","stemmer":"English"}}}'""", sql);
    }

    [Fact]
    public void Bm25Text_WithRawTokenizer_EmitsRawType() {
        var sql = GetCreateScript<RawTokenizerEntity>();
        Assert.Contains("""text_fields='{"Slug":{"tokenizer":{"type":"raw"}}}'""", sql);
    }

    [Fact]
    public void Bm25Text_WithStopwordsLanguage_EmitsStopwordsKey() {
        var sql = GetCreateScript<StopwordsEntity>();
        Assert.Contains("\"stopwords_language\":\"French\"", sql);
    }

    [Fact]
    public void Bm25Text_WithNgramTokenizer_EmitsMinMaxGramAndPrefixOnly() {
        var sql = GetCreateScript<NgramEntity>();
        Assert.Contains("""text_fields='{"Content":{"tokenizer":{"type":"ngram","min_gram":2,"max_gram":4,"prefix_only":false}}}'""", sql);
    }

    [Fact]
    public void Bm25Text_WithNgramAndPrefixOnly_EmitsPrefixOnly() {
        var sql = GetCreateScript<NgramPrefixEntity>();
        Assert.Contains("\"prefix_only\":true", sql);
    }

    [Fact]
    public void Bm25Text_WithRegexTokenizer_EmitsPattern() {
        var sql = GetCreateScript<RegexEntity>();
        Assert.Contains("\"type\":\"regex\"", sql);
        Assert.Contains("\"pattern\":", sql);
    }

    [Fact]
    public void Bm25Text_WithFastTrue_EmitsFastKey() {
        var sql = GetCreateScript<FastTextEntity>();
        Assert.Contains("\"fast\":true", sql);
    }

    [Fact]
    public void Bm25Text_WithRecordPosition_EmitsRecordKey() {
        var sql = GetCreateScript<RecordPositionEntity>();
        Assert.Contains("\"record\":\"position\"", sql);
    }

    [Fact]
    public void Bm25Text_WithIndexedFalse_EmitsIndexedFalse() {
        var sql = GetCreateScript<NotIndexedTextEntity>();
        Assert.Contains("\"indexed\":false", sql);
    }

    [Fact]
    public void Bm25Text_WithFieldnormsFalse_EmitsFieldnormsFalse() {
        var sql = GetCreateScript<NoFieldnormsEntity>();
        Assert.Contains("\"fieldnorms\":false", sql);
    }

    [Fact]
    public void Bm25Numeric_WithFastTrue_EmitsNumericFields() {
        var sql = GetCreateScript<NumericEntity>();
        Assert.Contains("""numeric_fields='{"Rating":{"fast":true}}'""", sql);
    }

    [Fact]
    public void Bm25Boolean_WithFastTrue_EmitsBooleanFields() {
        var sql = GetCreateScript<BooleanEntity>();
        Assert.Contains("""boolean_fields='{"InStock":{"fast":true}}'""", sql);
    }

    [Fact]
    public void Bm25DateTime_WithFastTrue_EmitsDatetimeFields() {
        var sql = GetCreateScript<DateTimeEntity>();
        Assert.Contains("""datetime_fields='{"PublishedAt":{"fast":true}}'""", sql);
    }

    [Fact]
    public void Bm25Json_WithExpandDots_EmitsExpandDots() {
        var sql = GetCreateScript<JsonEntity>();
        Assert.Contains("json_fields=", sql);
        Assert.Contains("\"expand_dots\":true", sql);
    }

    [Fact]
    public void MixedFieldTypes_EmitSeparateStorageParameters() {
        var sql = GetCreateScript<MixedEntity>();
        Assert.Contains("text_fields=", sql);
        Assert.Contains("numeric_fields=", sql);
        Assert.Contains("boolean_fields=", sql);
    }

    [Fact]
    public void NgramParamWithoutNgramTokenizer_Throws() {
        var ex = Assert.Throws<InvalidOperationException>(() => GetCreateScript<InvalidNgramParamEntity>());
        Assert.Contains("MinGram/MaxGram/PrefixOnly require Tokenizer = Bm25Tokenizer.Ngram", ex.Message);
    }

    [Fact]
    public void NgramTokenizerWithoutMinMax_Throws() {
        var ex = Assert.Throws<InvalidOperationException>(() => GetCreateScript<NgramMissingMinMaxEntity>());
        Assert.Contains("Bm25Tokenizer.Ngram requires both MinGram and MaxGram", ex.Message);
    }

    [Fact]
    public void RegexTokenizerWithoutPattern_Throws() {
        var ex = Assert.Throws<InvalidOperationException>(() => GetCreateScript<RegexMissingPatternEntity>());
        Assert.Contains("Bm25Tokenizer.Regex requires a RegexPattern", ex.Message);
    }

    [Fact]
    public void RegexParamWithoutRegexTokenizer_Throws() {
        var ex = Assert.Throws<InvalidOperationException>(() => GetCreateScript<RegexParamOnNonRegexTokenizerEntity>());
        Assert.Contains("RegexPattern requires Tokenizer = Bm25Tokenizer.Regex", ex.Message);
    }

    [Fact]
    public void Bm25Numeric_WithIndexedFalse_EmitsIndexedFalse() {
        var sql = GetCreateScript<NotIndexedNumericEntity>();
        Assert.Contains("numeric_fields=", sql);
        Assert.Contains("\"indexed\":false", sql);
    }

    [Fact]
    public void Bm25Boolean_WithIndexedFalse_EmitsIndexedFalse() {
        var sql = GetCreateScript<NotIndexedBooleanEntity>();
        Assert.Contains("boolean_fields=", sql);
        Assert.Contains("\"indexed\":false", sql);
    }

    [Fact]
    public void Bm25DateTime_WithIndexedFalse_EmitsIndexedFalse() {
        var sql = GetCreateScript<NotIndexedDateTimeEntity>();
        Assert.Contains("datetime_fields=", sql);
        Assert.Contains("\"indexed\":false", sql);
    }

    [Fact]
    public void OrphanFieldAttributeWithoutBm25Index_Throws() {
        var ex = Assert.Throws<InvalidOperationException>(() => GetCreateScript<OrphanNoIndexEntity>());
        Assert.Contains("OrphanText", ex.Message);
        Assert.Contains("no [Bm25Index]", ex.Message);
    }

    [Fact]
    public void FieldAttributeOnPropertyNotInIndexColumns_Throws() {
        var ex = Assert.Throws<InvalidOperationException>(() => GetCreateScript<FieldAttrNotInIndexEntity>());
        Assert.Contains("Untracked", ex.Message);
        Assert.Contains("not listed in the [Bm25Index] columns", ex.Message);
    }
}

internal sealed class ConfigTestContext<TEntity> : DbContext where TEntity : class {
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseNpgsql("Host=localhost;Database=test", npgsql => npgsql.UseParadeDb());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<TEntity>();
    }
}

[Bm25Index(nameof(Id), nameof(Content))]
internal class PlainEntity {
    public int Id { get; set; }
    public string Content { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Content))]
internal class EnglishStemmerEntity {
    public int Id { get; set; }
    [Bm25Text(Stemmer = Bm25Language.English)]
    public string Content { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Slug))]
internal class RawTokenizerEntity {
    public int Id { get; set; }
    [Bm25Text(Tokenizer = Bm25Tokenizer.Raw)]
    public string Slug { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Content))]
internal class StopwordsEntity {
    public int Id { get; set; }
    [Bm25Text(StopwordsLanguage = Bm25Language.French)]
    public string Content { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Content))]
internal class NgramEntity {
    public int Id { get; set; }
    [Bm25Text(Tokenizer = Bm25Tokenizer.Ngram, MinGram = 2, MaxGram = 4)]
    public string Content { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Content))]
internal class NgramPrefixEntity {
    public int Id { get; set; }
    [Bm25Text(Tokenizer = Bm25Tokenizer.Ngram, MinGram = 2, MaxGram = 4, PrefixOnly = true)]
    public string Content { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Content))]
internal class RegexEntity {
    public int Id { get; set; }
    [Bm25Text(Tokenizer = Bm25Tokenizer.Regex, RegexPattern = "neuro.*")]
    public string Content { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Title))]
internal class FastTextEntity {
    public int Id { get; set; }
    [Bm25Text(Fast = true)]
    public string Title { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Content))]
internal class RecordPositionEntity {
    public int Id { get; set; }
    [Bm25Text(Record = Bm25Record.Position)]
    public string Content { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Content))]
internal class NotIndexedTextEntity {
    public int Id { get; set; }
    [Bm25Text(Indexed = false)]
    public string Content { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Content))]
internal class NoFieldnormsEntity {
    public int Id { get; set; }
    [Bm25Text(Fieldnorms = false)]
    public string Content { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Rating))]
internal class NumericEntity {
    public int Id { get; set; }
    [Bm25Numeric(Fast = true)]
    public int Rating { get; set; }
}

[Bm25Index(nameof(Id), nameof(InStock))]
internal class BooleanEntity {
    public int Id { get; set; }
    [Bm25Boolean(Fast = true)]
    public bool InStock { get; set; }
}

[Bm25Index(nameof(Id), nameof(PublishedAt))]
internal class DateTimeEntity {
    public int Id { get; set; }
    [Bm25DateTime(Fast = true)]
    public DateTime PublishedAt { get; set; }
}

[Bm25Index(nameof(Id), nameof(Metadata))]
internal class JsonEntity {
    public int Id { get; set; }
    [Bm25Json(ExpandDots = true)]
    public string Metadata { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Title), nameof(Rating), nameof(InStock))]
internal class MixedEntity {
    public int Id { get; set; }
    [Bm25Text(Fast = true)]
    public string Title { get; set; } = null!;
    [Bm25Numeric(Fast = true)]
    public int Rating { get; set; }
    [Bm25Boolean(Fast = true)]
    public bool InStock { get; set; }
}

[Bm25Index(nameof(Id), nameof(Content))]
internal class InvalidNgramParamEntity {
    public int Id { get; set; }
    [Bm25Text(MinGram = 2)]
    public string Content { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Content))]
internal class NgramMissingMinMaxEntity {
    public int Id { get; set; }
    [Bm25Text(Tokenizer = Bm25Tokenizer.Ngram)]
    public string Content { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Content))]
internal class RegexMissingPatternEntity {
    public int Id { get; set; }
    [Bm25Text(Tokenizer = Bm25Tokenizer.Regex)]
    public string Content { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Content))]
internal class RegexParamOnNonRegexTokenizerEntity {
    public int Id { get; set; }
    [Bm25Text(Tokenizer = Bm25Tokenizer.Default, RegexPattern = "neuro.*")]
    public string Content { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Rating))]
internal class NotIndexedNumericEntity {
    public int Id { get; set; }
    [Bm25Numeric(Indexed = false)]
    public int Rating { get; set; }
}

[Bm25Index(nameof(Id), nameof(InStock))]
internal class NotIndexedBooleanEntity {
    public int Id { get; set; }
    [Bm25Boolean(Indexed = false)]
    public bool InStock { get; set; }
}

[Bm25Index(nameof(Id), nameof(PublishedAt))]
internal class NotIndexedDateTimeEntity {
    public int Id { get; set; }
    [Bm25DateTime(Indexed = false)]
    public DateTime PublishedAt { get; set; }
}

internal class OrphanNoIndexEntity {
    public int Id { get; set; }
    [Bm25Text]
    public string OrphanText { get; set; } = null!;
}

[Bm25Index(nameof(Id), nameof(Indexed))]
internal class FieldAttrNotInIndexEntity {
    public int Id { get; set; }
    public string Indexed { get; set; } = null!;
    [Bm25Text]
    public string Untracked { get; set; } = null!;
}
