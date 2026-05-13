using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

public sealed class IntegrationDbContext(DbContextOptions<IntegrationDbContext> options) : DbContext(options) {
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<KeywordRecord> KeywordRecords => Set<KeywordRecord>();
    public DbSet<NgramRecord> NgramRecords => Set<NgramRecord>();
    public DbSet<IcuRecord> IcuRecords => Set<IcuRecord>();
    public DbSet<SourceCodeRecord> SourceCodeRecords => Set<SourceCodeRecord>();
    public DbSet<RegexRecord> RegexRecords => Set<RegexRecord>();
    public DbSet<GermanArticle> GermanArticles => Set<GermanArticle>();
}

[Table("articles")]
[Bm25Index(nameof(Id), nameof(Title), nameof(Content), nameof(Category), nameof(Rating))]
public sealed class Article {
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    [Bm25Text(Stemmer = Bm25Language.English, Fast = true)]
    public string Title { get; set; } = null!;

    [Column("content")]
    [Bm25Text(Stemmer = Bm25Language.English, Record = Bm25Record.Position)]
    public string Content { get; set; } = null!;

    [Column("category")]
    [Bm25Text(Tokenizer = Bm25Tokenizer.Raw, Fast = true)]
    public string Category { get; set; } = null!;

    [Column("rating")]
    [Bm25Numeric(Fast = true)]
    public int Rating { get; set; }
}

[Table("products")]
[Bm25Index(nameof(Id), nameof(Name), nameof(InStock), nameof(ReleasedAt), nameof(Specs))]
public sealed class Product {
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [Bm25Text(Stemmer = Bm25Language.English, Fast = true)]
    public string Name { get; set; } = null!;

    [Column("in_stock")]
    [Bm25Boolean(Fast = true)]
    public bool InStock { get; set; }

    [Column("released_at")]
    [Bm25DateTime(Fast = true)]
    public DateTime ReleasedAt { get; set; }

    [Column("specs", TypeName = "jsonb")]
    [Bm25Json(ExpandDots = true)]
    public string Specs { get; set; } = null!;
}

[Table("keyword_records")]
[Bm25Index(nameof(Id), nameof(Code))]
public sealed class KeywordRecord {
    [Column("id")]
    public int Id { get; set; }

    [Column("code")]
    [Bm25Text(Tokenizer = Bm25Tokenizer.Keyword)]
    public string Code { get; set; } = null!;
}

[Table("ngram_records")]
[Bm25Index(nameof(Id), nameof(Body))]
public sealed class NgramRecord {
    [Column("id")]
    public int Id { get; set; }

    [Column("body")]
    [Bm25Text(Tokenizer = Bm25Tokenizer.Ngram, MinGram = 3, MaxGram = 5)]
    public string Body { get; set; } = null!;
}

[Table("icu_records")]
[Bm25Index(nameof(Id), nameof(Body))]
public sealed class IcuRecord {
    [Column("id")]
    public int Id { get; set; }

    [Column("body")]
    [Bm25Text(Tokenizer = Bm25Tokenizer.Icu)]
    public string Body { get; set; } = null!;
}

[Table("source_code_records")]
[Bm25Index(nameof(Id), nameof(Snippet))]
public sealed class SourceCodeRecord {
    [Column("id")]
    public int Id { get; set; }

    [Column("snippet")]
    [Bm25Text(Tokenizer = Bm25Tokenizer.SourceCode)]
    public string Snippet { get; set; } = null!;
}

[Table("regex_records")]
[Bm25Index(nameof(Id), nameof(Body))]
public sealed class RegexRecord {
    [Column("id")]
    public int Id { get; set; }

    [Column("body")]
    [Bm25Text(Tokenizer = Bm25Tokenizer.Regex, RegexPattern = "\\w+")]
    public string Body { get; set; } = null!;
}

[Table("german_articles")]
[Bm25Index(nameof(Id), nameof(Content))]
public sealed class GermanArticle {
    [Column("id")]
    public int Id { get; set; }

    [Column("content")]
    [Bm25Text(Stemmer = Bm25Language.German)]
    public string Content { get; set; } = null!;
}
