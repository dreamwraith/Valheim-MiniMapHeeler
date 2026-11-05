# Changelog

All notable changes to **MiniMapHeeler** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-11-04

### Fixed
- I completely missed the ship HUD when I made the minimap resizable so a bigger minimap sat right on top of the speed icon and wind indicator while sailing. They move with the minimap now by default. Optional adjustments also added.

### Added
- Ship HUD settings for the speed icon and wind indicator. Put them under the minimap or above it or to its left or anywhere on screen. They shrink as well and start at 0.75.
- `DetachedFreeMovement` minimap placement so the minimap can go anywhere too.

### Changed
- `ShiftBelowStatusEffects` lives in `MinimapPlacement` and the minimap moves with your status effect rows. No more empty gap or buffs sitting on the minimap.
- Settings got reshuffled into new sections and a few got renamed. Old settings carry over on first launch.
- Bigger slider ranges for big screens.
- Cleaned up some Unity null checks.

## [1.1.1] - 2026-11-01

### Changed
- Skips its patches on dedicated servers.
- The `PositionOffsetX` and `PositionOffsetY` descriptions actually explain how they work with `ShiftBelowStatusEffects` now.
- Bumped BepInExPack to 5.4.2360.
- Build tooling and source layout now match the rest of my mods.

## [1.1.0] - 2026-10-19

### Changed
- Migrated build pipeline to compile-time publicized game assemblies via Jotunn prebuild task (`DoPrebuild.props` and build-only `JotunnLib`).
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
