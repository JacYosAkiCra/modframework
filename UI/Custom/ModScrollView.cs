using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Themed scrollable container. Returns the content panel to add widgets to.
/// Uses anchor-based sizing to properly fill its parent.
/// </summary>
namespace ModFramework.UI.Custom
{
    public static class ModScrollView
    {
        /// <summary>
        /// Creates a themed scrollable container that fills its parent.
        /// </summary>
        /// <param name="contentHeight">Initial total content height (auto-expands with ContentSizeFitter).</param>
        /// <param name="parent">Parent to attach to.</param>
        /// <returns>The content panel — add your widgets as children of this.</returns>
        public static GameObject Create(float contentHeight, GameObject parent)
        {
            GameTheme.Initialize();

            // Scroll view container — fills parent
            GameObject scrollObj = new GameObject("ModScrollView");
            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.SetParent(parent.transform, false);
            scrollObj.AddComponent<Image>().color = Color.clear;

            // Use LayoutElement to claim all available space in parent's layout group
            LayoutElement rootLE = scrollObj.AddComponent<LayoutElement>();
            rootLE.flexibleWidth = 1f;
            rootLE.flexibleHeight = 1f;
            rootLE.preferredHeight = contentHeight; // For VerticalLayoutGroups that need a hint

            // Viewport (masks overflowing content)
            GameObject viewport = new GameObject("Viewport");
            RectTransform vpRect = viewport.AddComponent<RectTransform>();
            vpRect.SetParent(scrollRect, false);
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.sizeDelta = new Vector2(-10f, 0f); // Leave space for scrollbar
            vpRect.offsetMin = new Vector2(0f, 0f);
            vpRect.offsetMax = new Vector2(-10f, 0f);
            vpRect.anchoredPosition = Vector2.zero;
            viewport.AddComponent<Image>().color = Color.white;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            // Content panel — where widgets get added
            GameObject content = new GameObject("Content");
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.SetParent(vpRect, false);
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0f, 1f);
            contentRect.sizeDelta = new Vector2(0f, contentHeight);
            contentRect.anchoredPosition = Vector2.zero;

            // Vertical layout for auto-stacking child widgets
            VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.spacing = GameTheme.SectionSpacing;
            contentLayout.padding = new RectOffset(4, 4, 4, 4);

            // Auto-expand height based on children
            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ScrollRect component
            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.content = contentRect;
            scroll.viewport = vpRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = true;
            scroll.scrollSensitivity = 20f;

            // Scrollbar (slim, right side)
            GameObject barObj = new GameObject("Scrollbar");
            RectTransform barRect = barObj.AddComponent<RectTransform>();
            barRect.SetParent(scrollRect, false);
            barRect.anchorMin = new Vector2(1f, 0f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.pivot = new Vector2(1f, 0.5f);
            barRect.sizeDelta = new Vector2(8f, 0f);
            barRect.anchoredPosition = Vector2.zero;
            barObj.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.05f);

            Scrollbar bar = barObj.AddComponent<Scrollbar>();
            bar.direction = Scrollbar.Direction.BottomToTop;

            // Handle
            GameObject handleObj = new GameObject("Handle");
            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.SetParent(barRect, false);
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.sizeDelta = Vector2.zero;
            Image handleImg = handleObj.AddComponent<Image>();
            handleImg.color = GameTheme.ScrollbarHandle;

            bar.handleRect = handleRect;
            bar.targetGraphic = handleImg;

            scroll.verticalScrollbar = bar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

            return content;
        }
    }
}
