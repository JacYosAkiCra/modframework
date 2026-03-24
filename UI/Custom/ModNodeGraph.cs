using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A visual graph widget that displays nodes connected by lines.
/// Supports manual positioning or auto-layout (tree).
/// </summary>
namespace ModFramework.UI.Custom
{
    public class NodeGraphNode
    {
        public string Id;
        public string Label;
        public Color Color = GameTheme.ButtonNormal;
        public Vector2 Position;           // Set by auto-layout or manually. Top-left origin, Y goes down (negative).
        public Action OnClick;
        internal GameObject Visual;        // The rendered GameObject
        internal int Depth;                // BFS depth for tree layout
    }

    public class NodeGraphEdge
    {
        public string FromId;
        public string ToId;
        public Color LineColor = GameTheme.LabelColor;
    }

    public class ModNodeGraph
    {
        public GameObject Root;

        private List<NodeGraphNode> _nodes = new List<NodeGraphNode>();
        private List<NodeGraphEdge> _edges = new List<NodeGraphEdge>();
        private GameObject _nodeLayer;
        private GameObject _edgeLayer;
        private RectTransform _contentRect;

        // Node visual constants
        private const float NODE_WIDTH = 120f;
        private const float NODE_HEIGHT = 36f;
        private const float LAYER_SPACING_Y = 80f;
        private const float NODE_SPACING_X = 20f;
        private const float EDGE_THICKNESS = 2f;

        public static ModNodeGraph Create(GameObject parent, float width = -1f, float height = 300f)
        {
            var graph = new ModNodeGraph();

            // Root container
            graph.Root = new GameObject("ModNodeGraph");
            RectTransform rootRect = graph.Root.AddComponent<RectTransform>();
            rootRect.SetParent(parent.transform, false);

            LayoutElement layout = graph.Root.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
            if (width > 0f)
                layout.preferredWidth = width;
            else
                layout.flexibleWidth = 1f;

            // Scroll View internals
            ScrollRect scroll = graph.Root.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 15f;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            RectTransform viewRect = viewport.AddComponent<RectTransform>();
            viewRect.SetParent(rootRect, false);
            viewRect.anchorMin = Vector2.zero;
            viewRect.anchorMax = Vector2.one;
            viewRect.sizeDelta = Vector2.zero;
            viewRect.offsetMin = Vector2.zero;
            viewRect.offsetMax = Vector2.zero;
            viewport.AddComponent<RectMask2D>();

            // Background
            Image bg = viewport.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.15f, 0.15f);

            // Content container - top-left anchored so (0,0) = top-left
            // Y goes negative downward matching anchoredPosition convention
            GameObject content = new GameObject("GraphContent");
            graph._contentRect = content.AddComponent<RectTransform>();
            graph._contentRect.SetParent(viewRect, false);
            graph._contentRect.anchorMin = new Vector2(0f, 1f);
            graph._contentRect.anchorMax = new Vector2(0f, 1f);
            graph._contentRect.pivot     = new Vector2(0f, 1f);
            graph._contentRect.sizeDelta = new Vector2(1000f, 1000f);

            scroll.viewport = viewRect;
            scroll.content  = graph._contentRect;

            // Edge Layer - SAME pivot/anchor as content so local coords match node positions
            graph._edgeLayer = new GameObject("EdgeLayer");
            RectTransform edgeRect = graph._edgeLayer.AddComponent<RectTransform>();
            edgeRect.SetParent(graph._contentRect, false);
            edgeRect.anchorMin = new Vector2(0f, 1f);
            edgeRect.anchorMax = new Vector2(0f, 1f);
            edgeRect.pivot     = new Vector2(0f, 1f);
            edgeRect.sizeDelta = new Vector2(1000f, 1000f);

            // Node Layer - same setup
            graph._nodeLayer = new GameObject("NodeLayer");
            RectTransform nodeRect = graph._nodeLayer.AddComponent<RectTransform>();
            nodeRect.SetParent(graph._contentRect, false);
            nodeRect.anchorMin = new Vector2(0f, 1f);
            nodeRect.anchorMax = new Vector2(0f, 1f);
            nodeRect.pivot     = new Vector2(0f, 1f);
            nodeRect.sizeDelta = new Vector2(1000f, 1000f);

            return graph;
        }

        public void AddNode(NodeGraphNode node) { _nodes.Add(node); }

        public void AddEdge(string fromId, string toId, Color? color = null)
        {
            _edges.Add(new NodeGraphEdge {
                FromId = fromId,
                ToId   = toId,
                LineColor = color ?? GameTheme.LabelColor
            });
        }

        public void Clear()
        {
            _nodes.Clear();
            _edges.Clear();
            Rebuild();
        }

        // --- Layout Algorithms ---

        public void AutoLayoutTree()
        {
            if (_nodes.Count == 0) return;

            // 1. Find root nodes - nodes with no incoming edges
            HashSet<string> hasIncoming = new HashSet<string>();
            foreach (var edge in _edges) hasIncoming.Add(edge.ToId);

            Queue<NodeGraphNode> queue = new Queue<NodeGraphNode>();
            HashSet<string> visited   = new HashSet<string>();

            // Initialize ALL nodes to depth -1 (unvisited marker)
            foreach (var node in _nodes)
            {
                node.Depth = -1;
            }

            // Seed BFS from roots (depth 0)
            foreach (var node in _nodes)
            {
                if (!hasIncoming.Contains(node.Id))
                {
                    node.Depth = 0;
                    queue.Enqueue(node);
                    visited.Add(node.Id);
                }
            }

            // If no clear root (cyclic graph), pick first node
            if (queue.Count == 0 && _nodes.Count > 0)
            {
                _nodes[0].Depth = 0;
                queue.Enqueue(_nodes[0]);
                visited.Add(_nodes[0].Id);
            }

            // BFS - assign depths
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var edge in _edges)
                {
                    if (edge.FromId != current.Id) continue;
                    if (visited.Contains(edge.ToId)) continue;

                    var child = _nodes.Find(n => n.Id == edge.ToId);
                    if (child == null) continue;

                    child.Depth = current.Depth + 1;
                    queue.Enqueue(child);
                    visited.Add(child.Id);
                }
            }

            // Anything still at -1 is disconnected - put it at depth 0
            foreach (var node in _nodes)
                if (node.Depth < 0) node.Depth = 0;

            // 2. Group by depth
            var layers = new Dictionary<int, List<NodeGraphNode>>();
            int maxDepth = 0;
            foreach (var node in _nodes)
            {
                if (!layers.ContainsKey(node.Depth))
                    layers[node.Depth] = new List<NodeGraphNode>();
                layers[node.Depth].Add(node);
                if (node.Depth > maxDepth) maxDepth = node.Depth;
            }

            // 3. Find widest layer to size graph
            float maxLayerPx = 0f;
            foreach (var kvp in layers)
            {
                float w = kvp.Value.Count * (NODE_WIDTH + NODE_SPACING_X) - NODE_SPACING_X;
                if (w > maxLayerPx) maxLayerPx = w;
            }
            float treeHeight = (maxDepth + 1) * LAYER_SPACING_Y + NODE_HEIGHT;
            
            // Create a large, freely pannable canvas
            float totalWidth = Mathf.Max(maxLayerPx + 600f, 1500f);
            float totalHeight = Mathf.Max(treeHeight + 400f, 1000f);

            // Resize content to fit the tree (both width and height must be positive and large enough)
            _contentRect.sizeDelta = new Vector2(totalWidth, totalHeight);

            // Fix edgeLayer / nodeLayer to match
            var edgeRt = _edgeLayer.GetComponent<RectTransform>();
            edgeRt.sizeDelta = new Vector2(totalWidth, totalHeight);
            var nodeRt = _nodeLayer.GetComponent<RectTransform>();
            nodeRt.sizeDelta = new Vector2(totalWidth, totalHeight);

            // 4. Position nodes - center the tree inside the large canvas
            float startYOffset = -(totalHeight - treeHeight) * 0.5f;

            foreach (var kvp in layers)
            {
                int depth = kvp.Key;
                var layerNodes = kvp.Value;
                int count = layerNodes.Count;
                float rowWidth = count * NODE_WIDTH + (count - 1) * NODE_SPACING_X;
                float startX   = (totalWidth - rowWidth) * 0.5f;

                for (int i = 0; i < count; i++)
                {
                    float y = startYOffset - (depth * LAYER_SPACING_Y + NODE_HEIGHT * 0.5f);
                    float x = startX + i * (NODE_WIDTH + NODE_SPACING_X) + NODE_WIDTH * 0.5f;
                    layerNodes[i].Position = new Vector2(x, y);
                }
            }
        }

        // --- Rendering ---

        public void Rebuild()
        {
            // Clear existing
            for (int i = _nodeLayer.transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(_nodeLayer.transform.GetChild(i).gameObject);

            var oldEdgeDrawer = _edgeLayer.GetComponent<NodeGraphEdgeDrawer>();
            if (oldEdgeDrawer != null) UnityEngine.Object.Destroy(oldEdgeDrawer);

            // Render Nodes
            foreach (var node in _nodes)
            {
                GameObject nodeObj = new GameObject("Node_" + node.Id);
                nodeObj.transform.SetParent(_nodeLayer.transform, false);

                RectTransform nodeRect = nodeObj.AddComponent<RectTransform>();
                // Anchor: same top-left origin as the layer
                nodeRect.anchorMin       = new Vector2(0f, 1f);
                nodeRect.anchorMax       = new Vector2(0f, 1f);
                nodeRect.pivot           = new Vector2(0.5f, 0.5f);
                nodeRect.sizeDelta       = new Vector2(NODE_WIDTH, NODE_HEIGHT);
                nodeRect.anchoredPosition = node.Position;
                node.Visual = nodeObj;

                Image nodeBg = nodeObj.AddComponent<Image>();
                nodeBg.color = node.Color;
                if (GameTheme.WindowSprite != null)
                {
                    nodeBg.sprite = GameTheme.WindowSprite;
                    nodeBg.type   = Image.Type.Sliced;
                }

                GameObject labelObj = new GameObject("Label");
                RectTransform labelRect = labelObj.AddComponent<RectTransform>();
                labelRect.SetParent(nodeObj.transform, false);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.sizeDelta = Vector2.zero;

                Text labelText = labelObj.AddComponent<Text>();
                labelText.text      = node.Label;
                labelText.font      = GameTheme.GameFont;
                labelText.fontSize  = GameTheme.SmallFontSize;
                labelText.color     = GameTheme.LabelColor;
                labelText.alignment = TextAnchor.MiddleCenter;

                if (node.OnClick != null)
                {
                    Button btn = nodeObj.AddComponent<Button>();
                    btn.onClick.AddListener(new UnityEngine.Events.UnityAction(() => node.OnClick()));
                    ColorBlock cb = btn.colors;
                    cb.normalColor      = Color.white;
                    cb.highlightedColor = new Color(0.85f, 0.85f, 0.85f);
                    cb.pressedColor     = new Color(0.7f, 0.7f, 0.7f);
                    cb.colorMultiplier  = 1f;
                    btn.colors = cb;
                }
            }

            // Render Edges
            if (_edges.Count > 0)
            {
                var edgeDrawer = _edgeLayer.AddComponent<NodeGraphEdgeDrawer>();
                var edgeData   = new List<NodeGraphEdgeDrawer.EdgeData>();

                // The EdgeLayer has the same anchor/pivot (top-left) as nodes, so
                // node.Position IS already in the EdgeLayer's local coordinate space.
                foreach (var edge in _edges)
                {
                    var fromNode = _nodes.Find(n => n.Id == edge.FromId);
                    var toNode   = _nodes.Find(n => n.Id == edge.ToId);
                    if (fromNode == null || toNode == null) continue;

                    // Line from bottom-center of "from" to top-center of "to"
                    Vector2 fromPos = fromNode.Position + new Vector2(0f, -NODE_HEIGHT * 0.5f);
                    Vector2 toPos   = toNode.Position   + new Vector2(0f,  NODE_HEIGHT * 0.5f);

                    edgeData.Add(new NodeGraphEdgeDrawer.EdgeData {
                        From  = fromPos,
                        To    = toPos,
                        Color = edge.LineColor
                    });
                }

                edgeDrawer.Setup(edgeData, EDGE_THICKNESS);
            }
        }
    }

    /// <summary>
    /// Custom MaskableGraphic that draws thick connecting lines for the node graph.
    /// Must share the same anchor/pivot origin as node RectTransforms so that
    /// local coordinates match node.Position values.
    /// </summary>
    internal class NodeGraphEdgeDrawer : MaskableGraphic
    {
        public struct EdgeData { public Vector2 From; public Vector2 To; public Color Color; }

        private List<EdgeData> _edges     = new List<EdgeData>();
        private float          _thickness = 2f;

        public void Setup(List<EdgeData> edges, float thickness)
        {
            _edges     = edges;
            _thickness = thickness;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (_edges == null || _edges.Count == 0) return;

            foreach (var edge in _edges)
            {
                Vector2 from = edge.From;
                Vector2 to   = edge.To;

                Vector2 dir  = (to - from);
                if (dir.sqrMagnitude < 0.001f) continue;
                dir = dir.normalized;

                Vector2 perp = new Vector2(-dir.y, dir.x) * (_thickness * 0.5f);

                Vector2 v0 = from - perp;
                Vector2 v1 = from + perp;
                Vector2 v2 = to   - perp;
                Vector2 v3 = to   + perp;

                int i0 = vh.currentVertCount;

                UIVertex vert = UIVertex.simpleVert;
                vert.color = edge.Color;

                vert.position = v0; vh.AddVert(vert);
                vert.position = v1; vh.AddVert(vert);
                vert.position = v2; vh.AddVert(vert);
                vert.position = v3; vh.AddVert(vert);

                vh.AddTriangle(i0,     i0 + 1, i0 + 2);
                vh.AddTriangle(i0 + 1, i0 + 3, i0 + 2);
            }
        }
    }
}
