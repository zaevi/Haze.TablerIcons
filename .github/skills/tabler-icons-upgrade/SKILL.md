---
name: tabler-icons-upgrade
description: 'Upgrade Haze.TablerIcons to a new upstream Tabler Icons release. Use when syncing icons, replacing tabler-icons.ttf, regenerating Generated/, bumping version, and validating build/pack before NuGet publish.'
argument-hint: 'Target upstream version (e.g. 3.44.0)'
user-invocable: true
---

# Tabler Icons Upgrade Workflow

## What This Skill Produces

- Updated upstream source checkout (`.tmp-tabler-icons/`) for generator input.
- Updated embedded font file at `src/Haze.TablerIcons/Resources/tabler-icons.ttf`.
- Regenerated C# sources in `src/Haze.TablerIcons/Generated/` via the local console generator.
- Synchronized `Version` / `AssemblyVersion` in the project file.
- Validated `dotnet build` and `dotnet pack` both succeed.

## When To Use

- "Upgrade icon library to latest"
- "Sync with latest Tabler Icons release"
- "Refresh font and generated icons"
- Release preparation where icon source, font asset, and package version must stay aligned.
- Troubleshooting upgrade issues (shallow clone pitfalls, npm temp-folder problems).

## Architecture Context

```
Haze.TablerIcons.slnx
├── src/Haze.TablerIcons/                  # Icon library project
│   ├── Haze.TablerIcons.csproj
│   ├── TablerIcon.cs, TablerIcons.cs
│   ├── Resources/tabler-icons.ttf         # Embedded font
│   └── Generated/                         # Auto-generated C# sources
└── src/TablerIconGenerator/               # Local console generator
    ├── TablerIconGenerator.csproj
    ├── Program.cs                         # CLI entry point
    ├── TablerIconGenerator.cs             # Generator logic
    ├── TablerIconData.cs                  # Icon metadata parser
    └── Utils.cs                           # Shared utility types
```

## Inputs

- **Target upstream version** — e.g. `3.44.0`. Determined from GitHub Releases.
- Source checkout path (default: `.tmp-tabler-icons/`).
- Temp folder for npm webfont download (default: `tmp-upgrade/`).

## Procedure

### 1. Confirm local state

- Check `git status` for any uncommitted changes.
- Do **not** revert unrelated local edits; only reset files if interfering with regeneration.

### 2. Determine target version

- Browse https://github.com/tabler/tabler-icons/releases to find the latest release tag.
- Read current version from `src/Haze.TablerIcons/Haze.TablerIcons.csproj` (`<Version>`).

### 3. Update upstream source checkout

The repo maintains a shallow clone at `.tmp-tabler-icons/` for generator input.

```powershell
# Fetch the target tag and check it out
cd .tmp-tabler-icons
git fetch --depth=1 origin v<target-version>
git checkout FETCH_HEAD
```

If `.tmp-tabler-icons/` doesn't exist yet, create a fresh shallow clone:

```powershell
git clone --depth=1 https://github.com/tabler/tabler-icons.git .tmp-tabler-icons
```

> **Note:** Tags are not available in a shallow clone created with `--depth=1` alone. Use `git fetch --depth=1 origin v<tag>` to retrieve the specific release.

### 4. Download webfont and extract tabler-icons.ttf

Use a **non-dot-prefixed** temp folder to avoid npm package name validation errors.

```powershell
New-Item -ItemType Directory -Path tmp-upgrade -Force
Set-Location tmp-upgrade
npm install @tabler/icons-webfont@<target-version>
```

The font file is located at:

```
node_modules/@tabler/icons-webfont/dist/fonts/tabler-icons.ttf
```

### 5. Replace embedded font

```powershell
Copy-Item tmp-upgrade/node_modules/@tabler/icons-webfont/dist/fonts/tabler-icons.ttf `
  -Destination src/Haze.TablerIcons/Resources/tabler-icons.ttf -Force
```

Optional: verify the file hash matches the npm package.

### 6. Regenerate Generated sources

Run the local console generator, pointing it to the upstream icon SVGs:

```powershell
dotnet run --project src/TablerIconGenerator -- `
  .tmp-tabler-icons/icons src/Haze.TablerIcons/Generated
```

Confirm the tool outputs `Done` with no errors.

Generated files under `src/Haze.TablerIcons/Generated/` are **tool-produced only** — do not hand-edit.

### 7. Update project version

Edit `src/Haze.TablerIcons/Haze.TablerIcons.csproj`:

- `<Version>` — set to `<upstream>.<patch>` (e.g. `3.44.0.1`)
- `<AssemblyVersion>` — same value

**Versioning rule:**

| Component | Source |
|-----------|--------|
| Major.Minor.Build | Upstream Tabler Icons version |
| Revision | Repository patch counter (increment for each release of this package) |

Examples:
- First release after upstream `3.44.0` → `3.44.0.1`
- A repository-only fix without upstream change → `3.44.0.2`

### 8. Validate build and pack

```powershell
dotnet build Haze.TablerIcons.slnx -c Release
dotnet pack src/Haze.TablerIcons/Haze.TablerIcons.csproj -c Release -o ./nupkg
```

Confirm:
- Build completes with no errors.
- `.nupkg` file is created in `./nupkg/`.

### 9. Clean up temporary files

```powershell
Remove-Item -Recurse -Force tmp-upgrade
```

### 10. Review and finalize

Verify only intended files changed:

- `src/Haze.TablerIcons/Resources/tabler-icons.ttf` (font update)
- `src/Haze.TablerIcons/Generated/*.cs` (regenerated sources)
- `src/Haze.TablerIcons/Haze.TablerIcons.csproj` (version bump)
- Workflow docs if needed

## Decision Points and Branching

| Situation | Action |
|-----------|--------|
| `npm init` fails in dot-prefixed folder | Rename folder to non-dot style, or skip `npm init` and run `npm install` directly. |
| Shallow clone has no tags | Use `git fetch --depth=1 origin v<tag>` to pull just that tag. |
| `git fetch --tags` fails with `invalid index-pack output` | Do not block upgrade; use GitHub Releases page as version authority. |
| Generator encounters unexpected SVG format | Check if upstream added/changed SVG structure; may need generator code update. |
| `.tmp-tabler-icons` doesn't exist | Create fresh shallow clone from upstream repo. |

## Quality Gates

- [ ] Generated files are tool-produced only; no manual edits in `src/Haze.TablerIcons/Generated/`.
- [ ] `<Version>` and `<AssemblyVersion>` are synchronized.
- [ ] `dotnet build -c Release` passes.
- [ ] `dotnet pack -c Release` produces a valid `.nupkg`.
- [ ] No secrets (NuGet API keys) included in committed changes.

## Commit Convention

**Title template:**

```
chore(icons): upgrade Tabler Icons to v3.44.0 (repo patch 1)
```

**Body checklist:**

```
- Replace src/Haze.TablerIcons/Resources/tabler-icons.ttf from @tabler/icons-webfont@3.44.0
- Regenerate src/Haze.TablerIcons/Generated/ via TablerIconGenerator
- Sync Version and AssemblyVersion to 3.44.0.1
- Validate dotnet build and dotnet pack
```

## Known Pitfalls

### npm folder naming
Dot-prefixed temp directories (e.g. `.tmp-upgrade`) cause `npm init -y` to fail with `Invalid name: ".tmp-upgrade"`. Fix: use a non-hidden folder name, or skip `npm init` entirely and run `npm install` directly.

### Shallow clone tags
`git clone --depth=1` does **not** include tags. Use `git fetch --depth=1 origin v<tag>` to retrieve the specific release.

### Large tag fetch in constrained environments
`git fetch --tags` can fail with `fatal: fetch-pack: invalid index-pack output`. Do not block the upgrade on this; rely on GitHub Releases page/API as the source of truth.

## Completion Checklist

- [ ] Upstream source checked out at `.tmp-tabler-icons` at target tag.
- [ ] Font replaced at `src/Haze.TablerIcons/Resources/tabler-icons.ttf`.
- [ ] Generated sources refreshed under `src/Haze.TablerIcons/Generated/`.
- [ ] Project version fields updated consistently.
- [ ] `dotnet build` and `dotnet pack` both successful.
- [ ] Temp folder `tmp-upgrade/` cleaned up.
