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
        public const float HardcodedStatusEffectsClearance = 220.0f;

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
                if (minimapInstance.m_smallRoot != null)
                {
                    RectTransform rootRectTransform = minimapInstance.m_smallRoot.GetComponent<RectTransform>();
                    if (rootRectTransform != null)
                    {
                        BaseRootPosition = rootRectTransform.anchoredPosition;
                        BaseRootSize = rootRectTransform.sizeDelta;
                    }
                }

                if (minimapInstance.m_windMarker != null)
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
                if (hudInstance.m_statusEffectListRoot != null)
                {
                    BaseStatusPosition = hudInstance.m_statusEffectListRoot.anchoredPosition;
                }
                s_capturedHudDefaults = true;
                Plugin.LogDebug($"HUD defaults captured: BaseStatusPosition={BaseStatusPosition}");
            }

            HudInitialized = true;
            ApplyLayout();
        }

        public static void UpdateViewportUv(Minimap minimapInstance, float innerWidth, float innerHeight, Vector3? centerWorldPosition = null)
        {
            if (minimapInstance == null || minimapInstance.m_mapImageSmall == null)
            {
                return;
            }

            Vector3 worldPosition = centerWorldPosition ?? (Player.m_localPlayer != null ? Player.m_localPlayer.transform.position : Vector3.zero);
            minimapInstance.WorldToMapPoint(worldPosition, out var mapPointX, out var mapPointY);

            Rect uvRectangle = minimapInstance.m_mapImageSmall.uvRect;
            uvRectangle.width = minimapInstance.SmallZoom * (innerWidth / NativeMapInnerSize);
            uvRectangle.height = minimapInstance.SmallZoom * (innerHeight / NativeMapInnerSize);
            uvRectangle.center = new Vector2(mapPointX, mapPointY);
            minimapInstance.m_mapImageSmall.uvRect = uvRectangle;
        }

        public static void ApplyLayout()
        {
            if (!MinimapInitialized || Minimap.instance == null || Minimap.instance.m_smallRoot == null)
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

            if (!Minimap.instance.m_smallRoot.TryGetComponent(out RectTransform rootRectTransform))
            {
                return;
            }

            rootRectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);

            if (ModConfig.ShiftBelowStatusEffects.Value && HudInitialized)
            {
                float verticalPosition = BaseStatusPosition.y - HardcodedStatusEffectsClearance + ModConfig.PositionOffsetY.Value;
                rootRectTransform.anchoredPosition = new Vector2(
                    BaseRootPosition.x + ModConfig.PositionOffsetX.Value,
                    verticalPosition
                );
            }
            else
            {
                rootRectTransform.anchoredPosition = new Vector2(
                    BaseRootPosition.x + ModConfig.PositionOffsetX.Value,
                    BaseRootPosition.y + ModConfig.PositionOffsetY.Value
                );
            }

            if (HudInitialized && Hud.instance && Hud.instance.m_statusEffectListRoot)
            {
                if (ModConfig.ShiftBelowStatusEffects.Value)
                {
                    Hud.instance.m_statusEffectListRoot.anchoredPosition = new Vector2(
                        BaseRootPosition.x,
                        BaseStatusPosition.y
                    );
                }
                else
                {
                    float minimapLeftEdge = (BaseRootPosition.x + ModConfig.PositionOffsetX.Value) - targetWidth;
                    float statusEffectsPositionX = minimapLeftEdge - NativeStatusGap;
                    Hud.instance.m_statusEffectListRoot.anchoredPosition = new Vector2(
                        statusEffectsPositionX,
                        BaseStatusPosition.y
                    );
                }
            }

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

            Plugin.LogDebug(
                $"Layout applied: size=({targetWidth:F0}x{targetHeight:F0}), pos={rootRectTransform.anchoredPosition}, " +
                $"statusPos={(Hud.instance && Hud.instance.m_statusEffectListRoot ? Hud.instance.m_statusEffectListRoot.anchoredPosition.ToString() : "N/A")}, " +
                $"shiftBelow={ModConfig.ShiftBelowStatusEffects.Value}");
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
            if (MinimapInitialized && Minimap.instance && Minimap.instance.m_smallRoot)
            {
                if (Minimap.instance.m_smallRoot.TryGetComponent(out RectTransform rootRectTransform))
                {
                    rootRectTransform.sizeDelta = BaseRootSize;
                    rootRectTransform.anchoredPosition = BaseRootPosition;
                }

                if (Minimap.instance.m_windMarker)
                {
                    Minimap.instance.m_windMarker.anchoredPosition = BaseWindPosition;
                }

                Minimap.instance.SmallZoom = 0.01f;
                UpdateViewportUv(Minimap.instance, NativeMapInnerSize, NativeMapInnerSize);

                Minimap.instance.m_pinUpdateRequired = true;
                RefreshPins();
            }

            if (HudInitialized && Hud.instance && Hud.instance.m_statusEffectListRoot)
            {
                Hud.instance.m_statusEffectListRoot.anchoredPosition = BaseStatusPosition;
            }

            Plugin.LogDebug("Restored native minimap and HUD layout defaults.");
        }
    }
}
