using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach to a title bar or drag handle to make the parent window draggable.
/// Clamps position to screen bounds so windows can't be lost off-screen.
/// 
/// PERFORMANCE: Hides the window's content area during drag so Unity
/// doesn't recalculate layouts on 300+ table cells every frame.
/// The title bar remains visible so users can see where they're dragging.
/// </summary>
namespace ModFramework.UI.Custom
{
    public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform _windowRect;
        private Vector2 _dragOffset;
        private bool _isDragging;

        /// <summary>
        /// Set this to the RectTransform of the window root (not the title bar).
        /// </summary>
        public void SetTarget(RectTransform windowRect)
        {
            _windowRect = windowRect;
        }

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private GameObject _contentArea;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_windowRect == null) return;
            _isDragging = true;
            _canvas = _windowRect.GetComponentInParent<Canvas>();

            // Disable raycasts to prevent per-frame hover calculations on child elements
            _canvasGroup = _windowRect.GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = _windowRect.gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.blocksRaycasts = false;

            // CRITICAL PERF FIX: Hide the content area during drag.
            // The content area contains 300+ table cells with LayoutGroups.
            // Moving ANY ancestor RectTransform forces Unity to recalculate ALL of them.
            // By deactivating content, there are no children to recalculate.
            _contentArea = FindContentArea();
            if (_contentArea != null) _contentArea.SetActive(false);

            // Bring to front
            _windowRect.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _windowRect == null) return;

            if (_canvas != null)
            {
                _windowRect.anchoredPosition += eventData.delta / _canvas.scaleFactor;
            }
            else
            {
                _windowRect.anchoredPosition += eventData.delta;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;

            // Restore raycasts
            if (_canvasGroup != null) _canvasGroup.blocksRaycasts = true;

            // Re-show content
            if (_contentArea != null) _contentArea.SetActive(true);
            _contentArea = null;

            // Clamp to screen
            ClampToScreen();
        }

        /// <summary>
        /// Find the "Content" child of the window root. This is the heavy part
        /// that contains all the tables, labels, buttons etc.
        /// </summary>
        private GameObject FindContentArea()
        {
            if (_windowRect == null) return null;
            Transform content = _windowRect.Find("Content");
            return content != null ? content.gameObject : null;
        }

        private void ClampToScreen()
        {
            if (_windowRect == null) return;

            Vector3 pos = _windowRect.position;
            Vector2 size = _windowRect.sizeDelta;

            // Keep at least 40px of the title bar visible on screen
            float minVisible = 40f;
            pos.x = Mathf.Clamp(pos.x, -size.x + minVisible, Screen.width - minVisible);
            pos.y = Mathf.Clamp(pos.y, minVisible, Screen.height + size.y - minVisible);

            _windowRect.position = pos;
        }
    }
}
