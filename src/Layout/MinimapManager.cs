using MiniMapHeeler.Configuration;
using UnityEngine;

namespace MiniMapHeeler.Layout
{
    internal static class MinimapManager
    {
        private const float HardcodedDefaultZoom = 0.015f;
        private const float HardcodedMinZoom = 0.002f;
        private const float HardcodedMaxZoom = 1.0f;
        private const float HardcodedStatusRowsGap = 20.0f;

        private const float NativeMapPadding = 12.0f;
        private const float NativeMapInnerSize = 188.0f;
        private const float NativeStatusGap = 31.0f;

        private static Vector2 s_baseStatusPosition = new Vector2(-271f, -45f);
        private static Vector2 s_baseWindPosition = new Vector2(-78f, -78f);
        private static bool s_minimapInitialized;
        private static bool s_hudInitialized;
        private static bool s_capturedMinimapDefaults;
        private static bool s_capturedHudDefaults;

        public static Vector2 BaseRootSize { get; private set; } = new Vector2(200f, 200f);
        public static Vector2 BaseRootPosition { get; private set; } = new Vector2(-40f, -40f);

        public static void InitializeMinimap(Minimap minimapInstance)
        {
            if (!s_capturedMinimapDefaults)
            {
                RectTransform rootRectTransform = (RectTransform)minimapInstance.m_smallRoot.transform;
                BaseRootPosition = rootRectTransform.anchoredPosition;
                BaseRootSize = rootRectTransform.sizeDelta;
                s_baseWindPosition = minimapInstance.m_windMarker.anchoredPosition;

                s_capturedMinimapDefaults = true;
                Plugin.LogDebug($"Minimap defaults captured: BaseRootSize={BaseRootSize}, BaseRootPosition={BaseRootPosition}, BaseWindPosition={s_baseWindPosition}");
            }

            s_minimapInitialized = true;
            if (ModConfig.Instance.EnableMod.Value)
            {
                minimapInstance.SmallZoom = HardcodedDefaultZoom;
            }
            ApplyLayout();
        }

        public static void InitializeHud(Hud hudInstance)
        {
            if (!s_capturedHudDefaults)
            {
                s_baseStatusPosition = hudInstance.m_statusEffectListRoot.anchoredPosition;
                ShipHudLayout.CaptureDefaults(hudInstance);
                s_capturedHudDefaults = true;
                Plugin.LogDebug($"HUD defaults captured: BaseStatusPosition={s_baseStatusPosition}");
            }

            s_hudInitialized = true;
            ApplyLayout();
        }

        // returns true to let the game setter run, false when the mod already set the zoom past the game clamp
        public static bool HandleSmallZoomSet(Minimap minimapInstance, float value)
        {
            if (!ModConfig.Instance.EnableMod.Value)
            {
                return true;
            }

            float current = minimapInstance.SmallZoom;

            // the game zooms in at 0.5x but out at 1.5x so both directions use 1.5x here
            bool isZoomIn = Mathf.Approximately(value, current * 0.5f);
            bool isZoomOut = Mathf.Approximately(value, current * 1.5f);

            if (isZoomIn)
            {
                float newZoom = Mathf.Clamp(current / 1.5f, HardcodedMinZoom, HardcodedMaxZoom);
                minimapInstance.m_smallZoom = newZoom;
                Plugin.LogDebug($"Zoom in: {current:F4} -> {newZoom:F4}");
                return false;
            }

            if (isZoomOut)
            {
                float newZoom = Mathf.Clamp(current * 1.5f, HardcodedMinZoom, HardcodedMaxZoom);
                minimapInstance.m_smallZoom = newZoom;
                Plugin.LogDebug($"Zoom out: {current:F4} -> {newZoom:F4}");
                return false;
            }

            minimapInstance.m_smallZoom = Mathf.Clamp(value, HardcodedMinZoom, HardcodedMaxZoom);
            return false;
        }

        public static void HandleCenterMap(Minimap minimapInstance, Vector3 centerPoint)
        {
            if (ModConfig.Instance.EnableMod.Value && minimapInstance.m_mode == Minimap.MapMode.Small)
            {
                UpdateViewportUv(minimapInstance, GetInnerMapSize(), centerPoint);
            }
        }

        private static void UpdateViewportUv(Minimap minimapInstance, Vector2 innerMapSize, Vector3? centerWorldPosition = null)
        {
            Vector3 worldPosition = Vector3.zero;
            if (centerWorldPosition.HasValue)
            {
                worldPosition = centerWorldPosition.Value;
            }
            else if (Player.m_localPlayer)
            {
                worldPosition = Player.m_localPlayer.transform.position;
            }
            minimapInstance.WorldToMapPoint(worldPosition, out float mapPointX, out float mapPointY);

            Rect uvRectangle = minimapInstance.m_mapImageSmall.uvRect;
            uvRectangle.width = minimapInstance.SmallZoom * (innerMapSize.x / NativeMapInnerSize);
            uvRectangle.height = minimapInstance.SmallZoom * (innerMapSize.y / NativeMapInnerSize);
            uvRectangle.center = new Vector2(mapPointX, mapPointY);
            minimapInstance.m_mapImageSmall.uvRect = uvRectangle;
        }

        public static void ApplyLayout()
        {
            // no restore here, that reset the minimap zoom every time the big map closed with the mod disabled
            if (!ModConfig.Instance.EnableMod.Value || !TryGetMinimapRoot(out RectTransform rootRectTransform))
            {
                return;
            }

            Vector2 targetSize = new Vector2(ModConfig.Instance.MinimapWidth.Value, ModConfig.Instance.MinimapHeight.Value);
            rootRectTransform.sizeDelta = targetSize;
            PlaceHudElements(rootRectTransform);

            Vector2 innerMapSize = GetInnerMapSize();
            Minimap.instance.m_windMarker.anchoredPosition = new Vector2(
                -innerMapSize.x / 2f + 16f,
                -innerMapSize.y / 2f + 16f
            );

            UpdateViewportUv(Minimap.instance, innerMapSize);
            Minimap.instance.m_pinUpdateRequired = true;

            Plugin.LogDebug($"Layout applied: size=({targetSize.x:F0}x{targetSize.y:F0})");
        }

        public static void ApplyPositions()
        {
            if (ModConfig.Instance.EnableMod.Value && TryGetMinimapRoot(out RectTransform rootRectTransform))
            {
                PlaceHudElements(rootRectTransform);
            }
        }

        private static void PlaceHudElements(RectTransform rootRectTransform)
        {
            MinimapPlacementMode minimapPlacement = ModConfig.Instance.MinimapPlacement.Value;
            ShipHudPlacementMode shipHudPlacement = ModConfig.Instance.ShipHudPlacement.Value;
            bool shipHudShowing = HudLayoutTracker.ShipHudShowing;

            // detached measures from the screen top right corner, which is 0,0 in this space
            Vector2 minimapPosition = minimapPlacement == MinimapPlacementMode.DetachedFreeMovement
                ? Vector2.zero
                : BaseRootPosition;

            if (minimapPlacement == MinimapPlacementMode.ShiftBelowStatusEffects && HudReady && HudLayoutTracker.StatusRowCount > 0)
            {
                float statusRowsHeight = HudLayoutTracker.StatusRowCount * Hud.instance.m_statusEffectSpacing;
                minimapPosition.y = s_baseStatusPosition.y - statusRowsHeight - HardcodedStatusRowsGap;
            }

            if (shipHudShowing && shipHudPlacement == ShipHudPlacementMode.AboveMinimap)
            {
                // ship hud row takes over the old minimap top while steering
                minimapPosition.y -= ShipHudLayout.GetScaledSize().y;
            }

            rootRectTransform.anchoredPosition = minimapPosition + new Vector2(ModConfig.Instance.MinimapOffsetX.Value, ModConfig.Instance.MinimapOffsetY.Value);

            if (!HudReady)
            {
                return;
            }

            Rect shipHudRect = ShipHudLayout.Apply(Hud.instance, rootRectTransform);

            // in Default status icons sit left of the minimap, or left of the ship hud column while steering in that mode,
            // the other minimap modes put them on the right screen edge
            float statusEffectsRightEdge = shipHudShowing && shipHudPlacement == ShipHudPlacementMode.LeftOfMinimap
                ? shipHudRect.xMin
                : rootRectTransform.anchoredPosition.x - rootRectTransform.sizeDelta.x;
            float statusEffectsPositionX = minimapPlacement == MinimapPlacementMode.Default
                ? statusEffectsRightEdge - NativeStatusGap
                : BaseRootPosition.x;

            Hud.instance.m_statusEffectListRoot.anchoredPosition = new Vector2(statusEffectsPositionX, s_baseStatusPosition.y);

            Plugin.LogDebug(
                $"HUD positions applied: minimap={rootRectTransform.anchoredPosition}, statusX={statusEffectsPositionX:F0}, " +
                $"statusRows={HudLayoutTracker.StatusRowCount}, shipHudShowing={shipHudShowing}, minimapPlacement={minimapPlacement}");
        }

        public static void RestoreDefaults()
        {
            if (TryGetMinimapRoot(out RectTransform rootRectTransform))
            {
                rootRectTransform.sizeDelta = BaseRootSize;
                rootRectTransform.anchoredPosition = BaseRootPosition;
                Minimap.instance.m_windMarker.anchoredPosition = s_baseWindPosition;

                Minimap.instance.SmallZoom = 0.01f;
                UpdateViewportUv(Minimap.instance, new Vector2(NativeMapInnerSize, NativeMapInnerSize));
                Minimap.instance.m_pinUpdateRequired = true;
            }

            if (HudReady)
            {
                Hud.instance.m_statusEffectListRoot.anchoredPosition = s_baseStatusPosition;
                ShipHudLayout.Restore(Hud.instance);
            }

            Plugin.LogDebug("Restored base game minimap and HUD layout defaults.");
        }

        private static Vector2 GetInnerMapSize() => new Vector2(
            ModConfig.Instance.MinimapWidth.Value - NativeMapPadding,
            ModConfig.Instance.MinimapHeight.Value - NativeMapPadding);

        private static bool HudReady => s_hudInitialized && Hud.instance;

        private static bool TryGetMinimapRoot(out RectTransform rootRectTransform)
        {
            rootRectTransform = null!;
            return s_minimapInitialized && Minimap.instance && Minimap.instance.m_smallRoot.TryGetComponent(out rootRectTransform);
        }
    }
}
