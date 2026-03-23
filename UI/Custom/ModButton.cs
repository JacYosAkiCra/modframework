using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Themed button with hover/press transitions.
/// </summary>
namespace ModFramework.UI.Custom
{
    public static class ModButton
    {
        /// <summary>
        /// Creates a themed button.
        /// </summary>
        public static GameObject Create(string text, Action onClick, GameObject parent)
        {
            GameTheme.Initialize();

            GameObject btnObj = new GameObject("ModButton");
            btnObj.transform.SetParent(parent.transform, false);

            // Background
            Image bg = btnObj.AddComponent<Image>();
            bg.color = GameTheme.ButtonNormal;

            // Button component with themed color transitions
            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = GameTheme.ButtonNormal;
            colors.highlightedColor = GameTheme.ButtonHover;
            colors.pressedColor = GameTheme.ButtonPressed;
            colors.disabledColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            btn.colors = colors;
            btn.onClick.AddListener(new UnityAction(() => onClick()));

            // Layout sizing
            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = GameTheme.ButtonHeight;
            le.flexibleWidth = 1f;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);
            Text label = labelObj.AddComponent<Text>();
            label.text = text;
            label.font = GameTheme.GameFont;
            label.fontSize = GameTheme.DefaultFontSize;
            label.color = GameTheme.LabelColor;
            label.alignment = TextAnchor.MiddleCenter;

            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;

            return btnObj;
        }

        /// <summary>
        /// Creates a fixed-width button (not stretchy).
        /// </summary>
        public static GameObject Create(string text, Action onClick, GameObject parent, float width)
        {
            GameObject btn = Create(text, onClick, parent);
            LayoutElement le = btn.GetComponent<LayoutElement>();
            le.preferredWidth = width;
            le.flexibleWidth = 0f;
            return btn;
        }
    }
}
