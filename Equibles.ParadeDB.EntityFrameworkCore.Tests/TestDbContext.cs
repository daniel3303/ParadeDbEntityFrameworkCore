using Microsoft.EntityFrameworkCore;

namespace Equibles.ParadeDB.EntityFrameworkCore.Tests;

[Bm25Index(nameof(Id), nameof(Title), nameof(Content))]
public class Article {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

[Bm25Index(nameof(Id), nameof(Content), nameof(DocumentId), nameof(DocumentType))]
public class Chunk {
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public Guid DocumentId { get; set; }
    public int DocumentType { get; set; }
}

public class TestDbContext : DbContext {
    public DbSet<Article> Articles { get; set; } = null!;
    public DbSet<Chunk> Chunks { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseNpgsql("Host=localhost;Database=test", npgsql => npgsql.UseParadeDb());
    }
}
