using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Themed text labels and section headers.
/// </summary>
namespace ModFramework.UI.Custom
{
    public static class ModLabel
    {
        /// <summary>
        /// Creates a themed text label.
        /// </summary>
        public static Text Create(string text, GameObject parent, bool bold = false)
        {
            GameTheme.Initialize();

            GameObject obj = new GameObject("ModLabel");
            obj.transform.SetParent(parent.transform, false);

            Text label = obj.AddComponent<Text>();
            label.text = text;
            label.font = GameTheme.GameFont;
            label.fontSize = GameTheme.DefaultFontSize;
            label.color = GameTheme.LabelColor;
            label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            label.alignment = TextAnchor.MiddleLeft;

            // Auto-height based on content
            ContentSizeFitter fitter = obj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;

            return label;
        }

        /// <summary>
        /// Creates a live-updating label. The valueProvider function is called
        /// every refresh tick and the label text is updated in-place (no destroy/recreate).
        /// Requires a ModRefreshDriver somewhere up the hierarchy (ModWindow adds one automatically).
        /// </summary>
        public static Text CreateLive(Func<string> valueProvider, GameObject parent, bool bold = false)
        {
            Text label = Create(valueProvider != null ? valueProvider() : "", parent, bold);

            // Find the nearest refresh driver up the hierarchy
            ModRefreshDriver driver = parent.GetComponentInParent<ModRefreshDriver>();
            if (driver != null)
            {
                driver.Register(() =>
                {
                    if (label != null && valueProvider != null)
                    {
                        label.text = valueProvider();
                    }
                });
            }

            return label;
        }
    }

    public static class ModHeader
    {
        /// <summary>
        /// Creates a bold section header with themed color.
        /// </summary>
        public static Text Create(string text, GameObject parent)
        {
            GameTheme.Initialize();

            GameObject obj = new GameObject("ModHeader");
            obj.transform.SetParent(parent.transform, false);

            Text header = obj.AddComponent<Text>();
            header.text = text;
            header.font = GameTheme.GameFont;
            header.fontSize = GameTheme.HeaderFontSize;
            header.fontStyle = FontStyle.Bold;
            header.color = GameTheme.HeaderColor;
            header.alignment = TextAnchor.MiddleLeft;

            ContentSizeFitter fitter = obj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1f;

            return header;
        }
    }
}
