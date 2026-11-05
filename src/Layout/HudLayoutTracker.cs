using UnityEngine;

namespace MiniMapHeeler.Layout
{
    // status rows and the ship hud both change what the minimap and status icons make room for,
    // so they get checked every frame and positions rerun only when one changes
    internal static class HudLayoutTracker
    {
        public static int StatusRowCount { get; private set; }
        public static bool ShipHudShowing { get; private set; }

        public static void CheckForChanges(Hud hudInstance)
        {
            int statusRowCount = Mathf.CeilToInt((float)hudInstance.m_statusEffects.Count / hudInstance.m_effectsPerRow);
            bool shipHudShowing = hudInstance.m_shipHudRoot.activeSelf;
            if (statusRowCount == StatusRowCount && shipHudShowing == ShipHudShowing)
            {
                return;
            }

            StatusRowCount = statusRowCount;
            ShipHudShowing = shipHudShowing;
            MinimapManager.ApplyPositions();
        }
    }
}
