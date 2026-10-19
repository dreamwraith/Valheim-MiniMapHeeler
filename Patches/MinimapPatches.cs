using HarmonyLib;
using MiniMapHeeler.Configuration;
using MiniMapHeeler.Layout;
using UnityEngine;

namespace MiniMapHeeler.Patches
{
    [HarmonyPatch(typeof(Minimap))]
    internal static class MinimapPatches
    {
        public const float HardcodedDefaultZoom = 0.015f;
        public const float HardcodedMinZoom = 0.002f;
        public const float HardcodedMaxZoom = 1.0f;

        private static readonly AccessTools.FieldRef<Minimap, float> s_smallZoomFieldRef =
            AccessTools.FieldRefAccess<Minimap, float>("m_smallZoom");

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Minimap.SmallZoom), MethodType.Setter)]
        private static bool SetSmallZoomPrefix(Minimap __instance, ref float value)
        {
            if (!ModConfig.EnableMod.Value)
            {
                return true;
            }

            float current = __instance.SmallZoom;

            // Replace native asymmetric zoom (0.5x vs 1.5x) with symmetric 1.5x scaling
            bool isZoomIn = Mathf.Approximately(value, current * 0.5f);
            bool isZoomOut = Mathf.Approximately(value, current * 1.5f);

            if (isZoomIn)
            {
                float newZoom = Mathf.Clamp(current / 1.5f, HardcodedMinZoom, HardcodedMaxZoom);
                s_smallZoomFieldRef(__instance) = newZoom;
                Plugin.LogDebug($"Zoom in: {current:F4} -> {newZoom:F4}");
                return false;
            }

            if (isZoomOut)
            {
                float newZoom = Mathf.Clamp(current * 1.5f, HardcodedMinZoom, HardcodedMaxZoom);
                s_smallZoomFieldRef(__instance) = newZoom;
                Plugin.LogDebug($"Zoom out: {current:F4} -> {newZoom:F4}");
                return false;
            }

            s_smallZoomFieldRef(__instance) = Mathf.Clamp(value, HardcodedMinZoom, HardcodedMaxZoom);
            return false;
        }

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
        [HarmonyPatch("CenterMap", typeof(Vector3))]
        private static void CenterMapPostfix(Minimap __instance, Vector3 centerPoint)
        {
            if (!ModConfig.EnableMod.Value || __instance.m_mode != Minimap.MapMode.Small || __instance.m_mapImageSmall == null)
            {
                return;
            }

            float targetWidth = Mathf.Max(50f, ModConfig.MinimapWidth.Value);
            float targetHeight = Mathf.Max(50f, ModConfig.MinimapHeight.Value);

            float innerMapWidth = targetWidth - MinimapManager.NativeMapPadding;
            float innerMapHeight = targetHeight - MinimapManager.NativeMapPadding;

            MinimapManager.UpdateViewportUv(__instance, innerMapWidth, innerMapHeight, centerPoint);
        }
    }
}
