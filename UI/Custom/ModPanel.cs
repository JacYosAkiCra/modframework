using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A themed container panel with optional VerticalLayoutGroup.
/// Use for grouping sections of widgets inside a ModWindow.
/// </summary>
namespace ModFramework.UI.Custom
{
    public static class ModPanel
    {
        /// <summary>
        /// Creates a themed panel with a VerticalLayoutGroup for auto-stacking child widgets.
        /// </summary>
        /// <param name="parent">Parent GameObject to attach to.</param>
        /// <param name="withBackground">If true, panel gets a subtle background tint for visual grouping.</param>
        /// <returns>The panel GameObject - add widgets as children of this.</returns>
        public static GameObject Create(GameObject parent, bool withBackground = false)
        {
            GameTheme.Initialize();

            GameObject panelObj = new GameObject("ModPanel");
            RectTransform rect = panelObj.AddComponent<RectTransform>();
            rect.SetParent(parent.transform, false);

            // Fill parent by default
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            if (withBackground)
            {
                Image bg = panelObj.AddComponent<Image>();
                bg.color = GameTheme.PanelBackground;
            }

            // Add layout group for auto-stacking
            VerticalLayoutGroup layout = panelObj.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = GameTheme.SectionSpacing;
            layout.padding = new RectOffset(
                (int)GameTheme.WindowPadding,
                (int)GameTheme.WindowPadding,
                (int)GameTheme.WindowPadding,
                (int)GameTheme.WindowPadding
            );

            return panelObj;
        }

        /// <summary>
        /// Creates a horizontal layout panel (for putting widgets side-by-side).
        /// </summary>
        public static GameObject CreateHorizontal(GameObject parent, float spacing = -1f)
        {
            GameTheme.Initialize();

            GameObject panelObj = new GameObject("ModHPanel");
            RectTransform rect = panelObj.AddComponent<RectTransform>();
            rect.SetParent(parent.transform, false);

            HorizontalLayoutGroup layout = panelObj.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true; // Crucial for wide rows and expanding cells
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = spacing >= 0 ? spacing : GameTheme.SectionSpacing;

            // Set preferred height
            LayoutElement le = panelObj.AddComponent<LayoutElement>();
            le.preferredHeight = GameTheme.RowHeight;

            return panelObj;
        }
    }
}
