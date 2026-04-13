using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to the bottom-right corner of a window to allow resizing.
/// The parent window must have its pivot set to (0.5, 1) to anchor at the top.
/// </summary>
namespace ModFramework.UI.Custom
{
    public class ResizeHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform _windowRect;
        private Vector2 _minSize = new Vector2(200f, 150f);
        private Vector2 _maxSize = new Vector2(1600f, 1000f);
        private Vector2 _startSize;
        private Vector2 _startMousePos;
        
        private GameObject _contentArea;

        /// <summary>
        /// Set the target window RectTransform and optional constraints.
        /// </summary>
        public void SetTarget(RectTransform windowRect)
        {
            _windowRect = windowRect;
        }

        public void SetMinSize(Vector2 min) { _minSize = min; }
        public void SetMaxSize(Vector2 max) { _maxSize = max; }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_windowRect == null) return;
            
            _startSize = _windowRect.sizeDelta;
            
            RectTransform parentRect = _windowRect.parent as RectTransform;
            if (parentRect == null) return;

            // PERFORMANCE FIX: Hide the content area during resize.
            // Adjusting sizeDelta triggers the exact same LayoutGroup diryt-cascade
            // as anchoredPosition. Hiding content prevents 300+ cells from recalculating every frame.
            _contentArea = FindContentArea();
            if (_contentArea != null) _contentArea.SetActive(false);

            // Convert mouse position to local space of the window's parent
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out _startMousePos);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_windowRect == null) return;
            
            RectTransform parentRect = _windowRect.parent as RectTransform;
            if (parentRect == null) return;

            Vector2 currentMouse;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out currentMouse))
            {
                Vector2 delta = currentMouse - _startMousePos;
                
                // Pivot is (0.5, 1.0) -- top-center
                // Dragging right = increase width (delta.x)
                // Dragging down = increase height (but Y is inverted in screen space)
                float newWidth = Mathf.Clamp(_startSize.x + delta.x * 2f, _minSize.x, _maxSize.x); // x2 because pivot is center horizontally
                float newHeight = Mathf.Clamp(_startSize.y - delta.y, _minSize.y, _maxSize.y);
                
                _windowRect.sizeDelta = new Vector2(newWidth, newHeight);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Restore content area
            if (_contentArea != null) _contentArea.SetActive(true);
            _contentArea = null;
        }

        private GameObject FindContentArea()
        {
            if (_windowRect == null) return null;
            Transform content = _windowRect.Find("Content");
            return content != null ? content.gameObject : null;
        }
    }
}
