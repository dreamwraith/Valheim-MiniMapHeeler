using HarmonyLib;
using MiniMapHeeler.Layout;

namespace MiniMapHeeler.Patches
{
    [HarmonyPatch(typeof(Hud))]
    internal static class HudPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Hud.Awake))]
        private static void AwakePostfix(Hud __instance)
        {
            MinimapManager.InitializeHud(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Hud.LateUpdate))]
        private static void LateUpdatePostfix(Hud __instance)
        {
            HudLayoutTracker.CheckForChanges(__instance);
        }
    }
}
