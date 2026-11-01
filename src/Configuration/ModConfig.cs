using BepInEx.Configuration;
using MiniMapHeeler.Layout;

namespace MiniMapHeeler.Configuration
{
    internal static class ModConfig
    {
        public static ConfigEntry<bool> EnableMod = null!;
        public static ConfigEntry<float> MinimapWidth = null!;
        public static ConfigEntry<float> MinimapHeight = null!;
        public static ConfigEntry<bool> ShiftBelowStatusEffects = null!;
        public static ConfigEntry<float> PositionOffsetX = null!;
        public static ConfigEntry<float> PositionOffsetY = null!;
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

            MinimapWidth = Bind(
                config,
                "2 - Dimensions",
                "MinimapWidth",
                200.0f,
                "Width of the top-corner HUD minimap in canvas UI units (native default is 200; scales with UI scale).",
                new AcceptableValueRange<float>(100.0f, 1920.0f)
            );

            MinimapHeight = Bind(
                config,
                "2 - Dimensions",
                "MinimapHeight",
                200.0f,
                "Height of the top-corner HUD minimap in canvas UI units (native default is 200; scales with UI scale).",
                new AcceptableValueRange<float>(100.0f, 1080.0f)
            );

            ShiftBelowStatusEffects = Bind(
                config,
                "3 - HUD Placement",
                "ShiftBelowStatusEffects",
                false,
                "When true, shifts status effect icons to the right screen edge without raising them, and places the minimap below them."
            );

            PositionOffsetX = Bind(
                config,
                "3 - HUD Placement",
                "PositionOffsetX",
                0.0f,
                "Horizontal offset in canvas UI units from default position.",
                new AcceptableValueRange<float>(-1920.0f, 1920.0f)
            );

            PositionOffsetY = Bind(
                config,
                "3 - HUD Placement",
                "PositionOffsetY",
                0.0f,
                "Vertical offset in canvas UI units from default position.",
                new AcceptableValueRange<float>(-1080.0f, 1080.0f)
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

            EnableMod.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            MinimapWidth.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            MinimapHeight.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            ShiftBelowStatusEffects.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            PositionOffsetX.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
            PositionOffsetY.SettingChanged += (sender, eventArgs) => MinimapManager.ApplyLayout();
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
    }
}
