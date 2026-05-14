# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed

- `ParadeDbJsonQuery.FuzzyTerm`, `Parse`, and `Match` (4-arg) now emit their boolean option keys unconditionally so `transpositionCostOne: false` (and other `false` flags) actually disable the option instead of falling back to pg_search's default ([#60](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/60), closes [#47](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/issues/47)).
- `ParadeDbJsonQuery.Range` now emits both `lower_bound` and `upper_bound` keys with `null` values for unbounded sides instead of omitting the key, which previously triggered a `missing field upper_bound` Rust panic in pg_search ([#62](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/62), closes [#51](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/issues/51)).
- Removed the 1-arg `ParadeDbJsonQuery.Match(value)` overload — pg_search's JSON `match` variant requires a `field` key, so the overload was unreachable at runtime ([#61](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/61), closes [#56](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/issues/56)). **Breaking**: callers using `ParadeDbJsonQuery.Match("term")` must switch to the 2-arg overload `ParadeDbJsonQuery.Match("term", "fieldName")`.

### Changed

- Codebase reformatted with CSharpier 1.2.6 (Allman braces). Authorship is preserved via `.git-blame-ignore-revs` ([#64](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/64), [#65](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/65)).
- Bumped `Testcontainers.PostgreSql` from 4.7.0 to 4.11.0 and migrated to the new image-arg constructor ([#74](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/74)).

### Internal / CI

- CI split into a `lint` job (`dotnet csharpier check` + `dotnet build -warnaserror`) and a `build` matrix across net8/net9/net10, with concurrency cancellation and Codecov upload gated to the net10 matrix entry ([#66](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/66)).
- IntegrationTests now multi-targets net8/net9/net10 with conditional Npgsql.EFCore package references, matching the library's own pattern ([#73](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/73)).
- Added pre-commit hook bundle (`.pre-commit-config.yaml`) covering line endings, EOF, large files, YAML/JSON, private keys, markdownlint, codespell, and a local CSharpier hook ([#67](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/67)).
- Added PR title linting via `amannn/action-semantic-pull-request@v5` ([#68](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/68)).
- Added Dependabot with grouped weekly NuGet + GitHub Actions updates, limit 3 per ecosystem ([#69](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/69)).
- Added `SECURITY.md`, `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md` (Contributor Covenant v2.1) ([#80](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/80), [#83](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/83)).
- Added issue forms (bug, feature) and PR template ([#82](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/82)).
- Added README badges for NuGet downloads, license, and .NET versions ([#79](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/79)).
- Documented upstream ParadeDB issue/PR for PascalCase column-quoting workaround ([#55](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/55)).
- Coverage expanded with regression and edge-case integration tests (PRs [#39](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/39)–[#59](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/59)).

## [2.0.1] — 2026-05-13

### Fixed

- 5-arg fuzzy overloads (`MatchesFuzzy`, `MatchesAllFuzzy`, `MatchesTermFuzzy` with `prefix` and `transpositionCostOne` flags) now route through `pdb.match` / `pdb.fuzzy_term` function calls instead of typmod suffixes, because `pdb.fuzzy(...)` typmod accepts only a single int (PR [#33](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/33)).
- `PhrasePrefix` translation now emits the correct named argument `max_expansion` (singular), not `max_expansions` (PR [#32](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/32)).
- `ParadeDbJsonQuery.MoreLikeThis` now emits `key_value` instead of `document_id` to match the JSON shape pg_search expects (PR [#31](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/31)).

## [2.0.0] — 2026-05-13

### Added

- Per-column BM25 index configuration via attributes (`[Bm25Text]`, `[Bm25Numeric]`, `[Bm25Boolean]`, `[Bm25DateTime]`, `[Bm25Json]`) emitting `text_fields` / `numeric_fields` / `boolean_fields` / `datetime_fields` / `json_fields` storage parameters (PR [#3](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/3)).
- Multi-targeting: package now ships for net8.0, net9.0, net10.0 (PR [#2](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/2)).
- Integration test project against a real ParadeDB container via Testcontainers (PR [#4](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/4)).

### Changed

- Reframed PascalCase column-name caveat in README as a narrow MoreLikeThis / JsonSearch limitation rather than a global restriction (PR [#5](https://github.com/daniel3303/ParadeDbEntityFrameworkCore/pull/5)).

## [1.1.0] — 2026-03-17

### Added

- Inline boolean builder overload for `JsonSearch` for ergonomic must/should/must_not composition.

## [1.0.0] — 2026-03-11

### Added

- Query translation tests verifying SQL generation without a live database.
- First stable release of the LINQ-to-pg_search translator surface.

## [0.3.0] — 2026-03-10

### Added

- LINQ query methods for BM25 search (`Matches`, `MatchesAll`, fuzzy variants), scoring (`Score`), and snippets (`Snippet`, `Snippets`).

## [0.2.0] — 2026-03-10

### Added

- NuGet package icon and basic packaging metadata.

## [0.1.0] — 2026-03-10

### Added

- Initial release: `[Bm25Index]` attribute + EF Core convention to emit `pg_search` BM25 indexes via migrations.

[Unreleased]: https://github.com/daniel3303/ParadeDbEntityFrameworkCore/compare/v2.0.1...HEAD
[2.0.1]: https://github.com/daniel3303/ParadeDbEntityFrameworkCore/compare/v2.0.0...v2.0.1
[2.0.0]: https://github.com/daniel3303/ParadeDbEntityFrameworkCore/compare/v1.1.0...v2.0.0
[1.1.0]: https://github.com/daniel3303/ParadeDbEntityFrameworkCore/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/daniel3303/ParadeDbEntityFrameworkCore/compare/v0.3.0...v1.0.0
[0.3.0]: https://github.com/daniel3303/ParadeDbEntityFrameworkCore/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/daniel3303/ParadeDbEntityFrameworkCore/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/daniel3303/ParadeDbEntityFrameworkCore/releases/tag/v0.1.0
