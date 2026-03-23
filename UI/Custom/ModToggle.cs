using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Themed toggle (checkbox) and slider widgets.
/// </summary>
namespace ModFramework.UI.Custom
{
    public static class ModToggle
    {
        /// <summary>
        /// Creates a themed toggle (checkbox with label).
        /// </summary>
        public static Toggle Create(string label, bool isOn, Action<bool> onChange, GameObject parent)
        {
            GameTheme.Initialize();

            GameObject obj = new GameObject("ModToggle");
            obj.transform.SetParent(parent.transform, false);

            // Horizontal layout: checkbox + label
            HorizontalLayoutGroup layout = obj.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 6f;

            LayoutElement rootLE = obj.AddComponent<LayoutElement>();
            rootLE.preferredHeight = GameTheme.RowHeight;
            rootLE.flexibleWidth = 1f;

            // Checkbox background
            GameObject checkBg = new GameObject("Background");
            checkBg.transform.SetParent(obj.transform, false);
            Image checkBgImg = checkBg.AddComponent<Image>();
            checkBgImg.color = GameTheme.InputBackground;
            LayoutElement checkLE = checkBg.AddComponent<LayoutElement>();
            checkLE.preferredWidth = 18f;
            checkLE.preferredHeight = 18f;

            // Checkmark
            GameObject checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(checkBg.transform, false);
            Text checkText = checkmark.AddComponent<Text>();
            checkText.text = "X";
            checkText.font = GameTheme.GameFont;
            checkText.fontSize = GameTheme.DefaultFontSize;
            checkText.color = GameTheme.HeaderColor;
            checkText.alignment = TextAnchor.MiddleCenter;

            RectTransform cmRect = checkmark.GetComponent<RectTransform>();
            cmRect.anchorMin = Vector2.zero;
            cmRect.anchorMax = Vector2.one;
            cmRect.sizeDelta = Vector2.zero;

            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(obj.transform, false);
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = GameTheme.GameFont;
            labelText.fontSize = GameTheme.DefaultFontSize;
            labelText.color = GameTheme.LabelColor;
            labelText.alignment = TextAnchor.MiddleLeft;
            LayoutElement labelLE = labelObj.AddComponent<LayoutElement>();
            labelLE.flexibleWidth = 1f;

            // Toggle component
            Toggle toggle = obj.AddComponent<Toggle>();
            toggle.isOn = isOn;
            toggle.graphic = checkText;
            toggle.targetGraphic = checkBgImg;

            if (onChange != null)
                toggle.onValueChanged.AddListener(val => onChange(val));

            return toggle;
        }
    }

    public static class ModSlider
    {
        /// <summary>
        /// Creates a themed slider with proper fill and handle.
        /// </summary>
        public static Slider Create(float min, float max, float value, Action<float> onChange, GameObject parent)
        {
            GameTheme.Initialize();

            GameObject obj = new GameObject("ModSlider");
            obj.transform.SetParent(parent.transform, false);
            obj.AddComponent<RectTransform>();

            LayoutElement rootLE = obj.AddComponent<LayoutElement>();
            rootLE.preferredHeight = GameTheme.RowHeight;
            rootLE.flexibleWidth = 1f;

            // Background track
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(obj.transform, false);
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0.35f);
            bgRect.anchorMax = new Vector2(1f, 0.65f);
            bgRect.sizeDelta = Vector2.zero;

            // Fill area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(obj.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.35f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.65f);
            fillAreaRect.sizeDelta = new Vector2(-10f, 0f);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = GameTheme.ButtonHover;
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.sizeDelta = Vector2.zero;

            // Handle area
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(obj.transform, false);
            RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.sizeDelta = new Vector2(-10f, 0f);

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(12f, 0f);

            // Slider component
            Slider slider = obj.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImg;

            if (onChange != null)
                slider.onValueChanged.AddListener(val => onChange(val));

            return slider;
        }
    }
}
