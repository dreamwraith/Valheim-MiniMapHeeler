using System;
using BepInEx.Configuration;
using MiniMapHeeler.Layout;

namespace MiniMapHeeler.Configuration
{
    internal sealed class ModConfig(ConfigFile config)
    {
        public static ModConfig Instance { get; private set; } = null!;

        public ConfigEntry<bool> EnableMod { get; } = Bind(
            config,
            "1 - General",
            "EnableMod",
            true,
            "Master switch to enable or disable the functionality of this mod.");

        public ConfigEntry<MinimapPlacementMode> MinimapPlacement { get; } = Bind(
            config,
            "2 - Minimap",
            "MinimapPlacement",
            MinimapPlacementMode.Default,
            "Placement of the minimap and status effect icons. Default keeps the status effect icons to the left of the minimap. ShiftBelowStatusEffects moves the status effect icons to the right screen edge and places the minimap below them, following the number of icon rows. DetachedFreeMovement moves the status effect icons to the right screen edge and places the minimap only with MinimapOffsetX and MinimapOffsetY, measured from the top right screen corner.",
            customAttributes: new ConfigurationManagerAttributes { Order = 5 });

        public ConfigEntry<float> MinimapWidth { get; } = Bind(
            config,
            "2 - Minimap",
            "MinimapWidth",
            200.0f,
            "Width of the HUD minimap in canvas UI units (base game default is 200; scales with UI scale).",
            new AcceptableValueRange<float>(100.0f, 5000.0f),
            new ConfigurationManagerAttributes { Order = 4 });

        public ConfigEntry<float> MinimapHeight { get; } = Bind(
            config,
            "2 - Minimap",
            "MinimapHeight",
            200.0f,
            "Height of the HUD minimap in canvas UI units (base game default is 200; scales with UI scale).",
            new AcceptableValueRange<float>(100.0f, 3000.0f),
            new ConfigurationManagerAttributes { Order = 3 });

        public ConfigEntry<float> MinimapOffsetX { get; } = Bind(
            config,
            "2 - Minimap",
            "MinimapOffsetX",
            0.0f,
            "Horizontal offset in canvas UI units. Measured from the default position, or from the top right screen corner in DetachedFreeMovement. Status effect icons follow this offset only in the Default MinimapPlacement.",
            new AcceptableValueRange<float>(-5000.0f, 5000.0f),
            new ConfigurationManagerAttributes { Order = 2 });

        public ConfigEntry<float> MinimapOffsetY { get; } = Bind(
            config,
            "2 - Minimap",
            "MinimapOffsetY",
            0.0f,
            "Vertical offset in canvas UI units. Measured from the default position, from the shifted position in ShiftBelowStatusEffects, or from the top right screen corner in DetachedFreeMovement.",
            new AcceptableValueRange<float>(-3000.0f, 3000.0f),
            new ConfigurationManagerAttributes { Order = 1 });

        public ConfigEntry<ShipHudPlacementMode> ShipHudPlacement { get; } = Bind(
            config,
            "3 - Ship HUD",
            "ShipHudPlacement",
            ShipHudPlacementMode.Default,
            "Position of the ship speed icon and wind indicator. Default keeps the base game stack below the minimap. BelowMinimap and AboveMinimap place them side by side. AboveMinimap moves the minimap down to make room while steering a ship. FullyDetachedVertical (stacked) and FullyDetachedHorizontal (side by side) do not follow the minimap and are placed only with ShipHudOffsetX and ShipHudOffsetY.",
            customAttributes: new ConfigurationManagerAttributes { Order = 4 });

        public ConfigEntry<float> ShipHudScale { get; } = Bind(
            config,
            "3 - Ship HUD",
            "ShipHudScale",
            0.75f,
            "Size of the ship speed icon and wind indicator. 1.0 is the base game size.",
            new AcceptableValueRange<float>(0.25f, 1.0f),
            new ConfigurationManagerAttributes { Order = 3 });

        public ConfigEntry<float> ShipHudOffsetX { get; } = Bind(
            config,
            "3 - Ship HUD",
            "ShipHudOffsetX",
            0.0f,
            "Horizontal offset in canvas UI units for the ship speed icon and wind indicator. Measured from the default position of the selected ShipHudPlacement, or from the top right screen corner in the fully detached modes.",
            new AcceptableValueRange<float>(-5000.0f, 5000.0f),
            new ConfigurationManagerAttributes { Order = 2 });

        public ConfigEntry<float> ShipHudOffsetY { get; } = Bind(
            config,
            "3 - Ship HUD",
            "ShipHudOffsetY",
            0.0f,
            "Vertical offset in canvas UI units for the ship speed icon and wind indicator. Measured from the default position of the selected ShipHudPlacement, or from the top right screen corner in the fully detached modes.",
            new AcceptableValueRange<float>(-3000.0f, 3000.0f),
            new ConfigurationManagerAttributes { Order = 1 });

        public ConfigEntry<bool> EnableDebugLogs { get; } = Bind(
            config,
            "9 - Debug",
            "EnableDebugLogs",
            false,
            "Enables verbose diagnostic logging in the console and Player.log.",
            customAttributes: new ConfigurationManagerAttributes { IsAdvanced = true });

        public static void Initialize(ConfigFile configFile)
        {
            Instance = new ModConfig(configFile);
            Instance.SettingsMigrations(configFile);

            Instance.EnableMod.SettingChanged += OnEnableModChanged;
            Instance.MinimapPlacement.SettingChanged += OnLayoutSettingChanged;
            Instance.MinimapWidth.SettingChanged += OnLayoutSettingChanged;
            Instance.MinimapHeight.SettingChanged += OnLayoutSettingChanged;
            Instance.MinimapOffsetX.SettingChanged += OnLayoutSettingChanged;
            Instance.MinimapOffsetY.SettingChanged += OnLayoutSettingChanged;
            Instance.ShipHudPlacement.SettingChanged += OnLayoutSettingChanged;
            Instance.ShipHudScale.SettingChanged += OnLayoutSettingChanged;
            Instance.ShipHudOffsetX.SettingChanged += OnLayoutSettingChanged;
            Instance.ShipHudOffsetY.SettingChanged += OnLayoutSettingChanged;
        }

        private static void OnEnableModChanged(object sender, EventArgs eventArgs)
        {
            if (Instance.EnableMod.Value)
            {
                MinimapManager.ApplyLayout();
            }
            else
            {
                MinimapManager.RestoreDefaults();
            }
        }

        private static void OnLayoutSettingChanged(object sender, EventArgs eventArgs) => MinimapManager.ApplyLayout();

        private static ConfigEntry<T> Bind<T>(
            ConfigFile configFile,
            string section,
            string key,
            T defaultValue,
            string description,
            AcceptableValueBase? acceptableValues = null,
            ConfigurationManagerAttributes? customAttributes = null) =>
            configFile.Bind(section, key, defaultValue, new ConfigDescription(description, acceptableValues, customAttributes));

        // one-time migrations for configs from older versions, will drop in next version
        private void SettingsMigrations(ConfigFile configFile)
        {
            (ConfigDefinition Definition, Action<string> Apply)[] legacyEntries =
            [
                (new ConfigDefinition("3 - HUD Placement", "ShiftBelowStatusEffects"), rawValue =>
                {
                    if (bool.TryParse(rawValue, out bool shiftBelowStatusEffects) && shiftBelowStatusEffects)
                    {
                        MinimapPlacement.Value = MinimapPlacementMode.ShiftBelowStatusEffects;
                    }
                }),
                (new ConfigDefinition("2 - Dimensions", "MinimapWidth"), MinimapWidth.SetSerializedValue),
                (new ConfigDefinition("2 - Dimensions", "MinimapHeight"), MinimapHeight.SetSerializedValue),
                (new ConfigDefinition("3 - HUD Placement", "PositionOffsetX"), MinimapOffsetX.SetSerializedValue),
                (new ConfigDefinition("3 - HUD Placement", "PositionOffsetY"), MinimapOffsetY.SetSerializedValue)
            ];

            bool migratedAny = false;
            configFile.SaveOnConfigSet = false;
            try
            {
                foreach ((ConfigDefinition definition, Action<string> apply) in legacyEntries)
                {
                    string rawValue = configFile.Bind(definition, string.Empty).Value;
                    configFile.Remove(definition);
                    if (rawValue.Length == 0)
                    {
                        continue;
                    }

                    apply(rawValue);
                    migratedAny = true;
                    Plugin.Log.LogInfo($"Migrated old setting {definition.Section} / {definition.Key} = {rawValue}.");
                }
            }
            finally
            {
                configFile.SaveOnConfigSet = true;
            }

            if (migratedAny)
            {
                configFile.Save();
            }
        }
    }
}
