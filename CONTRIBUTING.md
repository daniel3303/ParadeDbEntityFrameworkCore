# Contributing

Thanks for considering a contribution. This is a small library; PRs that fix bugs, add tests, or extend the search API are welcome.

## Getting set up

```bash
git clone https://github.com/daniel3303/ParadeDbEntityFrameworkCore.git
cd ParadeDbEntityFrameworkCore
dotnet tool restore        # installs CSharpier locally
prek install -f            # installs git pre-commit hooks (https://github.com/j178/prek)
prek run --all-files       # one-off sweep
```

You'll also need Docker running locally — the integration test suite spins up a `paradedb/paradedb:latest` container via Testcontainers.

## Branching and commit style

- Branch from `master` using a prefix: `feature/`, `fix/`, `chore/`, `docs/`.
- Commits and PR titles follow [Conventional Commits](https://www.conventionalcommits.org). The PR-title workflow blocks merges that don't comply.
- Examples: `fix(json-query): ...`, `feat(translator): ...`, `test(integration): ...`, `docs(readme): ...`.

## Build, format, test

```bash
dotnet build -warnaserror                   # must produce 0 warnings
dotnet csharpier check .                    # must produce no diff (the lint job runs this in CI)
dotnet test                                 # unit + integration, all TFMs (net8/net9/net10)
```

The integration suite uses Testcontainers — Docker must be running.

## Adding a feature

1. Open an issue first if the scope isn't obvious — saves wasted effort.
2. Write the test before the production code (or alongside). For LINQ translator changes, both a unit test (asserts the generated SQL string) and an integration test (asserts the result set against real pg_search) are expected.
3. Keep PRs focused — one logical change per PR. Split unrelated changes into separate PRs.
4. Update `CHANGELOG.md` under the `## [Unreleased]` section.

## Reporting bugs

Use the [bug report template](.github/ISSUE_TEMPLATE/bug.yml). Include:

- Library version + .NET version + Npgsql.EFCore version + ParadeDB version.
- The LINQ query and the generated SQL (turn on `LogTo(Console.WriteLine)` on your `DbContext`).
- Expected vs. observed result.

## Security

Don't open public issues for vulnerabilities — see [SECURITY.md](SECURITY.md).
