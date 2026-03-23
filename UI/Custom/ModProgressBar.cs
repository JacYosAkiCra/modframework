using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Themed progress bar and tab panel widgets.
/// </summary>
namespace ModFramework.UI.Custom
{
    public static class ModProgressBar
    {
        /// <summary>
        /// Creates a themed horizontal progress bar (0.0 to 1.0).
        /// </summary>
        public static GameObject Create(float value01, GameObject parent)
        {
            GameTheme.Initialize();

            GameObject obj = new GameObject("ModProgressBar");
            obj.transform.SetParent(parent.transform, false);

            // Track background
            Image trackBg = obj.AddComponent<Image>();
            trackBg.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = 16f;
            le.flexibleWidth = 1f;

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(obj.transform, false);
            Image fillImg = fillObj.AddComponent<Image>();
            fillImg.color = GameTheme.ButtonHover;

            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(Mathf.Clamp01(value01), 1f);
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;

            return obj;
        }

        /// <summary>
        /// Creates a live-updating progress bar. The valueProvider function is called
        /// every refresh tick and the fill is updated in-place (no destroy/recreate).
        /// Requires a ModRefreshDriver somewhere up the hierarchy (ModWindow adds one automatically).
        /// </summary>
        public static GameObject CreateLive(Func<float> valueProvider, GameObject parent)
        {
            float initial = valueProvider != null ? valueProvider() : 0f;
            GameObject bar = Create(initial, parent);

            ModRefreshDriver driver = parent.GetComponentInParent<ModRefreshDriver>();
            if (driver != null)
            {
                driver.Register(() =>
                {
                    if (bar != null && valueProvider != null)
                    {
                        SetValue(bar, valueProvider());
                    }
                });
            }

            return bar;
        }

        /// <summary>
        /// Updates the fill amount of a progress bar (0.0 to 1.0).
        /// </summary>
        public static void SetValue(GameObject bar, float value01)
        {
            if (bar == null) return;
            Transform fill = bar.transform.Find("Fill");
            if (fill == null) return;

            RectTransform fillRect = fill.GetComponent<RectTransform>();
            if (fillRect != null)
            {
                fillRect.anchorMax = new Vector2(Mathf.Clamp01(value01), 1f);
            }
        }
    }

    public static class ModTabPanel
    {
        /// <summary>
        /// Creates a themed tab panel.
        /// Returns the root GameObject. Use GetTabContent() to get individual tab pages.
        /// </summary>
        public static GameObject Create(string[] tabNames, GameObject parent)
        {
            GameTheme.Initialize();

            GameObject obj = new GameObject("ModTabPanel");
            obj.transform.SetParent(parent.transform, false);

            VerticalLayoutGroup rootLayout = obj.AddComponent<VerticalLayoutGroup>();
            rootLayout.childAlignment = TextAnchor.UpperLeft;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            rootLayout.spacing = 0f;

            LayoutElement rootLE = obj.AddComponent<LayoutElement>();
            rootLE.flexibleWidth = 1f;
            rootLE.flexibleHeight = 1f;

            // Tab button bar
            GameObject tabBar = new GameObject("TabBar");
            tabBar.transform.SetParent(obj.transform, false);
            Image tabBarBg = tabBar.AddComponent<Image>();
            tabBarBg.color = GameTheme.PanelBackground;

            LayoutElement tabBarLE = tabBar.AddComponent<LayoutElement>();
            tabBarLE.preferredHeight = GameTheme.ButtonHeight + 4f;
            tabBarLE.flexibleWidth = 1f;

            HorizontalLayoutGroup tabLayout = tabBar.AddComponent<HorizontalLayoutGroup>();
            tabLayout.childAlignment = TextAnchor.MiddleLeft;
            tabLayout.childControlWidth = false;
            tabLayout.childControlHeight = true;
            tabLayout.childForceExpandWidth = false;
            tabLayout.childForceExpandHeight = false;
            tabLayout.spacing = 2f;
            tabLayout.padding = new RectOffset(4, 4, 2, 2);

            // Tab content pages container
            GameObject pagesContainer = new GameObject("Pages");
            pagesContainer.transform.SetParent(obj.transform, false);

            LayoutElement pagesLE = pagesContainer.AddComponent<LayoutElement>();
            pagesLE.flexibleWidth = 1f;
            pagesLE.flexibleHeight = 1f;

            // Create tab pages
            List<GameObject> pages = new List<GameObject>();
            List<Image> tabBgs = new List<Image>();

            for (int i = 0; i < tabNames.Length; i++)
            {
                int tabIndex = i;

                // Tab button
                GameObject tabBtnObj = new GameObject("Tab_" + i);
                tabBtnObj.transform.SetParent(tabBar.transform, false);

                Image tabBtnBg = tabBtnObj.AddComponent<Image>();
                tabBtnBg.color = (i == 0) ? GameTheme.WindowBackground : GameTheme.PanelBackground;
                tabBgs.Add(tabBtnBg);

                LayoutElement tabBtnLE = tabBtnObj.AddComponent<LayoutElement>();
                tabBtnLE.preferredWidth = 100f;

                Button tabBtn = tabBtnObj.AddComponent<Button>();

                // Tab label
                GameObject tabLabelObj = new GameObject("Label");
                tabLabelObj.transform.SetParent(tabBtnObj.transform, false);
                Text tabLabel = tabLabelObj.AddComponent<Text>();
                tabLabel.text = tabNames[i];
                tabLabel.font = GameTheme.GameFont;
                tabLabel.fontSize = GameTheme.DefaultFontSize;
                tabLabel.color = GameTheme.LabelColor;
                tabLabel.alignment = TextAnchor.MiddleCenter;

                RectTransform tabLabelRect = tabLabelObj.GetComponent<RectTransform>();
                tabLabelRect.anchorMin = Vector2.zero;
                tabLabelRect.anchorMax = Vector2.one;
                tabLabelRect.sizeDelta = Vector2.zero;

                // Tab page (content area)
                GameObject page = new GameObject("Page_" + i);
                page.transform.SetParent(pagesContainer.transform, false);

                RectTransform pageRect = page.AddComponent<RectTransform>();
                pageRect.anchorMin = Vector2.zero;
                pageRect.anchorMax = Vector2.one;
                pageRect.sizeDelta = Vector2.zero;
                pageRect.anchoredPosition = Vector2.zero;

                // Add layout group to page so widgets stack automatically
                VerticalLayoutGroup pageLayout = page.AddComponent<VerticalLayoutGroup>();
                pageLayout.childAlignment = TextAnchor.UpperLeft;
                pageLayout.childControlWidth = true;
                pageLayout.childControlHeight = true;
                pageLayout.childForceExpandWidth = true;
                pageLayout.childForceExpandHeight = false;
                pageLayout.spacing = GameTheme.SectionSpacing;
                pageLayout.padding = new RectOffset(
                    (int)GameTheme.WindowPadding,
                    (int)GameTheme.WindowPadding,
                    (int)GameTheme.WindowPadding,
                    (int)GameTheme.WindowPadding
                );

                page.SetActive(i == 0); // Only first tab visible
                pages.Add(page);

                // Tab click handler
                tabBtn.onClick.AddListener(() =>
                {
                    for (int j = 0; j < pages.Count; j++)
                    {
                        pages[j].SetActive(j == tabIndex);
                        tabBgs[j].color = (j == tabIndex) ? GameTheme.WindowBackground : GameTheme.PanelBackground;
                    }
                });
            }

            return obj;
        }

        /// <summary>
        /// Gets the content panel for a specific tab by index.
        /// Add widgets as children of the returned GameObject.
        /// </summary>
        public static GameObject GetTabContent(GameObject tabPanel, int index)
        {
            if (tabPanel == null) return null;
            Transform pages = tabPanel.transform.Find("Pages");
            if (pages == null) return null;

            string pageName = "Page_" + index;
            Transform page = pages.Find(pageName);
            return page != null ? page.gameObject : null;
        }
    }
}
