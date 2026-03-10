# Equibles.ParadeDB.EntityFrameworkCore

EF Core integration for [ParadeDB](https://www.paradedb.com/) `pg_search` — BM25 full-text search indexes on PostgreSQL.

Provides a `[Bm25Index]` attribute and `UseParadeDb()` extension method that automatically creates BM25 indexes via EF Core conventions and migrations. No raw SQL needed.

## Requirements

- PostgreSQL with the [pg_search](https://docs.paradedb.com/search/quickstart) extension installed
- [Npgsql.EntityFrameworkCore.PostgreSQL](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL) provider
- .NET 10+

## Installation

```bash
dotnet add package Equibles.ParadeDB.EntityFrameworkCore
```

## Usage

### 1. Enable ParadeDB in your DbContext

```csharp
services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.UseParadeDb()));
```

### 2. Add BM25 indexes to your entities

```csharp
using Equibles.ParadeDB.EntityFrameworkCore;

[Bm25Index(nameof(Id), nameof(Title), nameof(Content))]
public class Article
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
}
```

The first parameter is the key field (primary key), followed by the columns to index for full-text search.

### 3. Create a migration

```bash
dotnet ef migrations add AddBm25Index
dotnet ef database update
```

EF Core will generate the migration automatically, creating:
- The `pg_search` PostgreSQL extension
- A BM25 index on the specified columns with the correct `key_field` storage parameter

### How it works

The library hooks into EF Core's model finalization pipeline via `IConventionSetPlugin`. During model building, it:

1. Scans entity types for `[Bm25Index]` attributes
2. Creates database indexes with the `bm25` index method
3. Sets the `key_field` storage parameter (required by pg_search)
4. Registers the `pg_search` PostgreSQL extension

All of this is translated into standard EF Core migrations — no manual SQL required.

## License

MIT
