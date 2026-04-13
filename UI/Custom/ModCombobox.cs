using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Themed dropdown (combobox) that renders its options list by parenting directly
/// to the game's root Canvas transform. This completely bypasses all parent 
/// LayoutGroup constraints and ModWindow clipping, guaranteeing the dropdown
/// always renders on top of everything.
/// </summary>
namespace ModFramework.UI.Custom
{
    public static class ModCombobox
    {
        // Track active expansion so only one combobox is expanded at a time
        private static GameObject _expandedOptions;
        private static GameObject _expandedCombobox;

        /// <summary>True when any combobox has its options expanded.</summary>
        public static bool IsExpanded { get { return _expandedOptions != null; } }

        /// <summary>
        /// Creates a themed combobox/dropdown.
        /// </summary>
        public static GameObject Create(string[] options, int selected, Action<int> onChange, GameObject parent)
        {
            GameTheme.Initialize();

            GameObject obj = new GameObject("ModCombobox");
            obj.transform.SetParent(parent.transform, false);

            Image bg = obj.AddComponent<Image>();
            bg.color = Color.white;

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = GameTheme.RowHeight;
            le.flexibleWidth = 1f;

            // Current selection label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(obj.transform, false);
            Text label = labelObj.AddComponent<Text>();
            label.font = GameTheme.GameFont;
            label.fontSize = GameTheme.DefaultFontSize;
            label.color = GameTheme.LabelColor;
            label.alignment = TextAnchor.MiddleLeft;
            label.text = (selected >= 0 && selected < options.Length) ? options[selected] : "";

            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(8f, 0f);
            labelRect.offsetMax = new Vector2(-24f, 0f);

            // Arrow indicator
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(obj.transform, false);
            Text arrow = arrowObj.AddComponent<Text>();
            arrow.text = "v";
            arrow.font = GameTheme.GameFont;
            arrow.fontSize = GameTheme.SmallFontSize;
            arrow.color = GameTheme.LabelColor;
            arrow.alignment = TextAnchor.MiddleCenter;

            RectTransform arrowRect = arrowObj.GetComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1f, 0f);
            arrowRect.anchorMax = new Vector2(1f, 1f);
            arrowRect.pivot = new Vector2(1f, 0.5f);
            arrowRect.sizeDelta = new Vector2(30f, 0f);
            arrowRect.anchoredPosition = new Vector2(-5f, 0f);

            // Main click handler
            Button mainBtn = obj.AddComponent<Button>();
            mainBtn.targetGraphic = bg;
            ColorBlock mainCb = mainBtn.colors;
            mainCb.normalColor = GameTheme.InputBackground;
            mainCb.highlightedColor = GameTheme.ButtonHover;
            mainCb.pressedColor = GameTheme.ButtonPressed;
            mainCb.colorMultiplier = 1f;
            mainBtn.colors = mainCb;

            // Capture references
            Text labelRef = label;
            Text arrowRef = arrow;
            string[] optionsRef = options;
            GameObject comboObj = obj;

            mainBtn.onClick.AddListener(() =>
            {
                // Toggle if already expanded
                if (_expandedCombobox == comboObj && _expandedOptions != null)
                {
                    CollapseOptions();
                    return;
                }

                CollapseOptions();

                // Get the combobox world corners to position the dropdown
                RectTransform comboRect = comboObj.GetComponent<RectTransform>();
                Vector3[] corners = new Vector3[4];
                comboRect.GetWorldCorners(corners);
                // corners: 0=bottom-left, 1=top-left, 2=top-right, 3=bottom-right

                float comboWorldWidth = corners[3].x - corners[0].x;

                // Parent to the game's Canvas root -- completely escapes all LayoutGroups
                Canvas[] canvases = comboObj.GetComponentsInParent<Canvas>();
                Transform canvasRoot = comboRect;
                if (canvases.Length > 0)
                {
                    // Get the topmost canvas (last in the array if searching upward, or just iterate)
                    canvasRoot = canvases[canvases.Length - 1].transform;
                }
                else
                {
                    while (canvasRoot.parent != null)
                    {
                         canvasRoot = canvasRoot.parent;
                    }
                }

                GameObject optionsContainer = new GameObject("ModCombobox_Options");
                optionsContainer.transform.SetParent(canvasRoot, true); // worldPositionStays=true
                optionsContainer.transform.SetAsLastSibling();

                RectTransform optRect = optionsContainer.AddComponent<RectTransform>();
                optRect.pivot = new Vector2(0f, 1f); // Top-left pivot, dropdown grows downward

                // Position directly at combobox bottom-left using world coordinates
                optRect.position = corners[0];
                optRect.sizeDelta = new Vector2(comboWorldWidth, 0f);

                // Background
                Image containerBg = optionsContainer.AddComponent<Image>();
                containerBg.color = new Color(
                    Mathf.Max(GameTheme.InputBackground.r - 0.05f, 0f),
                    Mathf.Max(GameTheme.InputBackground.g - 0.05f, 0f),
                    Mathf.Max(GameTheme.InputBackground.b - 0.05f, 0f),
                    1f);

                // Vertical layout
                VerticalLayoutGroup optLayout = optionsContainer.AddComponent<VerticalLayoutGroup>();
                optLayout.childAlignment = TextAnchor.UpperLeft;
                optLayout.childControlWidth = true;
                optLayout.childControlHeight = true;
                optLayout.childForceExpandWidth = true;
                optLayout.childForceExpandHeight = false;
                optLayout.spacing = 0f;
                optLayout.padding = new RectOffset(4, 4, 2, 2);

                // Auto-size height
                ContentSizeFitter csf = optionsContainer.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                float itemH = GameTheme.RowHeight;

                // Build option buttons
                for (int i = 0; i < optionsRef.Length; i++)
                {
                    int index = i;
                    GameObject optObj = new GameObject("Option_" + i);
                    optObj.transform.SetParent(optionsContainer.transform, false);

                    Image optBg = optObj.AddComponent<Image>();
                    bool isSelected = (optionsRef[index] == labelRef.text);
                    optBg.color = Color.white;

                    LayoutElement optLE = optObj.AddComponent<LayoutElement>();
                    optLE.preferredHeight = itemH;

                    Button optBtn = optObj.AddComponent<Button>();
                    ColorBlock cb = optBtn.colors;
                    cb.normalColor = isSelected ? GameTheme.ButtonHover : Color.clear;
                    cb.highlightedColor = GameTheme.ButtonHover;
                    cb.pressedColor = GameTheme.ButtonPressed;
                    cb.colorMultiplier = 1f;
                    optBtn.colors = cb;
                    optBtn.targetGraphic = optBg;

                    optBtn.onClick.AddListener(() =>
                    {
                        labelRef.text = optionsRef[index];
                        CollapseOptions();
                        if (onChange != null) onChange(index);
                    });

                    // Option text
                    GameObject optLabelObj = new GameObject("Text");
                    optLabelObj.transform.SetParent(optObj.transform, false);
                    Text optLabel = optLabelObj.AddComponent<Text>();
                    optLabel.text = optionsRef[i];
                    optLabel.font = GameTheme.GameFont;
                    optLabel.fontSize = GameTheme.DefaultFontSize;
                    optLabel.color = GameTheme.LabelColor;
                    optLabel.alignment = TextAnchor.MiddleLeft;

                    RectTransform optLabelRect = optLabelObj.GetComponent<RectTransform>();
                    optLabelRect.anchorMin = Vector2.zero;
                    optLabelRect.anchorMax = Vector2.one;
                    optLabelRect.offsetMin = new Vector2(8f, 0f);
                    optLabelRect.offsetMax = new Vector2(-4f, 0f);
                }

                arrowRef.text = "^";
                _expandedOptions = optionsContainer;
                _expandedCombobox = comboObj;
            });

            return obj;
        }

        /// <summary>
        /// Collapse any currently expanded combobox options.
        /// </summary>
        public static void CollapseOptions()
        {
            if (_expandedOptions != null)
            {
                UnityEngine.Object.Destroy(_expandedOptions);
                _expandedOptions = null;
            }

            if (_expandedCombobox != null)
            {
                Transform arrowTransform = _expandedCombobox.transform.Find("Arrow");
                if (arrowTransform != null)
                {
                    Text arrowText = arrowTransform.GetComponent<Text>();
                    if (arrowText != null) arrowText.text = "v";
                }
                _expandedCombobox = null;
            }
        }

        // Keep backward compatibility alias
        public static void CloseActiveOverlay()
        {
            CollapseOptions();
        }
    }
}
