using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Two-panel layout with a draggable divider.
/// Supports horizontal (left|right) split.
/// </summary>
namespace ModFramework.UI.Custom
{
    public class ModSplitPane
    {
        public GameObject Root;
        public GameObject LeftPanel;
        public GameObject RightPanel;
        private GameObject _divider;
        private float _splitRatio;

        /// <summary>
        /// Create a horizontal split pane (left | right).
        /// </summary>
        /// <param name="initialRatio">Initial split ratio (0 to 1). 0.3 = 30% left, 70% right.</param>
        public static ModSplitPane Create(GameObject parent, float initialRatio = 0.3f, float height = 300f)
        {
            var pane = new ModSplitPane();
            pane._splitRatio = Mathf.Clamp(initialRatio, 0.1f, 0.9f);

            // Root with horizontal layout
            pane.Root = new GameObject("ModSplitPane");
            RectTransform rootRect = pane.Root.AddComponent<RectTransform>();
            rootRect.SetParent(parent.transform, false);

            HorizontalLayoutGroup hlg = pane.Root.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.spacing = 0f;

            LayoutElement rootLe = pane.Root.AddComponent<LayoutElement>();
            rootLe.preferredHeight = height;
            rootLe.flexibleWidth = 1f;

            // Left panel
            pane.LeftPanel = new GameObject("LeftPanel");
            RectTransform leftRect = pane.LeftPanel.AddComponent<RectTransform>();
            leftRect.SetParent(pane.Root.transform, false);

            Image leftBg = pane.LeftPanel.AddComponent<Image>();
            leftBg.color = new Color(GameTheme.PanelBackground.r, GameTheme.PanelBackground.g, GameTheme.PanelBackground.b, 0.3f);

            VerticalLayoutGroup leftVlg = pane.LeftPanel.AddComponent<VerticalLayoutGroup>();
            leftVlg.childControlWidth = true;
            leftVlg.childControlHeight = true;
            leftVlg.childForceExpandWidth = true;
            leftVlg.childForceExpandHeight = false;
            leftVlg.spacing = 4f;
            leftVlg.padding = new RectOffset(4, 4, 4, 4);

            LayoutElement leftLe = pane.LeftPanel.AddComponent<LayoutElement>();
            leftLe.flexibleWidth = pane._splitRatio;
            leftLe.flexibleHeight = 1f;

            // Divider (draggable)
            pane._divider = new GameObject("Divider");
            RectTransform divRect = pane._divider.AddComponent<RectTransform>();
            divRect.SetParent(pane.Root.transform, false);

            Image divImg = pane._divider.AddComponent<Image>();
            divImg.color = GameTheme.Separator;

            LayoutElement divLe = pane._divider.AddComponent<LayoutElement>();
            divLe.minWidth = 4f;
            divLe.preferredWidth = 4f;

            // Drag handler for resize
            var dragger = pane._divider.AddComponent<SplitPaneDragger>();
            dragger.Setup(pane);

            // Right panel
            pane.RightPanel = new GameObject("RightPanel");
            RectTransform rightRect = pane.RightPanel.AddComponent<RectTransform>();
            rightRect.SetParent(pane.Root.transform, false);

            Image rightBg = pane.RightPanel.AddComponent<Image>();
            rightBg.color = new Color(GameTheme.PanelBackground.r, GameTheme.PanelBackground.g, GameTheme.PanelBackground.b, 0.2f);

            VerticalLayoutGroup rightVlg = pane.RightPanel.AddComponent<VerticalLayoutGroup>();
            rightVlg.childControlWidth = true;
            rightVlg.childControlHeight = true;
            rightVlg.childForceExpandWidth = true;
            rightVlg.childForceExpandHeight = false;
            rightVlg.spacing = 4f;
            rightVlg.padding = new RectOffset(4, 4, 4, 4);

            LayoutElement rightLe = pane.RightPanel.AddComponent<LayoutElement>();
            rightLe.flexibleWidth = 1f - pane._splitRatio;
            rightLe.flexibleHeight = 1f;

            return pane;
        }

        /// <summary>
        /// Update the split ratio (called by the dragger).
        /// </summary>
        internal void SetRatio(float ratio)
        {
            _splitRatio = Mathf.Clamp(ratio, 0.1f, 0.9f);
            var leftLe = LeftPanel.GetComponent<LayoutElement>();
            var rightLe = RightPanel.GetComponent<LayoutElement>();
            if (leftLe != null) leftLe.flexibleWidth = _splitRatio;
            if (rightLe != null) rightLe.flexibleWidth = 1f - _splitRatio;
        }
    }

    /// <summary>
    /// Handles dragging the split pane divider.
    /// </summary>
    internal class SplitPaneDragger : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        private ModSplitPane _pane;
        private RectTransform _rootRect;

        public void Setup(ModSplitPane pane)
        {
            _pane = pane;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_rootRect == null)
                _rootRect = _pane.Root.GetComponent<RectTransform>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_rootRect == null) return;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rootRect, eventData.position, eventData.pressEventCamera, out localPoint);

            float totalWidth = _rootRect.rect.width;
            if (totalWidth <= 0) return;

            // Convert local X to ratio (local origin is at pivot)
            float ratio = (localPoint.x + totalWidth * _rootRect.pivot.x) / totalWidth;
            _pane.SetRatio(ratio);
        }
    }
}
