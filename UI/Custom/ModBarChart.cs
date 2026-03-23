using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Horizontal bar chart drawn with Unity Image fills.
/// Each bar shows: label | filled bar (proportional to value) | value text.
/// </summary>
namespace ModFramework.UI.Custom
{
    public struct BarChartEntry
    {
        public string Label;
        public float Value;    // 0–1 normalized, or raw value (auto-normalized if > 1)
        public Color BarColor;

        public BarChartEntry(string label, float value, Color color)
        {
            Label = label;
            Value = value;
            BarColor = color;
        }
    }

    public class ModBarChart
    {
        public GameObject Root;
        private GameObject _barsContainer;
        private float _chartHeight;
        private List<BarChartEntry> _entries = new List<BarChartEntry>();

        /// <summary>
        /// Create a bar chart inside the given parent.
        /// </summary>
        public static ModBarChart Create(GameObject parent, float height = 200f)
        {
            var chart = new ModBarChart();
            chart._chartHeight = height;

            // Root container
            chart.Root = new GameObject("ModBarChart");
            RectTransform rootRect = chart.Root.AddComponent<RectTransform>();
            rootRect.SetParent(parent.transform, false);

            VerticalLayoutGroup rootVlg = chart.Root.AddComponent<VerticalLayoutGroup>();
            rootVlg.childControlWidth = true;
            rootVlg.childControlHeight = true;
            rootVlg.childForceExpandWidth = true;
            rootVlg.childForceExpandHeight = false;
            rootVlg.spacing = 2f;
            rootVlg.padding = new RectOffset(4, 4, 4, 4);

            LayoutElement rootLe = chart.Root.AddComponent<LayoutElement>();
            rootLe.preferredHeight = height;
            rootLe.flexibleWidth = 1f;

            // Subtle background
            Image bg = chart.Root.AddComponent<Image>();
            bg.color = new Color(
                GameTheme.PanelBackground.r * 0.95f,
                GameTheme.PanelBackground.g * 0.95f,
                GameTheme.PanelBackground.b * 0.95f,
                0.5f
            );

            // Container for bars
            chart._barsContainer = chart.Root;

            return chart;
        }

        /// <summary>
        /// Set chart data and rebuild bars.
        /// </summary>
        public void SetData(BarChartEntry[] entries)
        {
            _entries.Clear();
            _entries.AddRange(entries);
            Rebuild();
        }

        /// <summary>
        /// Set chart data from a list.
        /// </summary>
        public void SetData(List<BarChartEntry> entries)
        {
            _entries.Clear();
            _entries.AddRange(entries);
            Rebuild();
        }

        private void Rebuild()
        {
            if (_barsContainer == null) return;

            // Clear existing bars
            for (int i = _barsContainer.transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(_barsContainer.transform.GetChild(i).gameObject);
            }

            if (_entries.Count == 0) return;

            // Find max value for normalization
            float maxVal = 0f;
            foreach (var e in _entries)
                if (e.Value > maxVal) maxVal = e.Value;
            if (maxVal <= 0f) maxVal = 1f;

            // Calculate bar height based on available space
            float barHeight = Mathf.Max(18f, (_chartHeight - 8f - (_entries.Count - 1) * 2f) / _entries.Count);

            foreach (var entry in _entries)
            {
                // Row: [Label 80px] [Bar fill] [Value 60px]
                GameObject row = new GameObject("Bar_" + entry.Label);
                RectTransform rowRect = row.AddComponent<RectTransform>();
                rowRect.SetParent(_barsContainer.transform, false);

                HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
                hlg.childControlWidth = true;
                hlg.childControlHeight = true;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = true;
                hlg.spacing = 4f;
                hlg.padding = new RectOffset(2, 2, 0, 0);

                LayoutElement rowLe = row.AddComponent<LayoutElement>();
                rowLe.preferredHeight = barHeight;
                rowLe.flexibleWidth = 1f;

                // Label
                GameObject labelObj = new GameObject("Label");
                RectTransform labelRect = labelObj.AddComponent<RectTransform>();
                labelRect.SetParent(row.transform, false);
                Text labelText = labelObj.AddComponent<Text>();
                labelText.text = entry.Label;
                labelText.font = GameTheme.GameFont;
                labelText.fontSize = GameTheme.SmallFontSize;
                labelText.color = GameTheme.LabelColor;
                labelText.alignment = TextAnchor.MiddleLeft;
                LayoutElement labelLe = labelObj.AddComponent<LayoutElement>();
                labelLe.minWidth = 80f;
                labelLe.preferredWidth = 80f;

                // Bar background (track)
                GameObject barBg = new GameObject("BarTrack");
                RectTransform barBgRect = barBg.AddComponent<RectTransform>();
                barBgRect.SetParent(row.transform, false);
                Image barBgImg = barBg.AddComponent<Image>();
                barBgImg.color = new Color(0.7f, 0.7f, 0.7f, 0.3f);
                LayoutElement barBgLe = barBg.AddComponent<LayoutElement>();
                barBgLe.flexibleWidth = 1f;

                // Bar fill (inside track)
                GameObject barFill = new GameObject("BarFill");
                RectTransform fillRect = barFill.AddComponent<RectTransform>();
                fillRect.SetParent(barBg.transform, false);
                Image fillImg = barFill.AddComponent<Image>();
                fillImg.color = entry.BarColor;

                // Position fill: anchor left, stretch vertically, width = normalized value
                float normalized = entry.Value / maxVal;
                fillRect.anchorMin = new Vector2(0f, 0f);
                fillRect.anchorMax = new Vector2(normalized, 1f);
                fillRect.sizeDelta = Vector2.zero;
                fillRect.anchoredPosition = Vector2.zero;

                // Value text
                GameObject valObj = new GameObject("Value");
                RectTransform valRect = valObj.AddComponent<RectTransform>();
                valRect.SetParent(row.transform, false);
                Text valText = valObj.AddComponent<Text>();
                valText.text = entry.Value <= 1f ? (entry.Value * 100f).ToString("F0") + "%" : entry.Value.ToString("F0");
                valText.font = GameTheme.GameFont;
                valText.fontSize = GameTheme.SmallFontSize;
                valText.color = GameTheme.LabelColor;
                valText.alignment = TextAnchor.MiddleRight;
                LayoutElement valLe = valObj.AddComponent<LayoutElement>();
                valLe.minWidth = 50f;
                valLe.preferredWidth = 50f;
            }
        }
    }
}
