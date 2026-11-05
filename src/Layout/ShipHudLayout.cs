using MiniMapHeeler.Configuration;
using UnityEngine;

namespace MiniMapHeeler.Layout
{
    // ShipHud and the minimap container both fill hudroot and the pieces anchor to its top right corner,
    // so their positions share the minimap coordinate space
    // Controls is left alone, the game moves it to the tiller every LateUpdate
    internal static class ShipHudLayout
    {
        private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        private static readonly Vector2 TopRight = new Vector2(1f, 1f);
        private static readonly Vector2 TopCenter = new Vector2(0.5f, 1f);
        private static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);

        private static Vector2 s_baseSpeedIconPosition;
        private static Vector2 s_baseWindIndicatorPosition;
        private static Vector2 s_speedIconExtents;
        private static Vector2 s_windIndicatorExtents;

        public static void CaptureDefaults(Hud hudInstance)
        {
            RectTransform speedIcon = GetSpeedIcon(hudInstance);
            RectTransform windIcon = hudInstance.m_shipWindIcon.rectTransform;

            s_baseSpeedIconPosition = speedIcon.anchoredPosition;
            s_baseWindIndicatorPosition = hudInstance.m_shipWindIndicatorRoot.anchoredPosition;
            s_speedIconExtents = speedIcon.sizeDelta / 2f;

            // wind icon circles the indicator center so its outer edge sets how far the indicator reaches
            float windReach = windIcon.anchoredPosition.magnitude + windIcon.sizeDelta.x / 2f;
            s_windIndicatorExtents = new Vector2(windReach, windReach);

            Plugin.LogDebug($"Ship HUD defaults captured: speedIcon={s_baseSpeedIconPosition}, windIndicator={s_baseWindIndicatorPosition}, windReach={windReach:F1}");
        }

        public static Vector2 GetScaledSize() =>
            GetGroupRect(GetWindIndicatorCenter(ModConfig.ShipHudPlacement.Value)).size * ModConfig.ShipHudScale.Value;

        public static Rect Apply(Hud hudInstance, RectTransform minimapRectTransform)
        {
            ShipHudPlacementMode placement = ModConfig.ShipHudPlacement.Value;
            Vector2 windIndicatorCenter = GetWindIndicatorCenter(placement);
            Rect groupRect = GetGroupRect(windIndicatorCenter);

            Vector2 minimapPivot = minimapRectTransform.pivot;
            Rect minimapRect = GetHudRect(minimapRectTransform.anchoredPosition, minimapRectTransform.sizeDelta, minimapPivot);
            Rect baseMinimapRect = GetHudRect(MinimapManager.BaseRootPosition, MinimapManager.BaseRootSize, minimapPivot);

            (Vector2 anchorPoint, Vector2 groupPoint) = placement switch
            {
                // detached modes measure from the screen top right corner, which is 0,0 in this space
                ShipHudPlacementMode.FullyDetachedVertical or ShipHudPlacementMode.FullyDetachedHorizontal => (
                    Vector2.zero,
                    Rect.NormalizedToPoint(groupRect, TopRight)),
                ShipHudPlacementMode.BelowMinimap => (
                    Rect.NormalizedToPoint(minimapRect, BottomCenter),
                    Rect.NormalizedToPoint(groupRect, TopCenter)),
                ShipHudPlacementMode.LeftOfMinimap => (
                    Rect.NormalizedToPoint(minimapRect, TopLeft),
                    Rect.NormalizedToPoint(groupRect, TopRight)),
                ShipHudPlacementMode.AboveMinimap => (
                    Rect.NormalizedToPoint(minimapRect, TopCenter),
                    Rect.NormalizedToPoint(groupRect, BottomCenter)),
                // Default keeps the base game stack, measured from the base game minimap so the base game gap stays exact
                _ => (
                    Rect.NormalizedToPoint(minimapRect, BottomCenter),
                    Rect.NormalizedToPoint(baseMinimapRect, BottomCenter))
            };

            Vector2 target = anchorPoint + new Vector2(ModConfig.ShipHudOffsetX.Value, ModConfig.ShipHudOffsetY.Value);
            float scale = ModConfig.ShipHudScale.Value;
            Vector2 speedIconPosition = target + (s_baseSpeedIconPosition - groupPoint) * scale;
            Vector2 windIndicatorPosition = target + (windIndicatorCenter - groupPoint) * scale;

            Place(GetSpeedIcon(hudInstance), speedIconPosition, scale);
            Place(hudInstance.m_shipWindIndicatorRoot, windIndicatorPosition, scale);

            Rect placedRect = new Rect(target + (groupRect.position - groupPoint) * scale, groupRect.size * scale);
            Plugin.LogDebug($"Ship HUD layout applied: placement={placement}, scale={scale:F2}, speedIcon={speedIconPosition}, windIndicator={windIndicatorPosition}, bounds={placedRect}");
            return placedRect;
        }

        public static void Restore(Hud hudInstance)
        {
            Place(GetSpeedIcon(hudInstance), s_baseSpeedIconPosition, 1f);
            Place(hudInstance.m_shipWindIndicatorRoot, s_baseWindIndicatorPosition, 1f);
        }

        private static Vector2 GetWindIndicatorCenter(ShipHudPlacementMode placement) =>
            placement is ShipHudPlacementMode.BelowMinimap or ShipHudPlacementMode.AboveMinimap or ShipHudPlacementMode.FullyDetachedHorizontal
                ? s_baseSpeedIconPosition + new Vector2(Vector2.Distance(s_baseSpeedIconPosition, s_baseWindIndicatorPosition), 0f)
                : s_baseWindIndicatorPosition;

        private static Rect GetGroupRect(Vector2 windIndicatorCenter)
        {
            Vector2 groupMin = Vector2.Min(s_baseSpeedIconPosition - s_speedIconExtents, windIndicatorCenter - s_windIndicatorExtents);
            Vector2 groupMax = Vector2.Max(s_baseSpeedIconPosition + s_speedIconExtents, windIndicatorCenter + s_windIndicatorExtents);
            return Rect.MinMaxRect(groupMin.x, groupMin.y, groupMax.x, groupMax.y);
        }

        // speed icon has no hud field, m_rudder is its direct child
        private static RectTransform GetSpeedIcon(Hud hudInstance) => (RectTransform)hudInstance.m_rudder.transform.parent;

        // anchoredPosition is where the pivot sits so step back to the bottom left corner
        private static Rect GetHudRect(Vector2 anchoredPosition, Vector2 size, Vector2 pivot) =>
            new Rect(anchoredPosition - Vector2.Scale(size, pivot), size);

        private static void Place(RectTransform piece, Vector2 anchoredPosition, float scale)
        {
            piece.anchoredPosition = anchoredPosition;
            piece.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
