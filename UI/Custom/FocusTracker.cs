using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Tracks which ModWindow has focus (was last clicked).
/// Attach to the window root — clicking anywhere on it brings it to front.
/// </summary>
namespace ModFramework.UI.Custom
{
    public class FocusTracker : MonoBehaviour, IPointerDownHandler
    {
        private RectTransform _windowRect;
        private ModWindow _owner;

        public void Setup(ModWindow owner, RectTransform windowRect)
        {
            _owner = owner;
            _windowRect = windowRect;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_windowRect != null)
            {
                // Bring to front in Unity's sibling order
                _windowRect.SetAsLastSibling();
            }

            // Notify the registry
            if (_owner != null)
            {
                ModWindowRegistry.SetFocused(_owner);
            }
        }
    }
}
