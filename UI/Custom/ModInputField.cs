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

            // Instead of building from scratch, spawn the native game input box
            // This explicitly tells Software Inc. to block hotkey evaluation while typing
            InputField input = WindowManager.SpawnInputbox();
            GameObject obj = input.gameObject;
            obj.name = "ModInputField";
            obj.transform.SetParent(parent.transform, false);

            // Re-style the native inputbox to match ModFramework
            Image bg = obj.GetComponent<Image>();
            if (bg != null) bg.color = GameTheme.InputBackground;

            LayoutElement le = obj.GetComponent<LayoutElement>();
            if (le == null) le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = GameTheme.RowHeight;
            le.flexibleWidth = 1f;

            // Restyle main text
            if (input.textComponent != null)
            {
                input.textComponent.font = GameTheme.GameFont;
                input.textComponent.fontSize = GameTheme.DefaultFontSize;
                input.textComponent.color = GameTheme.LabelColor;
                input.textComponent.alignment = TextAnchor.MiddleLeft;
            }

            // Restyle placeholder
            Text placeholder = input.placeholder as Text;
            if (placeholder != null)
            {
                placeholder.font = GameTheme.GameFont;
                placeholder.fontSize = GameTheme.DefaultFontSize;
                placeholder.fontStyle = FontStyle.Italic;
                placeholder.color = new Color(GameTheme.LabelColor.r, GameTheme.LabelColor.g, GameTheme.LabelColor.b, 0.4f);
                placeholder.alignment = TextAnchor.MiddleLeft;
            }

            input.text = initial ?? "";
            input.caretColor = GameTheme.LabelColor;

            if (onChange != null)
                input.onValueChanged.AddListener(val => onChange(val));

            return input;
        }

        /// <summary>
        /// Returns true if any ModFramework InputField currently has keyboard focus.
        /// Use this to suppress game keybinds while the user is typing.
        /// </summary>
        public static bool IsAnyInputFieldFocused()
        {
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es == null || es.currentSelectedGameObject == null) return false;
            var input = es.currentSelectedGameObject.GetComponent<InputField>();
            return input != null && input.isFocused;
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
