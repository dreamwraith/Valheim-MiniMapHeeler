using HarmonyLib;
using MiniMapHeeler.Layout;
using UnityEngine;

namespace MiniMapHeeler.Patches
{
    [HarmonyPatch(typeof(Minimap))]
    internal static class MinimapPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Minimap.SmallZoom), MethodType.Setter)]
        private static bool SetSmallZoomPrefix(Minimap __instance, float value) =>
            MinimapManager.HandleSmallZoomSet(__instance, value);

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Minimap.Start))]
        private static void StartPostfix(Minimap __instance)
        {
            MinimapManager.InitializeMinimap(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Minimap.SetMapMode))]
        private static void SetMapModePostfix(Minimap.MapMode mode)
        {
            if (mode == Minimap.MapMode.Small)
            {
                MinimapManager.ApplyLayout();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Minimap.CenterMap), typeof(Vector3))]
        private static void CenterMapPostfix(Minimap __instance, Vector3 centerPoint) =>
            MinimapManager.HandleCenterMap(__instance, centerPoint);
    }
}
