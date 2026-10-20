# Changelog

All notable changes to **MiniMapHeeler** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-10-19

### Changed
- Migrated build pipeline to compile-time publicized game assemblies via Jötunn prebuild task (`DoPrebuild.props` and build-only `JotunnLib`).
- Replace reflection (`AccessTools.FieldRefAccess` and `AccessTools.MethodDelegate`) on `m_smallZoom`, `m_pinUpdateRequired`, and `UpdatePins` with direct member access.
- Use `nameof(...)` for `Minimap.CenterMap` Harmony patch target.
- Remove Hexium publish target and tooling.
- Cleaned up component lookups with native `TryGetComponent` and simplified Unity null checks.

## [1.0.1] - 2026-10-18

### Fixed
- Fixed broken relative repository and license links in README when viewed on mod portals.

## [1.0.0] - 2026-10-17

### Added
- Initial release.
- Custom minimap width and height configuration with 1:1 terrain scaling.
- Expanded zoom range with balanced, reversible zoom steps.
- Automatic status effect repositioning to prevent overlap with the resized minimap.
- Optional layout mode (`ShiftBelowStatusEffects`) to position the minimap below status effect icons.
- Custom horizontal and vertical position offset controls.
- Real-time settings reload through BepInEx Configuration Manager.
- Clean restoration to native HUD layout when the mod is disabled.
