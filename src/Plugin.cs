using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MiniMapHeeler.Configuration;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiniMapHeeler
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "dreamwraith.MiniMapHeeler";
        public const string ModName = "MiniMapHeeler";
        public const string ModVersion = VersionInfo.Version;

        internal static ManualLogSource Log { get; private set; } = null!;
        private Harmony? _harmony;

        private void Awake()
        {
            Log = Logger;

            ModConfig.Initialize(Config);

            // same check as jotunns GUIManager.IsHeadless(), jotunn isnt here at runtime
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                return;
            }

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGUID);
            Log.LogInfo($"{ModName} v{ModVersion} loaded successfully.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        internal static void LogDebug(string message)
        {
            if (ModConfig.Instance.EnableDebugLogs.Value)
            {
                Log.LogInfo($"[DEBUG] {message}");
            }
        }
    }
}
