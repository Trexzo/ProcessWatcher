# Contributing to ProcessWatcher

Thanks for improving ProcessWatcher. The project is intentionally small and dependency-light, so changes should stay focused and easy to review.

## Before you start

- Use Windows 10 or Windows 11.
- Use the .NET 8 SDK.
- Keep each change focused on one behavior or documentation goal.
- Avoid committing generated `bin/` or `obj/` output.

For detailed build and troubleshooting steps, see [docs/BUILDING.md](docs/BUILDING.md).

## Branches

Use a short branch name that describes the change, for example:

```text
fix/process-refresh
docs/build-notes
feature/process-copy-action
```

Prefer one logical change per branch.

## Build verification

From the repository root:

```powershell
dotnet restore
dotnet build -c Release
```

If the change affects packaging, startup, native interop, or deployment behavior, also reproduce the self-contained Windows x64 publish:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

## Runtime smoke checks

For application-code changes, launch the resulting build and verify the affected path on Windows.

When relevant, check that:

- the process list continues refreshing;
- search and sorting still work;
- selected-process details update correctly;
- CPU, memory, disk, network, and uptime cards continue updating;
- executable-location and PID actions still behave correctly;
- changing the refresh interval does not freeze the UI.

You do not need to retest unrelated behavior for a documentation-only change.

## UI changes

ProcessWatcher uses a custom WinForms interface rather than a third-party UI framework.

For visual changes:

- keep controls readable at normal Windows scaling;
- avoid unnecessary layout shifts;
- verify long process names and large numeric values do not overlap nearby controls;
- keep keyboard/mouse behavior consistent with existing controls.

## Native and platform-specific changes

Changes involving Windows APIs or process inspection should fail gracefully when information is unavailable or access is denied. Do not assume every process exposes every property successfully.

Keep platform-specific behavior explicit rather than silently falling back to misleading values.

## Pull requests

A useful pull request should include:

1. a clear title;
2. a short description of the problem and the change;
3. how the change was verified;
4. screenshots only when they help explain a UI change;
5. no unrelated formatting or generated-file churn.

If a pull request resolves an issue, use GitHub's closing syntax such as `Closes #123`.

## Commit hygiene

Use concise commit messages that describe the change, for example:

```text
fix: handle inaccessible process paths
docs: clarify release publish steps
ui: prevent metric label overlap
```

Small, reviewable commits are preferred over large mixed changes.
