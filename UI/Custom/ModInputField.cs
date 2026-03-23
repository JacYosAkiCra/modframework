using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Themed input fields: single-line, multi-line (TextArea), search field, and numeric input.
/// </summary>
namespace ModFramework.UI.Custom
{
    public static class ModInputField
    {
        /// <summary>
        /// Creates a themed single-line input field.
        /// </summary>
        public static InputField Create(string initial, Action<string> onChange, GameObject parent)
        {
            GameTheme.Initialize();

            GameObject obj = new GameObject("ModInputField");
            obj.transform.SetParent(parent.transform, false);

            // Background
            Image bg = obj.AddComponent<Image>();
            bg.color = GameTheme.InputBackground;

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = GameTheme.RowHeight;
            le.flexibleWidth = 1f;

            // Text area
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(obj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.font = GameTheme.GameFont;
            text.fontSize = GameTheme.DefaultFontSize;
            text.color = GameTheme.LabelColor;
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(6f, 2f);
            textRect.offsetMax = new Vector2(-6f, -2f);

            // Placeholder
            GameObject phObj = new GameObject("Placeholder");
            phObj.transform.SetParent(obj.transform, false);
            Text placeholder = phObj.AddComponent<Text>();
            placeholder.font = GameTheme.GameFont;
            placeholder.fontSize = GameTheme.DefaultFontSize;
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.color = new Color(GameTheme.LabelColor.r, GameTheme.LabelColor.g, GameTheme.LabelColor.b, 0.4f);
            placeholder.alignment = TextAnchor.MiddleLeft;
            placeholder.text = "";

            RectTransform phRect = phObj.GetComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = new Vector2(6f, 2f);
            phRect.offsetMax = new Vector2(-6f, -2f);

            // InputField component
            InputField input = obj.AddComponent<InputField>();
            input.textComponent = text;
            input.placeholder = placeholder;
            input.text = initial ?? "";
            input.caretColor = GameTheme.LabelColor;

            if (onChange != null)
                input.onValueChanged.AddListener(val => onChange(val));

            return input;
        }
    }

    public static class ModTextArea
    {
        /// <summary>
        /// Creates a themed multi-line text area.
        /// </summary>
        public static InputField Create(string initial, int lines, Action<string> onChange, GameObject parent)
        {
            InputField input = ModInputField.Create(initial, onChange, parent);
            input.lineType = InputField.LineType.MultiLineNewline;

            LayoutElement le = input.GetComponent<LayoutElement>();
            le.preferredHeight = GameTheme.RowHeight * lines;

            return input;
        }
    }

    public static class ModSearchField
    {
        /// <summary>
        /// Creates a themed search input with placeholder text.
        /// </summary>
        public static InputField Create(string placeholderText, Action<string> onSearch, GameObject parent)
        {
            InputField input = ModInputField.Create("", onSearch, parent);

            // Update placeholder text
            Text placeholder = input.placeholder as Text;
            if (placeholder != null)
                placeholder.text = placeholderText ?? "Search...";

            return input;
        }
    }

    public static class ModNumericInput
    {
        /// <summary>
        /// Creates a themed integer input with min/max clamping.
        /// </summary>
        public static InputField Create(int value, int min, int max, Action<int> onChange, GameObject parent)
        {
            InputField input = ModInputField.Create(value.ToString(), null, parent);
            input.contentType = InputField.ContentType.IntegerNumber;
            input.onValueChanged.AddListener(val =>
            {
                int parsed;
                if (int.TryParse(val, out parsed))
                {
                    parsed = Mathf.Clamp(parsed, min, max);
                    if (onChange != null) onChange(parsed);
                }
            });

            return input;
        }
    }
}
