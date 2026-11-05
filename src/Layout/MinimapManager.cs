using System;
using MiniMapHeeler.Configuration;
using MiniMapHeeler.Patches;
using UnityEngine;

namespace MiniMapHeeler.Layout
{
    internal static class MinimapManager
    {
        public static Vector2 BaseRootSize = new Vector2(200f, 200f);
        public static Vector2 BaseRootPosition = new Vector2(-40f, -40f);
        public static Vector2 BaseStatusPosition = new Vector2(-271f, -45f);
        public static Vector2 BaseWindPosition = new Vector2(-78f, -78f);
        public const float HardcodedStatusRowsGap = 20.0f;

        public const float NativeMapPadding = 12.0f;
        public const float NativeMapInnerSize = 188.0f;
        public const float NativeStatusGap = 31.0f;

        public static bool MinimapInitialized;
        public static bool HudInitialized;
        private static bool s_capturedMinimapDefaults;
        private static bool s_capturedHudDefaults;

        public static void InitializeMinimap(Minimap minimapInstance)
        {
            if (!s_capturedMinimapDefaults)
            {
                if (minimapInstance.m_smallRoot)
                {
                    RectTransform rootRectTransform = minimapInstance.m_smallRoot.GetComponent<RectTransform>();
                    if (rootRectTransform)
                    {
                        BaseRootPosition = rootRectTransform.anchoredPosition;
                        BaseRootSize = rootRectTransform.sizeDelta;
                    }
                }

                if (minimapInstance.m_windMarker)
                {
                    BaseWindPosition = minimapInstance.m_windMarker.anchoredPosition;
                }

                s_capturedMinimapDefaults = true;
                Plugin.LogDebug($"Minimap defaults captured: BaseRootSize={BaseRootSize}, BaseRootPosition={BaseRootPosition}, BaseWindPosition={BaseWindPosition}");
            }

            MinimapInitialized = true;
            if (ModConfig.EnableMod.Value)
            {
                minimapInstance.SmallZoom = MinimapPatches.HardcodedDefaultZoom;
            }
            ApplyLayout();
        }

        public static void InitializeHud(Hud hudInstance)
        {
            if (!s_capturedHudDefaults)
            {
                if (hudInstance.m_statusEffectListRoot)
                {
                    BaseStatusPosition = hudInstance.m_statusEffectListRoot.anchoredPosition;
                }
                ShipHudLayout.CaptureDefaults(hudInstance);
                s_capturedHudDefaults = true;
                Plugin.LogDebug($"HUD defaults captured: BaseStatusPosition={BaseStatusPosition}");
            }

            HudInitialized = true;
            ApplyLayout();
        }

        public static void UpdateViewportUv(Minimap minimapInstance, float innerWidth, float innerHeight, Vector3? centerWorldPosition = null)
        {
            if (!minimapInstance || !minimapInstance.m_mapImageSmall)
            {
                return;
            }

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
            uvRectangle.width = minimapInstance.SmallZoom * (innerWidth / NativeMapInnerSize);
            uvRectangle.height = minimapInstance.SmallZoom * (innerHeight / NativeMapInnerSize);
            uvRectangle.center = new Vector2(mapPointX, mapPointY);
            minimapInstance.m_mapImageSmall.uvRect = uvRectangle;
        }

        public static void ApplyLayout()
        {
            if (!TryGetMinimapRoot(out RectTransform rootRectTransform))
            {
                return;
            }

            if (!ModConfig.EnableMod.Value)
            {
                RestoreDefaults();
                return;
            }

            float targetWidth = Mathf.Max(50f, ModConfig.MinimapWidth.Value);
            float targetHeight = Mathf.Max(50f, ModConfig.MinimapHeight.Value);

            rootRectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
            PlaceHudElements(rootRectTransform);

            if (Minimap.instance.m_windMarker)
            {
                float mapHalfWidth = (targetWidth - NativeMapPadding) / 2f;
                float mapHalfHeight = (targetHeight - NativeMapPadding) / 2f;
                Minimap.instance.m_windMarker.anchoredPosition = new Vector2(
                    -mapHalfWidth + 16f,
                    -mapHalfHeight + 16f
                );
            }

            float innerMapWidth = targetWidth - NativeMapPadding;
            float innerMapHeight = targetHeight - NativeMapPadding;
            UpdateViewportUv(Minimap.instance, innerMapWidth, innerMapHeight);

            Minimap.instance.m_pinUpdateRequired = true;
            RefreshPins();

            Plugin.LogDebug($"Layout applied: size=({targetWidth:F0}x{targetHeight:F0})");
        }

        public static void ApplyPositions()
        {
            if (ModConfig.EnableMod.Value && TryGetMinimapRoot(out RectTransform rootRectTransform))
            {
                PlaceHudElements(rootRectTransform);
            }
        }

        private static void PlaceHudElements(RectTransform rootRectTransform)
        {
            MinimapPlacementMode minimapPlacement = ModConfig.MinimapPlacement.Value;
            ShipHudPlacementMode shipHudPlacement = ModConfig.ShipHudPlacement.Value;
            bool shipHudShowing = HudLayoutTracker.ShipHudShowing;

            // detached measures from the screen top right corner, which is 0,0 in this space
            Vector2 minimapPosition = minimapPlacement == MinimapPlacementMode.DetachedFreeMovement
                ? Vector2.zero
                : BaseRootPosition;

            if (minimapPlacement == MinimapPlacementMode.ShiftBelowStatusEffects && HudReady && HudLayoutTracker.StatusRowCount > 0)
            {
                float statusRowsHeight = HudLayoutTracker.StatusRowCount * Hud.instance.m_statusEffectSpacing;
                minimapPosition.y = BaseStatusPosition.y - statusRowsHeight - HardcodedStatusRowsGap;
            }

            if (shipHudShowing && shipHudPlacement == ShipHudPlacementMode.AboveMinimap)
            {
                // ship hud row takes over the old minimap top while steering
                minimapPosition.y -= ShipHudLayout.GetScaledSize().y;
            }

            rootRectTransform.anchoredPosition = minimapPosition + new Vector2(ModConfig.MinimapOffsetX.Value, ModConfig.MinimapOffsetY.Value);

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

            if (Hud.instance.m_statusEffectListRoot)
            {
                Hud.instance.m_statusEffectListRoot.anchoredPosition = new Vector2(statusEffectsPositionX, BaseStatusPosition.y);
            }

            Plugin.LogDebug(
                $"HUD positions applied: minimap={rootRectTransform.anchoredPosition}, statusX={statusEffectsPositionX:F0}, " +
                $"statusRows={HudLayoutTracker.StatusRowCount}, shipHudShowing={shipHudShowing}, minimapPlacement={minimapPlacement}");
        }

        public static void RefreshPins()
        {
            if (!MinimapInitialized || !Minimap.instance)
            {
                return;
            }

            try
            {
                Minimap.instance.UpdatePins();
            }
            catch (Exception exception)
            {
                Plugin.Log.LogWarning($"Failed to update minimap pins: {exception.Message}");
                Plugin.LogDebug($"UpdatePins exception details: {exception}");
            }
        }

        public static void RestoreDefaults()
        {
            if (TryGetMinimapRoot(out RectTransform rootRectTransform))
            {
                rootRectTransform.sizeDelta = BaseRootSize;
                rootRectTransform.anchoredPosition = BaseRootPosition;

                if (Minimap.instance.m_windMarker)
                {
                    Minimap.instance.m_windMarker.anchoredPosition = BaseWindPosition;
                }

                Minimap.instance.SmallZoom = 0.01f;
                UpdateViewportUv(Minimap.instance, NativeMapInnerSize, NativeMapInnerSize);

                Minimap.instance.m_pinUpdateRequired = true;
                RefreshPins();
            }

            if (HudReady)
            {
                if (Hud.instance.m_statusEffectListRoot)
                {
                    Hud.instance.m_statusEffectListRoot.anchoredPosition = BaseStatusPosition;
                }

                ShipHudLayout.Restore(Hud.instance);
            }

            Plugin.LogDebug("Restored native minimap and HUD layout defaults.");
        }

        private static bool HudReady => HudInitialized && Hud.instance;

        private static bool TryGetMinimapRoot(out RectTransform rootRectTransform)
        {
            rootRectTransform = null!;
            return MinimapInitialized && Minimap.instance && Minimap.instance.m_smallRoot.TryGetComponent(out rootRectTransform);
        }
    }
}
