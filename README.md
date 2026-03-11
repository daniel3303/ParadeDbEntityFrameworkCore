# Equibles.ParadeDB.EntityFrameworkCore

[![NuGet](https://img.shields.io/nuget/v/Equibles.ParadeDB.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Equibles.ParadeDB.EntityFrameworkCore)

EF Core integration for [ParadeDB](https://www.paradedb.com/) `pg_search` — BM25 full-text search indexes on PostgreSQL.

Provides a `[Bm25Index]` attribute for automatic index creation via migrations, and LINQ-friendly query methods for BM25 search, fuzzy matching, boosting, scoring, snippets, and more. No raw SQL needed.

## Requirements

- PostgreSQL with the [pg_search](https://docs.paradedb.com/search/quickstart) extension installed
- [Npgsql.EntityFrameworkCore.PostgreSQL](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL) provider
- .NET 10+

## Installation

```bash
dotnet add package Equibles.ParadeDB.EntityFrameworkCore
```

## Setup

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

The first parameter is the **key field** (required by pg_search to identify rows for scoring via `pdb.score()`), followed by the columns to index for full-text search. The key field is not searchable — it's only used internally by ParadeDB.

### 3. Create a migration

```bash
dotnet ef migrations add AddBm25Index
dotnet ef database update
```

EF Core will generate the migration automatically, creating:
- The `pg_search` PostgreSQL extension
- A BM25 index on the specified columns with the correct `key_field` storage parameter

## Querying

### Basic Search

Uses the `|||` operator — matches documents containing **any** of the query terms (OR).

```csharp
var results = await dbContext.Articles
    .Where(a => EF.Functions.Matches(a.Content, "machine learning"))
    .ToListAsync();
```

```sql
SELECT * FROM "Articles" WHERE "Content" ||| 'machine learning'
```

### Conjunction Search

Uses the `&&&` operator — matches documents containing **all** of the query terms (AND).

```csharp
var results = await dbContext.Articles
    .Where(a => EF.Functions.MatchesAll(a.Content, "machine learning"))
    .ToListAsync();
```

```sql
SELECT * FROM "Articles" WHERE "Content" &&& 'machine learning'
```

### Phrase Search

Matches terms in exact order. Slop allows N words between terms or transposition of adjacent terms.

```csharp
// Exact phrase
var results = await dbContext.Articles
    .Where(a => EF.Functions.MatchesPhrase(a.Content, "neural networks"))
    .ToListAsync();

// Phrase with slop — allows up to 2 words between terms
var results = await dbContext.Articles
    .Where(a => EF.Functions.MatchesPhrase(a.Content, "neural networks", 2))
    .ToListAsync();
```

```sql
-- Exact phrase
SELECT * FROM "Articles" WHERE "Content" ### 'neural networks'

-- With slop
SELECT * FROM "Articles" WHERE "Content" ### 'neural networks'::pdb.slop(2)
```

### Term Search

Exact token match — the query is NOT tokenized (no stemming/lowering). Most tokenizers lowercase, so search lowercase.

```csharp
// Single term
var results = await dbContext.Articles
    .Where(a => EF.Functions.MatchesTerm(a.Content, "gpu"))
    .ToListAsync();

// Multiple terms (matches any)
var results = await dbContext.Articles
    .Where(a => EF.Functions.MatchesTermSet(a.Content, "gpu", "tpu", "npu"))
    .ToListAsync();
```

```sql
SELECT * FROM "Articles" WHERE "Content" === 'gpu'
SELECT * FROM "Articles" WHERE "Content" === ARRAY['gpu', 'tpu', 'npu']
```

### Fuzzy Search (Levenshtein Distance)

Tolerates typos by allowing up to N single-character edits (insertions, deletions, substitutions). Max distance is 2.

- `prefix`: exempts the initial substring from the edit distance
- `transpositionCostOne`: counts swapping two adjacent characters as one edit instead of two

```csharp
// Basic fuzzy (distance 2)
var results = await dbContext.Articles
    .Where(a => EF.Functions.MatchesFuzzy(a.Content, "machin", 2))
    .ToListAsync();

// Fuzzy with all options
var results = await dbContext.Articles
    .Where(a => EF.Functions.MatchesFuzzy(a.Content, "machin", 2, true, false))
    .ToListAsync();

// Fuzzy AND match
var results = await dbContext.Articles
    .Where(a => EF.Functions.MatchesAllFuzzy(a.Content, "machin lerning", 2))
    .ToListAsync();

// Fuzzy term match
var results = await dbContext.Articles
    .Where(a => EF.Functions.MatchesTermFuzzy(a.Content, "machin", 1))
    .ToListAsync();
```

```sql
SELECT * FROM "Articles" WHERE "Content" ||| 'machin'::pdb.fuzzy(2)
SELECT * FROM "Articles" WHERE "Content" ||| 'machin'::pdb.fuzzy(2, true, false)
SELECT * FROM "Articles" WHERE "Content" &&& 'machin lerning'::pdb.fuzzy(2)
SELECT * FROM "Articles" WHERE "Content" === 'machin'::pdb.fuzzy(1)
```

### Boost

Increases the BM25 relevance weight of a specific search term. Higher boost = higher score for matches on that term. Factor range: -2048 to 2048.

```csharp
// Boosted OR match
var results = await dbContext.Articles
    .Where(a => EF.Functions.MatchesBoosted(a.Title, "transformers", 2.0))
    .ToListAsync();

// Boosted AND match
var results = await dbContext.Articles
    .Where(a => EF.Functions.MatchesAllBoosted(a.Content, "attention mechanism", 1.5))
    .ToListAsync();

// Combined fuzzy + boost
var results = await dbContext.Articles
    .Where(a => EF.Functions.MatchesFuzzyBoosted(a.Title, "transfomers", 2, 2.0))
    .ToListAsync();
```

```sql
SELECT * FROM "Articles" WHERE "Title" ||| 'transformers'::pdb.boost(2)
SELECT * FROM "Articles" WHERE "Content" &&& 'attention mechanism'::pdb.boost(1.5)
SELECT * FROM "Articles" WHERE "Title" ||| 'transfomers'::pdb.fuzzy(2)::pdb.boost(2)
```

### BM25 Scoring

BM25 (Best Matching 25) ranks documents by relevance considering term frequency, inverse document frequency, and document length.

```csharp
var results = await dbContext.Articles
    .Where(a => EF.Functions.Matches(a.Content, "deep learning"))
    .Select(a => new
    {
        a.Title,
        Score = EF.Functions.Score(a.Id)
    })
    .OrderByDescending(a => a.Score)
    .Take(10)
    .ToListAsync();
```

```sql
SELECT "Title", pdb.score("Id") AS "Score"
FROM "Articles"
WHERE "Content" ||| 'deep learning'
ORDER BY pdb.score("Id") DESC
LIMIT 10
```

### Snippets

Returns text excerpts with matched terms highlighted using configurable HTML tags.

```csharp
// Basic snippet (default highlighting)
var results = await dbContext.Articles
    .Where(a => EF.Functions.Matches(a.Content, "neural networks"))
    .Select(a => new
    {
        a.Title,
        Snippet = EF.Functions.Snippet(a.Content)
    })
    .ToListAsync();

// Parameterized snippet (custom tags and length)
var results = await dbContext.Articles
    .Where(a => EF.Functions.Matches(a.Content, "neural networks"))
    .Select(a => new
    {
        a.Title,
        Snippet = EF.Functions.Snippet(a.Content, "<b>", "</b>", 100)
    })
    .ToListAsync();

// Multiple snippets
var results = await dbContext.Articles
    .Where(a => EF.Functions.Matches(a.Content, "neural networks"))
    .Select(a => new
    {
        a.Title,
        Snippets = EF.Functions.Snippets(a.Content, 15, 5, 0)
    })
    .ToListAsync();
```

```sql
SELECT "Title", pdb.snippet("Content") AS "Snippet" FROM "Articles" WHERE ...
SELECT "Title", pdb.snippet("Content", start_tag => '<b>', end_tag => '</b>', max_num_chars => 100) AS "Snippet" FROM "Articles" WHERE ...
SELECT "Title", pdb.snippets("Content", max_num_chars => 15, "limit" => 5, "offset" => 0) AS "Snippets" FROM "Articles" WHERE ...
```

### Parse Query (Tantivy Syntax)

Full query parser supporting `field:value`, boolean operators (AND/OR/NOT), ranges (`rating:>3`), and wildcards.

- `lenient`: ignores syntax errors
- `conjunctionMode`: defaults terms to AND instead of OR

```csharp
// Basic parse query
var results = await dbContext.Articles
    .Where(a => EF.Functions.Parse(a.Id, "title:transformers AND content:attention"))
    .ToListAsync();

// With options
var results = await dbContext.Articles
    .Where(a => EF.Functions.Parse(a.Id, "transformers attention", true, true))
    .ToListAsync();
```

```sql
SELECT * FROM "Articles" WHERE "Id" @@@ pdb.parse('title:transformers AND content:attention')
SELECT * FROM "Articles" WHERE "Id" @@@ pdb.parse('transformers attention', lenient => TRUE, conjunction_mode => TRUE)
```

### Regex Search

Matches indexed tokens against a regular expression (Rust regex syntax).

```csharp
var results = await dbContext.Articles
    .Where(a => EF.Functions.Regex(a.Content, "neuro.*"))
    .ToListAsync();
```

```sql
SELECT * FROM "Articles" WHERE "Content" @@@ pdb.regex('neuro.*')
```

### Phrase Prefix

Matches a phrase where the last term is treated as a prefix — useful for autocomplete/type-ahead.

```csharp
// Basic phrase prefix
var results = await dbContext.Articles
    .Where(a => EF.Functions.PhrasePrefix(a.Content, "running", "sh"))
    .ToListAsync();

// With max expansions
var results = await dbContext.Articles
    .Where(a => EF.Functions.PhrasePrefix(a.Content, 10, "running", "sh"))
    .ToListAsync();
```

```sql
SELECT * FROM "Articles" WHERE "Content" @@@ pdb.phrase_prefix(ARRAY['running', 'sh'])
SELECT * FROM "Articles" WHERE "Content" @@@ pdb.phrase_prefix(ARRAY['running', 'sh'], max_expansions => 10)
```

### More Like This

Finds documents similar to a given document by analyzing its indexed terms.

```csharp
// Find similar to document with ID 3
var results = await dbContext.Articles
    .Where(a => EF.Functions.MoreLikeThis(a.Id, 3))
    .ToListAsync();

// Restrict similarity analysis to specific fields
var results = await dbContext.Articles
    .Where(a => EF.Functions.MoreLikeThis(a.Id, 3, "description"))
    .ToListAsync();
```

```sql
SELECT * FROM "Articles" WHERE "Id" @@@ pdb.more_like_this(3)
SELECT * FROM "Articles" WHERE "Id" @@@ pdb.more_like_this(3, ARRAY['description'])
```

### Combining with LINQ

All search methods compose naturally with standard LINQ:

```csharp
var results = await dbContext.Articles
    .Where(a => EF.Functions.MatchesFuzzy(a.Content, "transfomers", 2)
                && a.CreatedAt > DateTime.UtcNow.AddMonths(-6))
    .Select(a => new
    {
        a.Title,
        Snippet = EF.Functions.Snippet(a.Content, "<mark>", "</mark>", 200),
        Score = EF.Functions.Score(a.Id)
    })
    .OrderByDescending(a => a.Score)
    .Take(20)
    .ToListAsync();
```

## How it works

### Index creation

The library hooks into EF Core's model finalization pipeline via `IConventionSetPlugin`. During model building, it:

1. Scans entity types for `[Bm25Index]` attributes
2. Creates database indexes with the `bm25` index method
3. Sets the `key_field` storage parameter (required by pg_search)
4. Registers the `pg_search` PostgreSQL extension

All of this is translated into standard EF Core migrations — no manual SQL required.

### Query translation

LINQ methods on `EF.Functions` are translated to SQL via `IMethodCallTranslatorPlugin`:

| C# Method | SQL |
|-----------|-----|
| `Matches(col, "q")` | `col \|\|\| 'q'` |
| `MatchesAll(col, "q")` | `col &&& 'q'` |
| `MatchesPhrase(col, "q")` | `col ### 'q'` |
| `MatchesPhrase(col, "q", 2)` | `col ### 'q'::pdb.slop(2)` |
| `MatchesTerm(col, "q")` | `col === 'q'` |
| `MatchesTermSet(col, "a", "b")` | `col === ARRAY['a', 'b']` |
| `MatchesFuzzy(col, "q", 2)` | `col \|\|\| 'q'::pdb.fuzzy(2)` |
| `MatchesFuzzy(col, "q", 2, true, false)` | `col \|\|\| 'q'::pdb.fuzzy(2, true, false)` |
| `MatchesAllFuzzy(col, "q", 2)` | `col &&& 'q'::pdb.fuzzy(2)` |
| `MatchesTermFuzzy(col, "q", 1)` | `col === 'q'::pdb.fuzzy(1)` |
| `MatchesBoosted(col, "q", 2.0)` | `col \|\|\| 'q'::pdb.boost(2)` |
| `MatchesAllBoosted(col, "q", 2.0)` | `col &&& 'q'::pdb.boost(2)` |
| `MatchesFuzzyBoosted(col, "q", 2, 2.0)` | `col \|\|\| 'q'::pdb.fuzzy(2)::pdb.boost(2)` |
| `MatchesAllFuzzyBoosted(col, "q", 2, 2.0)` | `col &&& 'q'::pdb.fuzzy(2)::pdb.boost(2)` |
| `Score(id)` | `pdb.score(id)` |
| `Snippet(col)` | `pdb.snippet(col)` |
| `Snippet(col, "<b>", "</b>", 100)` | `pdb.snippet(col, start_tag => '<b>', end_tag => '</b>', max_num_chars => 100)` |
| `Snippets(col, 15, 5, 0)` | `pdb.snippets(col, max_num_chars => 15, "limit" => 5, "offset" => 0)` |
| `Parse(id, "desc:shoes")` | `id @@@ pdb.parse('desc:shoes')` |
| `Parse(id, "q", true, true)` | `id @@@ pdb.parse('q', lenient => true, conjunction_mode => true)` |
| `Regex(col, "key.*")` | `col @@@ pdb.regex('key.*')` |
| `PhrasePrefix(col, "running", "sh")` | `col @@@ pdb.phrase_prefix(ARRAY['running', 'sh'])` |
| `PhrasePrefix(col, 10, "running", "sh")` | `col @@@ pdb.phrase_prefix(ARRAY['running', 'sh'], max_expansions => 10)` |
| `MoreLikeThis(id, 3)` | `id @@@ pdb.more_like_this(3)` |
| `MoreLikeThis(id, 3, "description")` | `id @@@ pdb.more_like_this(3, ARRAY['description'])` |

## License

[MIT](LICENSE)

## Author

Daniel Oliveira

[![Website](https://img.shields.io/badge/Website-FF6B6B?style=for-the-badge&logo=safari&logoColor=white)](https://danielapoliveira.com/)
[![X](https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/daniel_not_nerd)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://www.linkedin.com/in/daniel-ap-oliveira/)
