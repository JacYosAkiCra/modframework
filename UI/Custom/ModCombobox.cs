using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Themed dropdown (combobox) - built from scratch, no game prefab dependency.
/// Dropdown panel renders above the combobox using Canvas override for z-order.
/// </summary>
namespace ModFramework.UI.Custom
{
    public static class ModCombobox
    {
        /// <summary>
        /// Creates a themed combobox/dropdown.
        /// </summary>
        public static GameObject Create(string[] options, int selected, Action<int> onChange, GameObject parent)
        {
            GameTheme.Initialize();

            GameObject obj = new GameObject("ModCombobox");
            obj.transform.SetParent(parent.transform, false);

            Image bg = obj.AddComponent<Image>();
            bg.color = GameTheme.InputBackground;

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
            arrowRect.sizeDelta = new Vector2(20f, 0f);
            arrowRect.anchoredPosition = new Vector2(-10f, 0f);

            // Build dropdown panel (hidden initially)
            float itemH = GameTheme.RowHeight;
            float dropHeight = Mathf.Min(options.Length * itemH + 4f, 200f);

            GameObject dropdownPanel = new GameObject("DropdownPanel");
            RectTransform dropRect = dropdownPanel.AddComponent<RectTransform>();
            dropRect.SetParent(obj.transform, false);

            // Position BELOW the combobox
            dropRect.anchorMin = new Vector2(0f, 0f);
            dropRect.anchorMax = new Vector2(1f, 0f);
            dropRect.pivot = new Vector2(0.5f, 1f);
            dropRect.sizeDelta = new Vector2(0f, dropHeight);
            dropRect.anchoredPosition = Vector2.zero;

            Image dropBg = dropdownPanel.AddComponent<Image>();
            dropBg.color = GameTheme.WindowBackground;

            // Ensure dropdown renders on top of everything
            Canvas dropCanvas = dropdownPanel.AddComponent<Canvas>();
            dropCanvas.overrideSorting = true;
            dropCanvas.sortingOrder = 200;
            dropdownPanel.AddComponent<GraphicRaycaster>();

            // Options list
            VerticalLayoutGroup dropLayout = dropdownPanel.AddComponent<VerticalLayoutGroup>();
            dropLayout.childAlignment = TextAnchor.UpperLeft;
            dropLayout.childControlWidth = true;
            dropLayout.childControlHeight = true;
            dropLayout.childForceExpandWidth = true;
            dropLayout.childForceExpandHeight = false;
            dropLayout.spacing = 1f;
            dropLayout.padding = new RectOffset(2, 2, 2, 2);

            for (int i = 0; i < options.Length; i++)
            {
                int index = i;
                GameObject optObj = new GameObject("Option_" + i);
                optObj.transform.SetParent(dropdownPanel.transform, false);

                Image optBg = optObj.AddComponent<Image>();
                optBg.color = (i == selected) ? GameTheme.ButtonHover : Color.clear;

                LayoutElement optLE = optObj.AddComponent<LayoutElement>();
                optLE.preferredHeight = itemH;

                Button optBtn = optObj.AddComponent<Button>();
                ColorBlock cb = optBtn.colors;
                cb.normalColor = (i == selected) ? GameTheme.ButtonHover : Color.clear;
                cb.highlightedColor = GameTheme.ButtonHover;
                cb.pressedColor = GameTheme.ButtonPressed;
                optBtn.colors = cb;
                optBtn.targetGraphic = optBg;

                optBtn.onClick.AddListener(() =>
                {
                    label.text = options[index];
                    dropdownPanel.SetActive(false);
                    if (onChange != null) onChange(index);
                });

                // Option text
                GameObject optLabelObj = new GameObject("Text");
                optLabelObj.transform.SetParent(optObj.transform, false);
                Text optLabel = optLabelObj.AddComponent<Text>();
                optLabel.text = options[i];
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

            dropdownPanel.SetActive(false);

            // Main click handler to toggle dropdown
            Button mainBtn = obj.AddComponent<Button>();
            mainBtn.targetGraphic = bg;
            ColorBlock mainCb = mainBtn.colors;
            mainCb.normalColor = GameTheme.InputBackground;
            mainCb.highlightedColor = GameTheme.ButtonHover;
            mainCb.pressedColor = GameTheme.ButtonPressed;
            mainBtn.colors = mainCb;
            mainBtn.onClick.AddListener(() =>
            {
                dropdownPanel.SetActive(!dropdownPanel.activeSelf);
            });

            return obj;
        }
    }
}
