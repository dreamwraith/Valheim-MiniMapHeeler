using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using MiniMapHeeler.Layout;

namespace MiniMapHeeler.Configuration
{
    internal static class ModConfig
    {
        public static ConfigEntry<bool> EnableMod = null!;
        public static ConfigEntry<MinimapPlacementMode> MinimapPlacement = null!;
        public static ConfigEntry<float> MinimapWidth = null!;
        public static ConfigEntry<float> MinimapHeight = null!;
        public static ConfigEntry<float> MinimapOffsetX = null!;
        public static ConfigEntry<float> MinimapOffsetY = null!;
        public static ConfigEntry<ShipHudPlacementMode> ShipHudPlacement = null!;
        public static ConfigEntry<float> ShipHudScale = null!;
        public static ConfigEntry<float> ShipHudOffsetX = null!;
        public static ConfigEntry<float> ShipHudOffsetY = null!;
        public static ConfigEntry<bool> EnableDebugLogs = null!;

        public static void Initialize(ConfigFile config)
        {
            EnableMod = Bind(
                config,
                "1 - General",
                "EnableMod",
                true,
                "Master switch to enable or disable the functionality of this mod."
            );

            MinimapPlacement = Bind(
                config,
                "2 - Minimap",
                "MinimapPlacement",
                MinimapPlacementMode.Default,
                "Placement of the minimap and status effect icons. Default keeps the status effect icons to the left of the minimap. ShiftBelowStatusEffects moves the status effect icons to the right screen edge and places the minimap below them, following the number of icon rows. DetachedFreeMovement moves the status effect icons to the right screen edge and places the minimap only with MinimapOffsetX and MinimapOffsetY, measured from the top right screen corner.",
                acceptableValues: null,
                customAttributes: new ConfigurationManagerAttributes { Order = 5 }
            );

            MinimapWidth = Bind(
                config,
                "2 - Minimap",
                "MinimapWidth",
                200.0f,
                "Width of the HUD minimap in canvas UI units (native default is 200; scales with UI scale).",
                new AcceptableValueRange<float>(100.0f, 5000.0f),
                new ConfigurationManagerAttributes { Order = 4 }
            );

            MinimapHeight = Bind(
                config,
                "2 - Minimap",
                "MinimapHeight",
                200.0f,
                "Height of the HUD minimap in canvas UI units (native default is 200; scales with UI scale).",
                new AcceptableValueRange<float>(100.0f, 3000.0f),
                new ConfigurationManagerAttributes { Order = 3 }
            );

            MinimapOffsetX = Bind(
                config,
                "2 - Minimap",
                "MinimapOffsetX",
                0.0f,
                "Horizontal offset in canvas UI units. Measured from the default position, or from the top right screen corner in DetachedFreeMovement. Status effect icons follow this offset only in the Default MinimapPlacement.",
                new AcceptableValueRange<float>(-5000.0f, 5000.0f),
                new ConfigurationManagerAttributes { Order = 2 }
            );

            MinimapOffsetY = Bind(
                config,
                "2 - Minimap",
                "MinimapOffsetY",
                0.0f,
                "Vertical offset in canvas UI units. Measured from the default position, from the shifted position in ShiftBelowStatusEffects, or from the top right screen corner in DetachedFreeMovement.",
                new AcceptableValueRange<float>(-3000.0f, 3000.0f),
                new ConfigurationManagerAttributes { Order = 1 }
            );

            ShipHudPlacement = Bind(
                config,
                "3 - Ship HUD",
                "ShipHudPlacement",
                ShipHudPlacementMode.Default,
                "Position of the ship speed icon and wind indicator. Default keeps the base game stack below the minimap. BelowMinimap and AboveMinimap place them side by side. AboveMinimap moves the minimap down to make room while steering a ship. FullyDetachedVertical (stacked) and FullyDetachedHorizontal (side by side) do not follow the minimap and are placed only with ShipHudOffsetX and ShipHudOffsetY.",
                acceptableValues: null,
                customAttributes: new ConfigurationManagerAttributes { Order = 4 }
            );

            ShipHudScale = Bind(
                config,
                "3 - Ship HUD",
                "ShipHudScale",
                0.75f,
                "Size of the ship speed icon and wind indicator. 1.0 is the base game size.",
                new AcceptableValueRange<float>(0.25f, 1.0f),
                new ConfigurationManagerAttributes { Order = 3 }
            );

            ShipHudOffsetX = Bind(
                config,
                "3 - Ship HUD",
                "ShipHudOffsetX",
                0.0f,
                "Horizontal offset in canvas UI units for the ship speed icon and wind indicator. Measured from the default position of the selected ShipHudPlacement, or from the top right screen corner in the fully detached modes.",
                new AcceptableValueRange<float>(-5000.0f, 5000.0f),
                new ConfigurationManagerAttributes { Order = 2 }
            );

            ShipHudOffsetY = Bind(
                config,
                "3 - Ship HUD",
                "ShipHudOffsetY",
                0.0f,
                "Vertical offset in canvas UI units for the ship speed icon and wind indicator. Measured from the default position of the selected ShipHudPlacement, or from the top right screen corner in the fully detached modes.",
                new AcceptableValueRange<float>(-3000.0f, 3000.0f),
                new ConfigurationManagerAttributes { Order = 1 }
            );

            EnableDebugLogs = Bind(
                config,
                "9 - Debug",
                "EnableDebugLogs",
                false,
                "Enables verbose diagnostic logging in the console and Player.log.",
                acceptableValues: null,
                customAttributes: new ConfigurationManagerAttributes { IsAdvanced = true }
            );

            SettingsMigrations(config);

            EnableMod.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            MinimapPlacement.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            MinimapWidth.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            MinimapHeight.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            MinimapOffsetX.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            MinimapOffsetY.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            ShipHudPlacement.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            ShipHudScale.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            ShipHudOffsetX.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            ShipHudOffsetY.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
        }

        private static ConfigEntry<T> Bind<T>(
            ConfigFile configFile,
            string section,
            string key,
            T defaultValue,
            string description,
            AcceptableValueBase? acceptableValues = null,
            ConfigurationManagerAttributes? customAttributes = null)
        {
            return configFile.Bind(
                section,
                key,
                defaultValue,
                new ConfigDescription(description, acceptableValues, customAttributes)
            );
        }

        // one-time migrations for configs from older versions, will drop in next version
        private static void SettingsMigrations(ConfigFile config)
        {
            Dictionary<ConfigDefinition, Action<string>> legacyEntries = new Dictionary<ConfigDefinition, Action<string>>
            {
                [new ConfigDefinition("3 - HUD Placement", "ShiftBelowStatusEffects")] = rawValue =>
                {
                    if (bool.TryParse(rawValue, out bool shiftBelowStatusEffects) && shiftBelowStatusEffects)
                    {
                        MinimapPlacement.Value = MinimapPlacementMode.ShiftBelowStatusEffects;
                    }
                },
                [new ConfigDefinition("2 - Dimensions", "MinimapWidth")] = MinimapWidth.SetSerializedValue,
                [new ConfigDefinition("2 - Dimensions", "MinimapHeight")] = MinimapHeight.SetSerializedValue,
                [new ConfigDefinition("3 - HUD Placement", "PositionOffsetX")] = MinimapOffsetX.SetSerializedValue,
                [new ConfigDefinition("3 - HUD Placement", "PositionOffsetY")] = MinimapOffsetY.SetSerializedValue
            };

            config.SaveOnConfigSet = false;
            bool migratedAny = false;
            foreach (KeyValuePair<ConfigDefinition, Action<string>> legacyEntry in legacyEntries)
            {
                string rawValue = config.Bind(legacyEntry.Key, string.Empty).Value;
                config.Remove(legacyEntry.Key);
                if (rawValue.Length == 0)
                {
                    continue;
                }

                legacyEntry.Value(rawValue);
                migratedAny = true;
                Plugin.Log.LogInfo($"Migrated old setting {legacyEntry.Key.Section} / {legacyEntry.Key.Key} = {rawValue}.");
            }

            config.SaveOnConfigSet = true;
            if (migratedAny)
            {
                config.Save();
            }
        }
    }
}
