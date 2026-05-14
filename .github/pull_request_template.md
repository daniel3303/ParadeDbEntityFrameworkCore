<!-- markdownlint-disable MD041 -->
<!--
Title: follow Conventional Commits — `fix:`, `feat:`, `chore:`, `docs:`, `test:`, `ci:`, `refactor:`, `style:`.
A PR-title-lint workflow blocks merges that don't comply.
-->

## Summary

<!-- 1–3 bullets: what changed and why. Link to any issue this closes (e.g. `Closes #123`). -->

## Code changes

<!--
Per-file or per-area bullet list of what the change does.
Example:
- `Equibles.ParadeDB.EntityFrameworkCore/ParadeDbJsonQuery.cs` — emit `null` for unbounded range sides instead of omitting the key.
- `…/JsonRangeOpenEndedTests.cs` — unskip the regression test now that the fix lands.
-->

## Verification

<!--
How did you verify this works?
- `dotnet build -warnaserror` — clean
- `dotnet test` — N tests pass
- Manual repro — describe
-->

## Checklist

- [ ] PR title is a Conventional Commit
- [ ] `dotnet csharpier check .` passes
- [ ] `dotnet build -warnaserror` produces 0 warnings
- [ ] `dotnet test` passes on all TFMs the change affects
- [ ] `CHANGELOG.md` updated under `## [Unreleased]` (if user-visible)
