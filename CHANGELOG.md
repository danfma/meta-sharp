# Changelog

All notable changes to Metano are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project
adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 0.8.1

### 🐛 Bug Fixes

- `package.json` writes are now additive: hand-curated `type`,
  `sideEffects`, `name`, `imports`, and `exports` survive every
  regeneration. The transpiler only seeds missing fields and refreshes
  its own `{types, import}` exports, leaving user-added subpaths and
  augmented conditional fields untouched (#136).
- `metano-runtime` now declares `"sideEffects": false` so consumers'
  bundlers can tree-shake unused helpers. Verified bundle drop from
  175.1 KB to 1.96 KB on a `HashCode`-only entry — 88× smaller (#137).

### 🧰 Maintenance

- `dotnet-releaser.toml` adds Conventional Commits autolabelers so
  `feat:` / `fix:` / `chore:` etc. land in the right release-note
  sections instead of bundling under "🧰 Misc" (#135).
- Release workflow grants `pull-requests: write` and `issues: write`
  so dotnet-releaser can read merged-PR data when assembling the
  changelog.
- Pinned `dotnet-releaser` 0.16.0 → 0.18.1 for the Tomlyn config
  deserialization fix.
- Switched to a static `CHANGELOG.md` driving release notes —
  works around the GitHub `/commits/{sha}/pulls` 5xx flakiness that
  blocked the v0.8.1 changelog auto-generation.

## 0.8.0

See [the v0.8.0 release notes](https://github.com/danfma/metano/releases/tag/v0.8.0)
for the prior changeset.
