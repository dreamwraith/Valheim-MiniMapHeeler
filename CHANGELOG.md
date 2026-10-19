# Changelog

All notable changes to **MiniMapHeeler** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
