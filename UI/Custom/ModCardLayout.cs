using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Grid of styled cards for item display.
/// Uses GridLayoutGroup for even spacing with hover effects.
/// </summary>
namespace ModFramework.UI.Custom
{
    public class ModCardLayout
    {
        public GameObject Root;
        private int _columns;
        private float _cardHeight;

        /// <summary>
        /// Create a card grid layout.
        /// </summary>
        public static ModCardLayout Create(GameObject parent, int columns = 3, float cardHeight = 120f)
        {
            var layout = new ModCardLayout();
            layout._columns = columns;
            layout._cardHeight = cardHeight;

            layout.Root = new GameObject("ModCardLayout");
            RectTransform rootRect = layout.Root.AddComponent<RectTransform>();
            rootRect.SetParent(parent.transform, false);

            GridLayoutGroup grid = layout.Root.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.cellSize = new Vector2(0, cardHeight); // Width will be set dynamically
            grid.spacing = new Vector2(8f, 8f);
            grid.padding = new RectOffset(4, 4, 4, 4);
            grid.childAlignment = TextAnchor.UpperLeft;

            LayoutElement rootLe = layout.Root.AddComponent<LayoutElement>();
            rootLe.flexibleWidth = 1f;

            ContentSizeFitter csf = layout.Root.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Use a helper to set cell width after layout resolves
            var sizer = layout.Root.AddComponent<CardLayoutSizer>();
            sizer.Setup(grid, columns);

            return layout;
        }

        /// <summary>
        /// Add a card with title, subtitle, and click handler.
        /// Returns the card's content panel for adding custom content.
        /// </summary>
        public GameObject AddCard(string title, string subtitle = "", Action onClick = null)
        {
            GameObject card = new GameObject("Card_" + title);
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.SetParent(Root.transform, false);

            // Background
            Image cardBg = card.AddComponent<Image>();
            cardBg.color = GameTheme.PanelBackground;

            // Internal layout
            VerticalLayoutGroup vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 4f;
            vlg.padding = new RectOffset(8, 8, 8, 8);

            // Title
            if (!string.IsNullOrEmpty(title))
            {
                GameObject titleObj = new GameObject("Title");
                RectTransform titleRect = titleObj.AddComponent<RectTransform>();
                titleRect.SetParent(card.transform, false);
                Text titleText = titleObj.AddComponent<Text>();
                titleText.text = title;
                titleText.font = GameTheme.GameFont;
                titleText.fontSize = GameTheme.DefaultFontSize;
                titleText.fontStyle = FontStyle.Bold;
                titleText.color = GameTheme.HeaderColor;
                titleText.alignment = TextAnchor.UpperLeft;
            }

            // Subtitle
            if (!string.IsNullOrEmpty(subtitle))
            {
                GameObject subObj = new GameObject("Subtitle");
                RectTransform subRect = subObj.AddComponent<RectTransform>();
                subRect.SetParent(card.transform, false);
                Text subText = subObj.AddComponent<Text>();
                subText.text = subtitle;
                subText.font = GameTheme.GameFont;
                subText.fontSize = GameTheme.SmallFontSize;
                subText.color = GameTheme.LabelColor;
                subText.alignment = TextAnchor.UpperLeft;
            }

            // Content area for custom widgets
            GameObject content = new GameObject("Content");
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.SetParent(card.transform, false);

            VerticalLayoutGroup contentVlg = content.AddComponent<VerticalLayoutGroup>();
            contentVlg.childControlWidth = true;
            contentVlg.childControlHeight = true;
            contentVlg.childForceExpandWidth = true;
            contentVlg.childForceExpandHeight = false;
            contentVlg.spacing = 2f;

            LayoutElement contentLe = content.AddComponent<LayoutElement>();
            contentLe.flexibleHeight = 1f;

            // Click handler
            if (onClick != null)
            {
                Button btn = card.AddComponent<Button>();
                btn.onClick.AddListener(() => onClick.Invoke());
            }

            // Hover effect
            HoverHandler hover = card.AddComponent<HoverHandler>();
            hover.Setup(cardBg);

            return content;
        }
    }

    /// <summary>
    /// Helper to dynamically set GridLayoutGroup cell width based on parent width.
    /// </summary>
    internal class CardLayoutSizer : MonoBehaviour
    {
        private GridLayoutGroup _grid;
        private int _columns;
        private bool _sized;

        public void Setup(GridLayoutGroup grid, int columns)
        {
            _grid = grid;
            _columns = columns;
        }

        void LateUpdate()
        {
            if (_sized) return;

            RectTransform rect = GetComponent<RectTransform>();
            float totalWidth = rect.rect.width;
            if (totalWidth <= 0) return;

            float padding = _grid.padding.left + _grid.padding.right;
            float spacing = _grid.spacing.x * (_columns - 1);
            float cellWidth = (totalWidth - padding - spacing) / _columns;

            if (cellWidth > 10f)
            {
                _grid.cellSize = new Vector2(cellWidth, _grid.cellSize.y);
                _sized = true;
            }
        }
    }
}
