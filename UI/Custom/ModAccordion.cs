using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Collapsible sections with click-to-expand headers.
/// Each section has a header row with ▶/▼ indicator and a content panel
/// that toggles visibility on click.
/// </summary>
namespace ModFramework.UI.Custom
{
    public class AccordionSection
    {
        public GameObject Header;
        public GameObject ContentPanel;
        public Text ArrowText;
        public bool IsExpanded;
    }

    public class ModAccordion
    {
        public GameObject Root;
        private List<AccordionSection> _sections = new List<AccordionSection>();
        private bool _singleExpandMode;

        /// <summary>
        /// Create an accordion inside the given parent.
        /// </summary>
        /// <param name="singleExpand">If true, opening one section closes all others.</param>
        public static ModAccordion Create(GameObject parent, bool singleExpand = false)
        {
            var accordion = new ModAccordion();
            accordion._singleExpandMode = singleExpand;

            accordion.Root = new GameObject("ModAccordion");
            RectTransform rootRect = accordion.Root.AddComponent<RectTransform>();
            rootRect.SetParent(parent.transform, false);

            VerticalLayoutGroup vlg = accordion.Root.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 2f;

            LayoutElement le = accordion.Root.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;

            ContentSizeFitter csf = accordion.Root.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return accordion;
        }

        /// <summary>
        /// Add a collapsible section. Returns the content panel to add widgets to.
        /// Section starts collapsed by default.
        /// </summary>
        public GameObject AddSection(string title, bool startExpanded = false)
        {
            var section = new AccordionSection();

            // Section container
            GameObject container = new GameObject("Section_" + title);
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.SetParent(Root.transform, false);

            VerticalLayoutGroup containerVlg = container.AddComponent<VerticalLayoutGroup>();
            containerVlg.childControlWidth = true;
            containerVlg.childControlHeight = true;
            containerVlg.childForceExpandWidth = true;
            containerVlg.childForceExpandHeight = false;
            containerVlg.spacing = 0f;

            LayoutElement containerLe = container.AddComponent<LayoutElement>();
            containerLe.flexibleWidth = 1f;

            ContentSizeFitter containerCsf = container.AddComponent<ContentSizeFitter>();
            containerCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Header row (clickable)
            section.Header = new GameObject("Header");
            RectTransform headerRect = section.Header.AddComponent<RectTransform>();
            headerRect.SetParent(container.transform, false);

            Image headerBg = section.Header.AddComponent<Image>();
            headerBg.color = GameTheme.TitleBarColor;

            HorizontalLayoutGroup headerHlg = section.Header.AddComponent<HorizontalLayoutGroup>();
            headerHlg.childControlWidth = true;
            headerHlg.childControlHeight = true;
            headerHlg.childForceExpandWidth = false;
            headerHlg.childForceExpandHeight = true;
            headerHlg.spacing = 6f;
            headerHlg.padding = new RectOffset(8, 8, 4, 4);

            LayoutElement headerLe = section.Header.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 28f;
            headerLe.flexibleWidth = 1f;

            // Arrow indicator
            GameObject arrowObj = new GameObject("Arrow");
            RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.SetParent(section.Header.transform, false);
            section.ArrowText = arrowObj.AddComponent<Text>();
            section.ArrowText.font = GameTheme.GameFont;
            section.ArrowText.fontSize = GameTheme.DefaultFontSize;
            section.ArrowText.color = GameTheme.HeaderColor;
            section.ArrowText.alignment = TextAnchor.MiddleCenter;
            LayoutElement arrowLe = arrowObj.AddComponent<LayoutElement>();
            arrowLe.minWidth = 16f;
            arrowLe.preferredWidth = 16f;

            // Title text
            GameObject titleObj = new GameObject("Title");
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.SetParent(section.Header.transform, false);
            Text titleText = titleObj.AddComponent<Text>();
            titleText.text = title;
            titleText.font = GameTheme.GameFont;
            titleText.fontSize = GameTheme.DefaultFontSize;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = GameTheme.HeaderColor;
            titleText.alignment = TextAnchor.MiddleLeft;
            LayoutElement titleLe = titleObj.AddComponent<LayoutElement>();
            titleLe.flexibleWidth = 1f;

            // Content panel
            section.ContentPanel = new GameObject("Content");
            RectTransform contentRect = section.ContentPanel.AddComponent<RectTransform>();
            contentRect.SetParent(container.transform, false);

            VerticalLayoutGroup contentVlg = section.ContentPanel.AddComponent<VerticalLayoutGroup>();
            contentVlg.childControlWidth = true;
            contentVlg.childControlHeight = true;
            contentVlg.childForceExpandWidth = true;
            contentVlg.childForceExpandHeight = false;
            contentVlg.spacing = 4f;
            contentVlg.padding = new RectOffset(12, 4, 4, 4);

            LayoutElement contentLe = section.ContentPanel.AddComponent<LayoutElement>();
            contentLe.flexibleWidth = 1f;

            ContentSizeFitter contentCsf = section.ContentPanel.AddComponent<ContentSizeFitter>();
            contentCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Click handler on header
            Button headerBtn = section.Header.AddComponent<Button>();
            var sectionRef = section;
            headerBtn.onClick.AddListener(() => ToggleSection(sectionRef));

            // Hover effect
            HoverHandler hover = section.Header.AddComponent<HoverHandler>();
            hover.Setup(headerBg);

            // Set initial state
            section.IsExpanded = startExpanded;
            section.ContentPanel.SetActive(startExpanded);
            section.ArrowText.text = startExpanded ? "▼" : "▶";

            _sections.Add(section);
            return section.ContentPanel;
        }

        private void ToggleSection(AccordionSection section)
        {
            if (_singleExpandMode && !section.IsExpanded)
            {
                // Collapse all others
                foreach (var s in _sections)
                {
                    if (s != section && s.IsExpanded)
                    {
                        s.IsExpanded = false;
                        s.ContentPanel.SetActive(false);
                        s.ArrowText.text = "▶";
                    }
                }
            }

            section.IsExpanded = !section.IsExpanded;
            section.ContentPanel.SetActive(section.IsExpanded);
            section.ArrowText.text = section.IsExpanded ? "▼" : "▶";
        }
    }
}
