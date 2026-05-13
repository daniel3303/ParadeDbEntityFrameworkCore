using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

public sealed class IntegrationDbContext(DbContextOptions<IntegrationDbContext> options) : DbContext(options) {
    public DbSet<Article> Articles => Set<Article>();
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
