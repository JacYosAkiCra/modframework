using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Line/trend chart using a custom MaskableGraphic for smooth anti-aliased lines.
/// Supports multiple series with auto-scaling Y-axis and proper mesh-based rendering.
/// </summary>
namespace ModFramework.UI.Custom
{
    public class LineSeries
    {
        public string Name;
        public float[] Values;
        public Color LineColor;

        public LineSeries(string name, float[] values, Color color)
        {
            Name = name;
            Values = values;
            LineColor = color;
        }
    }

    public class ModLineChart
    {
        public GameObject Root;
        private GameObject _chartArea;
        private GameObject _legendContainer;
        private float _chartHeight;
        private float _lineThickness = 1.0f;
        private List<LineSeries> _series = new List<LineSeries>();

        /// <summary>
        /// Create a line chart inside the given parent.
        /// </summary>
        public static ModLineChart Create(GameObject parent, float height = 200f)
        {
            var chart = new ModLineChart();
            chart._chartHeight = height;

            // Root vertical container
            chart.Root = new GameObject("ModLineChart");
            RectTransform rootRect = chart.Root.AddComponent<RectTransform>();
            rootRect.SetParent(parent.transform, false);

            VerticalLayoutGroup rootVlg = chart.Root.AddComponent<VerticalLayoutGroup>();
            rootVlg.childControlWidth = true;
            rootVlg.childControlHeight = true;
            rootVlg.childForceExpandWidth = true;
            rootVlg.childForceExpandHeight = false;
            rootVlg.spacing = 4f;
            rootVlg.padding = new RectOffset(4, 4, 4, 4);

            LayoutElement rootLe = chart.Root.AddComponent<LayoutElement>();
            rootLe.preferredHeight = height + 30f;
            rootLe.flexibleWidth = 1f;

            // Chart drawing area - dark background like game's finance chart
            chart._chartArea = new GameObject("ChartArea");
            RectTransform areaRect = chart._chartArea.AddComponent<RectTransform>();
            areaRect.SetParent(chart.Root.transform, false);

            LayoutElement areaLe = chart._chartArea.AddComponent<LayoutElement>();
            areaLe.preferredHeight = height;
            areaLe.flexibleWidth = 1f;

            // Dark background on the parent
            Image areaBg = chart._chartArea.AddComponent<Image>();
            areaBg.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

            // Child object for the mesh-based line renderer (separate from Image)
            GameObject lineLayer = new GameObject("LineLayer");
            RectTransform lineRect = lineLayer.AddComponent<RectTransform>();
            lineRect.SetParent(chart._chartArea.transform, false);
            lineRect.anchorMin = Vector2.zero;
            lineRect.anchorMax = Vector2.one;
            lineRect.sizeDelta = Vector2.zero;
            lineRect.anchoredPosition = Vector2.zero;

            // Legend row
            chart._legendContainer = new GameObject("Legend");
            RectTransform legendRect = chart._legendContainer.AddComponent<RectTransform>();
            legendRect.SetParent(chart.Root.transform, false);

            HorizontalLayoutGroup legendHlg = chart._legendContainer.AddComponent<HorizontalLayoutGroup>();
            legendHlg.childControlWidth = true;
            legendHlg.childControlHeight = true;
            legendHlg.childForceExpandWidth = false;
            legendHlg.childForceExpandHeight = false;
            legendHlg.spacing = 16f;
            legendHlg.childAlignment = TextAnchor.MiddleCenter;

            LayoutElement legendLe = chart._legendContainer.AddComponent<LayoutElement>();
            legendLe.preferredHeight = 22f;

            return chart;
        }

        public void AddSeries(string name, float[] values, Color color)
        {
            _series.Add(new LineSeries(name, values, color));
        }

        public void ClearSeries()
        {
            _series.Clear();
        }

        /// <summary>
        /// Rebuild the chart display. Call after adding all series.
        /// </summary>
        public void Rebuild()
        {
            // The line layer is the first child of chart area
            GameObject lineLayer = _chartArea.transform.GetChild(0).gameObject;

            // Clear line layer children (labels/grid from previous draw)
            for (int i = lineLayer.transform.childCount - 1; i >= 0; i--)
                Object.Destroy(lineLayer.transform.GetChild(i).gameObject);

            // Clear legend
            for (int i = _legendContainer.transform.childCount - 1; i >= 0; i--)
                Object.Destroy(_legendContainer.transform.GetChild(i).gameObject);

            if (_series.Count == 0) return;

            // Find global min/max across all series
            float globalMin = float.MaxValue;
            float globalMax = float.MinValue;
            int maxPoints = 0;

            foreach (var s in _series)
            {
                if (s.Values == null || s.Values.Length == 0) continue;
                if (s.Values.Length > maxPoints) maxPoints = s.Values.Length;
                foreach (float v in s.Values)
                {
                    if (v < globalMin) globalMin = v;
                    if (v > globalMax) globalMax = v;
                }
            }

            if (maxPoints < 2) return;
            float range = globalMax - globalMin;
            if (range <= 0f) range = 1f;
            globalMin -= range * 0.05f;
            globalMax += range * 0.05f;
            range = globalMax - globalMin;

            // Set up the mesh-based line renderer on the line layer
            var drawer = lineLayer.GetComponent<LineChartDrawer>();
            if (drawer == null)
                drawer = lineLayer.AddComponent<LineChartDrawer>();

            drawer.Setup(_series, globalMin, globalMax, range, maxPoints, _lineThickness, _chartHeight);

            // Build legend
            foreach (var s in _series)
            {
                GameObject entry = new GameObject("LegendEntry");
                RectTransform entryRect = entry.AddComponent<RectTransform>();
                entryRect.SetParent(_legendContainer.transform, false);

                HorizontalLayoutGroup entryHlg = entry.AddComponent<HorizontalLayoutGroup>();
                entryHlg.childControlWidth = true;
                entryHlg.childControlHeight = true;
                entryHlg.childForceExpandWidth = false;
                entryHlg.childForceExpandHeight = false;
                entryHlg.spacing = 4f;
                entryHlg.childAlignment = TextAnchor.MiddleLeft;

                // Color swatch
                GameObject swatch = new GameObject("Swatch");
                swatch.AddComponent<RectTransform>().SetParent(entry.transform, false);
                Image swatchImg = swatch.AddComponent<Image>();
                swatchImg.color = s.LineColor;
                LayoutElement swatchLe = swatch.AddComponent<LayoutElement>();
                swatchLe.minWidth = 24f;
                swatchLe.preferredWidth = 24f;
                swatchLe.minHeight = 3f;
                swatchLe.preferredHeight = 3f;

                // Label
                GameObject labelObj = new GameObject("Label");
                labelObj.AddComponent<RectTransform>().SetParent(entry.transform, false);
                Text labelText = labelObj.AddComponent<Text>();
                labelText.text = s.Name;
                labelText.font = GameTheme.GameFont;
                labelText.fontSize = GameTheme.SmallFontSize;
                labelText.color = GameTheme.LabelColor;
                labelText.alignment = TextAnchor.MiddleLeft;
                LayoutElement labelLe = labelObj.AddComponent<LayoutElement>();
                labelLe.preferredWidth = 80f;
            }
        }
    }

    /// <summary>
    /// Custom graphic that draws smooth lines using mesh triangulation.
    /// Lives on its own GameObject (separate from any Image).
    /// </summary>
    internal class LineChartDrawer : MaskableGraphic
    {
        private List<LineSeries> _series;
        private float _minVal, _maxVal, _range;
        private int _maxPoints;
        private float _thickness;
        private float _chartHeight;
        private bool _needsRebuild;

        public void Setup(List<LineSeries> series, float min, float max, float range, int maxPoints, float thickness, float chartHeight)
        {
            _series = new List<LineSeries>(series);
            _minVal = min;
            _maxVal = max;
            _range = range;
            _maxPoints = maxPoints;
            _thickness = thickness;
            _chartHeight = chartHeight;
            _needsRebuild = true;
            SetVerticesDirty();
        }

        void LateUpdate()
        {
            if (_needsRebuild)
            {
                _needsRebuild = false;
                DrawLabelsAndGrid();
                SetVerticesDirty();
            }
        }

        private void DrawLabelsAndGrid()
        {
            // Clear old labels/grid (children of this object)
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            RectTransform rect = GetComponent<RectTransform>();
            float w = rect.rect.width;
            float h = rect.rect.height;
            if (w <= 0 || h <= 0) { _needsRebuild = true; return; }

            // Grid lines (subtle white on dark background)
            // Use margin so labels don't overflow at top/bottom
            float marginY = 12f / h; // 12px margin
            for (int g = 0; g <= 4; g++)
            {
                float rawNormY = (float)g / 4f;
                float normY = marginY + rawNormY * (1f - 2f * marginY); // Map into [margin, 1-margin]
                float val = _minVal + (_range * rawNormY);

                // Grid line
                GameObject gridLine = new GameObject("Grid");
                RectTransform gridRect = gridLine.AddComponent<RectTransform>();
                gridRect.SetParent(transform, false);
                Image gridImg = gridLine.AddComponent<Image>();
                gridImg.color = new Color(1f, 1f, 1f, 0.08f);
                gridImg.raycastTarget = false;
                gridRect.anchorMin = new Vector2(0f, normY);
                gridRect.anchorMax = new Vector2(1f, normY);
                gridRect.sizeDelta = new Vector2(0f, 1f);
                gridRect.anchoredPosition = Vector2.zero;

                // Y-axis label
                GameObject labelObj = new GameObject("YLabel");
                RectTransform labelRect = labelObj.AddComponent<RectTransform>();
                labelRect.SetParent(transform, false);
                Text label = labelObj.AddComponent<Text>();
                label.text = val.ToString("F0");
                label.font = GameTheme.GameFont;
                label.fontSize = Mathf.Max(10, GameTheme.SmallFontSize - 1);
                label.color = new Color(1f, 1f, 1f, 0.5f);
                label.alignment = TextAnchor.MiddleLeft;
                label.raycastTarget = false;
                labelRect.anchorMin = new Vector2(0f, normY);
                labelRect.anchorMax = new Vector2(0f, normY);
                labelRect.sizeDelta = new Vector2(35f, 14f);
                labelRect.anchoredPosition = new Vector2(20f, 0f);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (_series == null || _series.Count == 0) return;

            Rect rect = GetPixelAdjustedRect();
            float w = rect.width;
            float h = rect.height;

            if (w <= 0 || h <= 0) return;

            float halfThick = _thickness;

            // Apply same margin as grid labels so lines align with grid
            float marginPx = 12f;
            float drawH = h - 2f * marginPx;
            float yBase = rect.yMin + marginPx;

            foreach (var series in _series)
            {
                if (series.Values == null || series.Values.Length < 2) continue;

                float xStep = w / (series.Values.Length - 1);

                // Draw line segments as quads
                for (int i = 0; i < series.Values.Length - 1; i++)
                {
                    float x1 = rect.xMin + i * xStep;
                    float y1 = yBase + ((series.Values[i] - _minVal) / _range * drawH);
                    float x2 = rect.xMin + (i + 1) * xStep;
                    float y2 = yBase + ((series.Values[i + 1] - _minVal) / _range * drawH);

                    AddLineQuad(vh, new Vector2(x1, y1), new Vector2(x2, y2), halfThick, series.LineColor);
                }
            }
        }

        private void AddLineQuad(VertexHelper vh, Vector2 a, Vector2 b, float halfWidth, Color color)
        {
            Vector2 dir = (b - a).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x) * halfWidth;

            int idx = vh.currentVertCount;

            UIVertex vert = UIVertex.simpleVert;
            vert.color = color;

            vert.position = a - perp; vh.AddVert(vert);
            vert.position = a + perp; vh.AddVert(vert);
            vert.position = b + perp; vh.AddVert(vert);
            vert.position = b - perp; vh.AddVert(vert);

            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx, idx + 2, idx + 3);
        }

        private void AddDotQuad(VertexHelper vh, Vector2 center, float size, Color color)
        {
            int idx = vh.currentVertCount;

            UIVertex vert = UIVertex.simpleVert;
            vert.color = color;

            vert.position = center + new Vector2(-size, -size); vh.AddVert(vert);
            vert.position = center + new Vector2(-size, size); vh.AddVert(vert);
            vert.position = center + new Vector2(size, size); vh.AddVert(vert);
            vert.position = center + new Vector2(size, -size); vh.AddVert(vert);

            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx, idx + 2, idx + 3);
        }
    }
}
