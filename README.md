# MiniMapHeeler

[![GitHub Release](https://img.shields.io/github/v/release/dreamwraith/Valheim-MiniMapHeeler?logo=github&color=1081c2)](https://github.com/dreamwraith/Valheim-MiniMapHeeler/releases)
[![GitHub Downloads](https://img.shields.io/github/downloads/dreamwraith/Valheim-MiniMapHeeler/total?logo=github&color=1081c2)](https://github.com/dreamwraith/Valheim-MiniMapHeeler/releases)
[![Last Commit](https://img.shields.io/github/last-commit/dreamwraith/Valheim-MiniMapHeeler?logo=git)](https://github.com/dreamwraith/Valheim-MiniMapHeeler/commits/main)
[![Publish Status](https://img.shields.io/github/actions/workflow/status/dreamwraith/Valheim-MiniMapHeeler/publish.yml?label=Publish%20Portals&logo=githubactions)](https://github.com/dreamwraith/Valheim-MiniMapHeeler/actions)
[![Thunderstore](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fthunderstore.io%2Fapi%2Fexperimental%2Fpackage%2FDreamWraith%2FMiniMapHeeler%2F&query=%24.latest.version_number&label=Thunderstore&logo=thunderstore&color=2980b9)](https://thunderstore.io/c/valheim/p/DreamWraith/MiniMapHeeler/)
[![Hexium](https://img.shields.io/badge/Hexium-MiniMapHeeler-6c5ce7)](https://valheim.hexium.gg/mods/DreamWraith/MiniMapHeeler)
[![Nexus Mods](https://img.shields.io/badge/Nexus_Mods-MiniMapHeeler-da8d24?logo=nexusmods)](https://www.nexusmods.com/valheim/mods/4508)
[![Game: Valheim](https://img.shields.io/badge/Valheim-Deep_North_%2F_1.x-1b2838?logo=steam&logoColor=white)](https://store.steampowered.com/app/892970/Valheim/)
[![BepInEx Pack](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fthunderstore.io%2Fapi%2Fexperimental%2Fpackage%2Fdenikson%2FBepInExPack_Valheim%2F&query=%24.latest.version_number&label=BepInEx&color=5B57E7)](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)
[![Target: .NET 4.8](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4?logo=dotnet)](MiniMapHeeler.csproj)
[![Client Only](https://img.shields.io/badge/Type-Client%20Only-blue)](README.md#compatibility)
[![License](https://img.shields.io/github/license/dreamwraith/Valheim-MiniMapHeeler?color=blue)](LICENSE.md)
[![AI Philosophy](https://img.shields.io/badge/AI%20Philosophy-Software%20Craft-2ea44f?logo=github)](https://gist.github.com/dreamwraith/77c91d656c842611bf8c40febf8056f2)

In native Valheim, the HUD minimap is locked to a fixed 200x200 square with a narrow zoom range, offering no way to adjust its size to fit your display or UI preferences. **MiniMapHeeler** unlocks minimap sizing while keeping the surrounding HUD clean and properly aligned.

This mod runs client-side on your local HUD. It does not need to be installed on servers and works in multiplayer.

---

## Features

- **Custom dimensions:** Freely adjust width (100–1920) and height (100–1080) larger or smaller without texture distortion.
- **Wider zoom range:** Expands zoom boundaries with balanced steps so zooming in and out stays consistent.
- **HUD alignment:** Automatically offsets status effect icons to prevent overlap, with an optional layout to place the minimap below them.
- **Live in-game tuning:** Settings changed in BepInEx Configuration Manager (`F1`) apply immediately without restarting.
- **Client-side only:** Safe for local worlds and multiplayer servers with no server installation needed.

---

## Configuration

Settings can be changed in-game using the BepInEx Configuration Manager (`F1`) or by editing `BepInEx/config/dreamwraith.MiniMapHeeler.cfg`:

| Section | Key | Type | Default | Description |
| :--- | :--- | :--- | :--- | :--- |
| `1 - General` | `EnableMod` | `bool` | `true` | Enable or disable the mod. |
| `2 - Dimensions` | `MinimapWidth` | `float` | `200.0` | Minimap width in UI units (native default is 200; scales with UI scale). |
| `2 - Dimensions` | `MinimapHeight` | `float` | `200.0` | Minimap height in UI units (native default is 200; scales with UI scale). |
| `3 - HUD Placement` | `ShiftBelowStatusEffects` | `bool` | `false` | When true, aligns status effects to the right edge and places the minimap below them. |
| `3 - HUD Placement` | `PositionOffsetX` | `float` | `0.0` | Horizontal offset in UI units from default position. |
| `3 - HUD Placement` | `PositionOffsetY` | `float` | `0.0` | Vertical offset in UI units from default position. |
| `9 - Debug` | `EnableDebugLogs` | `bool` | `false` | Enable verbose diagnostic logging in console and Player.log. |

---

## Compatibility

- **Client-side only:** Runs locally on the game HUD; safe to use on any multiplayer server.
- **Single-player:** Fully functional in local worlds.
- **Mod managers:** Compatible with Gale, Thunderstore Mod Manager, and r2modman.
- **UI scaling:** Fully compatible with Valheim's built-in UI scale slider (dimensions and offsets scale proportionally with the rest of the HUD).

---

## Installation

### Mod Manager (Recommended)
Install through **Gale**, **Thunderstore Mod Manager**, or **r2modman**.

### Manual Installation
1. Install **BepInExPack Valheim**.
2. Download the latest release from GitHub or Thunderstore.
3. Place `MiniMapHeeler.dll` into `Valheim/BepInEx/plugins/`.
4. Launch Valheim.

---

## Building from Source

Build using the .NET SDK:

```bash
dotnet build -c Release
```

The compiled assembly is output to `bin/Release/net48/MiniMapHeeler.dll`. Building with the `Release` configuration also creates a distribution zip in `bin/Publish/`.

### Custom Paths
If your Valheim installation or mod manager profile uses a non-standard path, copy `MiniMapHeeler.csproj.user.example` to `MiniMapHeeler.csproj.user` in the project root and edit your paths:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <GamePath>D:\SteamLibrary\steamapps\common\Valheim</GamePath>
    <BepInExCorePath>$(UserProfile)\AppData\Roaming\com.kesomannen.gale\valheim\profiles\<ProfileName>\BepInEx\core</BepInExCorePath>
  </PropertyGroup>
</Project>
```

---

## Packaging and Releases

Packaging and release automation are handled through [`.scripts/modtools.ps1`](.scripts/modtools.ps1) using the [**`DW.ValheimModTools`**](https://github.com/DreamWraith/DW-ValheimModTools) module:

- **Create a release:** `pwsh ./.scripts/modtools.ps1 release` (builds Release, creates distribution archives, extracts changelog notes, and creates a GitHub Release).
- **Bump version and package:** `pwsh ./.scripts/modtools.ps1 package -Bump Patch`
- **Publish to mod portals:** `pwsh ./.scripts/modtools.ps1 publish -Target All` (uploads to Thunderstore, Hexium, and Nexus Mods).

For full script options and secret configuration, see [`.scripts/README.md`](.scripts/README.md).

---

## License & Notes

- Licensed under the GNU General Public License v3.0 (see [LICENSE.md](LICENSE.md)).
- [A Note on Valheim Modding](https://gist.github.com/dreamwraith/98564f8441dc234bfadd7e2b605c694c)
- [A Note on AI and Software Craft](https://gist.github.com/dreamwraith/77c91d656c842611bf8c40febf8056f2)
