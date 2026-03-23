using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to a title bar or drag handle to make the parent window draggable.
/// Clamps position to screen bounds so windows can't be lost off-screen.
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

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_windowRect == null) return;
            RectTransform parentRect = _windowRect.parent as RectTransform;
            if (parentRect == null) return;

            _isDragging = true;

            // Calculate offset between mouse and window position
            Vector2 mousePos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out mousePos))
            {
                _dragOffset = (Vector2)_windowRect.localPosition - mousePos;
            }

            // Bring to front
            _windowRect.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _windowRect == null) return;
            RectTransform parentRect = _windowRect.parent as RectTransform;
            if (parentRect == null) return;

            Vector2 mousePos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out mousePos))
            {
                _windowRect.localPosition = (Vector3)mousePos + (Vector3)_dragOffset;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            _isDragging = false;

            // Clamp to screen
            ClampToScreen();
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
