using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Equibles.ParadeDB.EntityFrameworkCore.IntegrationTests;

public sealed class ParadeDbFixture : IAsyncLifetime {
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("paradedb/paradedb:latest")
        .WithDatabase("paradedb_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString { get; private set; } = default!;

    public async Task InitializeAsync() {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        await using var ctx = CreateDbContext();
        await ctx.Database.EnsureCreatedAsync();
        await SeedAsync(ctx);
    }

    public async Task DisposeAsync() {
        await _container.DisposeAsync();
    }

    public IntegrationDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<IntegrationDbContext>()
            .UseNpgsql(ConnectionString, n => n.UseParadeDb())
            .Options);

    private static async Task SeedAsync(IntegrationDbContext ctx) {
        ctx.Articles.AddRange(
            new Article {
                Title = "Introduction to neural networks",
                Content = "Deep learning models running on GPUs revolutionize machine learning. Neural networks are layered.",
                Category = "machine-learning",
                Rating = 5,
            },
            new Article {
                Title = "Transformer architectures",
                Content = "The attention mechanism powers modern language models. Transformers run efficiently on TPUs.",
                Category = "machine-learning",
                Rating = 4,
            },
            new Article {
                Title = "Quantum computing fundamentals",
                Content = "Qubits and entanglement enable parallel computation beyond classical bits.",
                Category = "physics",
                Rating = 3,
            },
            new Article {
                Title = "Cooking pasta perfectly",
                Content = "Salt the water generously and cook the pasta until al dente. Taste it as you go.",
                Category = "cooking",
                Rating = 5,
            });
        await ctx.SaveChangesAsync();
    }
}

[CollectionDefinition(nameof(ParadeDbCollection))]
public class ParadeDbCollection : ICollectionFixture<ParadeDbFixture> { }
