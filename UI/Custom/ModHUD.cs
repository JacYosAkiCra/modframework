using System;
using UnityEngine;
using UnityEngine.UI;

namespace ModFramework.UI.Custom
{
    /// <summary>
    /// A lightweight Heads-Up Display overlay that attaches directly to the game's Canvas.
    /// Used for persistent, unclosable UI elements (like resource trackers or minimaps) 
    /// that sit behind typical windows.
    /// </summary>
    public class ModHUD
    {
        public GameObject Root { get; private set; }
        public CanvasGroup Group { get; private set; }

        private ModHUD() { }

        /// <summary>
        /// Creates a new HUD overlay.
        /// </summary>
        /// <param name="name">Name of the GameObject.</param>
        /// <param name="anchorTarget">Defines which corner/edge to stick to (e.g. UpperRight).</param>
        /// <param name="offset">Pixel offset from the anchor.</param>
        /// <param name="blocksRaycasts">If true, the HUD can be clicked. If false, clicks pass through to the game.</param>
        public static ModHUD Create(string name, TextAnchor anchorTarget, Vector2 offset, bool blocksRaycasts = false)
        {
            GameTheme.Initialize();

            ModHUD hud = new ModHUD();
            hud.Root = new GameObject("ModHUD_" + name);
            
            // Attach to global canvas so it always renders on top of the 3D world
            hud.Root.transform.SetParent(WindowManager.Instance.Canvas.transform, false);

            RectTransform rect = hud.Root.AddComponent<RectTransform>();
            Vector2 anchor = GetAnchor(anchorTarget);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor; // Pivot matches anchor so it grows inwards
            rect.anchoredPosition = offset;

            // Auto-resize based on children
            ContentSizeFitter csf = hud.Root.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            VerticalLayoutGroup vlg = hud.Root.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            hud.Group = hud.Root.AddComponent<CanvasGroup>();
            hud.Group.blocksRaycasts = blocksRaycasts;
            hud.Group.interactable = blocksRaycasts;

            return hud;
        }

        public void Destroy()
        {
            if (Root != null)
            {
                UnityEngine.Object.Destroy(Root);
            }
        }

        private static Vector2 GetAnchor(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return new Vector2(0f, 1f);
                case TextAnchor.UpperCenter: return new Vector2(0.5f, 1f);
                case TextAnchor.UpperRight: return new Vector2(1f, 1f);
                case TextAnchor.MiddleLeft: return new Vector2(0f, 0.5f);
                case TextAnchor.MiddleCenter: return new Vector2(0.5f, 0.5f);
                case TextAnchor.MiddleRight: return new Vector2(1f, 0.5f);
                case TextAnchor.LowerLeft: return new Vector2(0f, 0f);
                case TextAnchor.LowerCenter: return new Vector2(0.5f, 0f);
                case TextAnchor.LowerRight: return new Vector2(1f, 0f);
                default: return new Vector2(0f, 1f);
            }
        }
    }
}
