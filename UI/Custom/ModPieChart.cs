using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pie chart matching game's built-in style: circular slices with labels,
/// dark outline ring, and color-swatch legend.
/// Uses stacked Image components with fillAmount for slices.
/// </summary>
namespace ModFramework.UI.Custom
{
    public struct PieSlice
    {
        public string Label;
        public float Value;
        public Color SliceColor;

        public PieSlice(string label, float value, Color color)
        {
            Label = label;
            Value = value;
            SliceColor = color;
        }
    }

    public class ModPieChart
    {
        public GameObject Root;
        private GameObject _pieContainer;
        private GameObject _legendContainer;
        private float _chartSize;
        private List<PieSlice> _slices = new List<PieSlice>();

        /// <summary>
        /// Creates a white filled circle sprite at runtime.
        /// </summary>
        private static Sprite _circleSprite;
        private static Sprite GetCircleSprite()
        {
            if (_circleSprite != null) return _circleSprite;

            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return _circleSprite;
        }

        /// <summary>
        /// Creates a ring/donut sprite for the outline.
        /// </summary>
        private static Sprite _ringSprite;
        private static Sprite GetRingSprite()
        {
            if (_ringSprite != null) return _ringSprite;

            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float outerRadius = center - 1f;
            float innerRadius = outerRadius - 3f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float outerAlpha = Mathf.Clamp01(outerRadius - dist + 0.5f);
                    float innerAlpha = Mathf.Clamp01(dist - innerRadius + 0.5f);
                    float alpha = outerAlpha * innerAlpha;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            _ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return _ringSprite;
        }

        /// <summary>
        /// Create a pie chart inside the given parent.
        /// </summary>
        public static ModPieChart Create(GameObject parent, float size = 180f)
        {
            var chart = new ModPieChart();
            chart._chartSize = size;

            // Root: horizontal layout [Pie | Legend]
            chart.Root = new GameObject("ModPieChart");
            RectTransform rootRect = chart.Root.AddComponent<RectTransform>();
            rootRect.SetParent(parent.transform, false);

            HorizontalLayoutGroup hlg = chart.Root.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 12f;
            hlg.padding = new RectOffset(4, 4, 4, 4);
            hlg.childAlignment = TextAnchor.MiddleLeft;

            LayoutElement rootLe = chart.Root.AddComponent<LayoutElement>();
            rootLe.preferredHeight = size + 8f;
            rootLe.flexibleWidth = 1f;

            // Pie container (fixed square)
            chart._pieContainer = new GameObject("PieArea");
            RectTransform pieRect = chart._pieContainer.AddComponent<RectTransform>();
            pieRect.SetParent(chart.Root.transform, false);
            LayoutElement pieLe = chart._pieContainer.AddComponent<LayoutElement>();
            pieLe.minWidth = size;
            pieLe.preferredWidth = size;
            pieLe.minHeight = size;
            pieLe.preferredHeight = size;

            // Legend container (vertical list)
            chart._legendContainer = new GameObject("Legend");
            RectTransform legendRect = chart._legendContainer.AddComponent<RectTransform>();
            legendRect.SetParent(chart.Root.transform, false);

            VerticalLayoutGroup legendVlg = chart._legendContainer.AddComponent<VerticalLayoutGroup>();
            legendVlg.childControlWidth = true;
            legendVlg.childControlHeight = true;
            legendVlg.childForceExpandWidth = false;
            legendVlg.childForceExpandHeight = false;
            legendVlg.spacing = 4f;
            legendVlg.childAlignment = TextAnchor.MiddleLeft;

            LayoutElement legendLe = chart._legendContainer.AddComponent<LayoutElement>();
            legendLe.flexibleWidth = 1f;

            return chart;
        }

        public void SetData(PieSlice[] slices)
        {
            _slices.Clear();
            _slices.AddRange(slices);
            Rebuild();
        }

        public void SetData(List<PieSlice> slices)
        {
            _slices.Clear();
            _slices.AddRange(slices);
            Rebuild();
        }

        private void Rebuild()
        {
            // Clear pie
            for (int i = _pieContainer.transform.childCount - 1; i >= 0; i--)
                Object.Destroy(_pieContainer.transform.GetChild(i).gameObject);

            // Clear legend
            for (int i = _legendContainer.transform.childCount - 1; i >= 0; i--)
                Object.Destroy(_legendContainer.transform.GetChild(i).gameObject);

            if (_slices.Count == 0) return;

            float total = 0f;
            foreach (var s in _slices) total += s.Value;
            if (total <= 0f) return;

            Sprite circle = GetCircleSprite();
            Sprite ring = GetRingSprite();

            // Build slices (stacked from back to front, largest fill first)
            float cumulativeFill = 1f;
            for (int i = _slices.Count - 1; i >= 0; i--)
            {
                var slice = _slices[i];
                float proportion = slice.Value / total;

                GameObject sliceObj = new GameObject("Slice_" + slice.Label);
                RectTransform sliceRect = sliceObj.AddComponent<RectTransform>();
                sliceRect.SetParent(_pieContainer.transform, false);
                sliceRect.anchorMin = Vector2.zero;
                sliceRect.anchorMax = Vector2.one;
                sliceRect.sizeDelta = Vector2.zero;
                sliceRect.anchoredPosition = Vector2.zero;

                Image sliceImg = sliceObj.AddComponent<Image>();
                sliceImg.sprite = circle;
                sliceImg.color = slice.SliceColor;
                sliceImg.type = Image.Type.Filled;
                sliceImg.fillMethod = Image.FillMethod.Radial360;
                sliceImg.fillOrigin = (int)Image.Origin360.Top;
                sliceImg.fillClockwise = true;
                sliceImg.fillAmount = cumulativeFill;
                sliceImg.raycastTarget = false;

                cumulativeFill -= proportion;
            }

            // Dark outline ring on top
            GameObject outlineObj = new GameObject("Outline");
            RectTransform outlineRect = outlineObj.AddComponent<RectTransform>();
            outlineRect.SetParent(_pieContainer.transform, false);
            outlineRect.anchorMin = Vector2.zero;
            outlineRect.anchorMax = Vector2.one;
            outlineRect.sizeDelta = Vector2.zero;
            outlineRect.anchoredPosition = Vector2.zero;
            Image outlineImg = outlineObj.AddComponent<Image>();
            outlineImg.sprite = ring;
            outlineImg.color = new Color(0.15f, 0.15f, 0.15f, 0.6f);
            outlineImg.raycastTarget = false;

            // Add labels on slices (positioned using angle midpoints)
            float currentAngle = 0f;
            for (int i = 0; i < _slices.Count; i++)
            {
                var slice = _slices[i];
                float proportion = slice.Value / total;
                float sliceAngle = proportion * 360f;
                float midAngle = currentAngle + sliceAngle / 2f;

                // Only show label if slice is big enough (> 8%)
                if (proportion >= 0.05f)
                {
                    // Convert angle to position on pie (0° = top, clockwise)
                    float rad = (90f - midAngle) * Mathf.Deg2Rad;
                    float labelRadius = _chartSize * 0.32f; // Inside the pie
                    float labelX = Mathf.Cos(rad) * labelRadius;
                    float labelY = Mathf.Sin(rad) * labelRadius;

                    GameObject labelObj = new GameObject("SliceLabel_" + slice.Label);
                    RectTransform labelRect = labelObj.AddComponent<RectTransform>();
                    labelRect.SetParent(_pieContainer.transform, false);
                    labelRect.anchorMin = new Vector2(0.5f, 0.5f);
                    labelRect.anchorMax = new Vector2(0.5f, 0.5f);
                    labelRect.anchoredPosition = new Vector2(labelX, labelY);
                    labelRect.sizeDelta = new Vector2(100f, 20f);

                    Text labelText = labelObj.AddComponent<Text>();
                    labelText.text = slice.Label;
                    labelText.font = GameTheme.GameFont;
                    labelText.fontSize = Mathf.Max(10, GameTheme.SmallFontSize);
                    labelText.color = Color.white;
                    labelText.alignment = TextAnchor.MiddleCenter;
                    labelText.raycastTarget = false;

                    // Add shadow for readability
                    Shadow shadow = labelObj.AddComponent<Shadow>();
                    shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
                    shadow.effectDistance = new Vector2(1f, -1f);
                }

                currentAngle += sliceAngle;
            }

            // Build legend entries
            for (int i = 0; i < _slices.Count; i++)
            {
                var slice = _slices[i];
                float pct = (slice.Value / total) * 100f;

                GameObject legendRow = new GameObject("LegendItem_" + i);
                RectTransform legendRowRect = legendRow.AddComponent<RectTransform>();
                legendRowRect.SetParent(_legendContainer.transform, false);

                HorizontalLayoutGroup rowHlg = legendRow.AddComponent<HorizontalLayoutGroup>();
                rowHlg.childControlWidth = true;
                rowHlg.childControlHeight = true;
                rowHlg.childForceExpandWidth = false;
                rowHlg.childForceExpandHeight = false;
                rowHlg.spacing = 6f;
                rowHlg.childAlignment = TextAnchor.MiddleLeft;

                // Color swatch
                GameObject swatch = new GameObject("Swatch");
                swatch.AddComponent<RectTransform>().SetParent(legendRow.transform, false);
                Image swatchImg = swatch.AddComponent<Image>();
                swatchImg.sprite = circle;
                swatchImg.color = slice.SliceColor;
                LayoutElement swatchLe = swatch.AddComponent<LayoutElement>();
                swatchLe.minWidth = 14f;
                swatchLe.preferredWidth = 14f;
                swatchLe.minHeight = 14f;
                swatchLe.preferredHeight = 14f;

                // Label + percentage
                GameObject labelObj = new GameObject("Label");
                labelObj.AddComponent<RectTransform>().SetParent(legendRow.transform, false);
                Text labelText = labelObj.AddComponent<Text>();
                labelText.text = string.Format("{0} ({1:F0}%)", slice.Label, pct);
                labelText.font = GameTheme.GameFont;
                labelText.fontSize = GameTheme.SmallFontSize;
                labelText.color = GameTheme.LabelColor;
                labelText.alignment = TextAnchor.MiddleLeft;
                LayoutElement labelLe = labelObj.AddComponent<LayoutElement>();
                labelLe.flexibleWidth = 1f;
            }
        }

        /// <summary>
        /// Returns white or dark text color based on slice brightness for readability.
        /// </summary>
        private Color GetContrastColor(Color bg)
        {
            float luminance = 0.299f * bg.r + 0.587f * bg.g + 0.114f * bg.b;
            return luminance > 0.5f ? new Color(0.1f, 0.1f, 0.1f, 1f) : new Color(1f, 1f, 1f, 0.95f);
        }
    }
}
