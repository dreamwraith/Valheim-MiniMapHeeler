using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MiniMapHeeler.Configuration;

namespace MiniMapHeeler
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "dreamwraith.MiniMapHeeler";
        public const string ModName = "MiniMapHeeler";
        public const string ModVersion = VersionInfo.Version;

        internal static Plugin? Instance { get; private set; }
        internal static ManualLogSource Log = null!;
        private Harmony? _harmony;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            ModConfig.Initialize(Config);

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGUID);
            Log.LogInfo($"{ModName} v{ModVersion} loaded successfully.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        internal static void LogDebug(string message)
        {
            if (ModConfig.EnableDebugLogs != null && ModConfig.EnableDebugLogs.Value)
            {
                Log.LogInfo($"[DEBUG] {message}");
            }
        }
    }
}
