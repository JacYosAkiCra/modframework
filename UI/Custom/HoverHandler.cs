using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Provides subtle hover effects for interactive elements (buttons, rows, etc.).
/// Changes alpha on hover to give "quiet richness" feel.
/// </summary>
namespace ModFramework.UI.Custom
{
    public class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Graphic _target;
        private Color _originalColor;
        private float _hoverDelta = 0.08f;

        public void Setup(Graphic target, float hoverDelta)
        {
            _target = target;
            _hoverDelta = hoverDelta;
            if (_target != null)
                _originalColor = _target.color;
        }

        public void Setup(Graphic target)
        {
            Setup(target, GameTheme.HoverAlphaDelta);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_target == null) return;
            Color hovered = _originalColor;
            // Darken slightly for light backgrounds, lighten for dark
            if (_originalColor.grayscale > 0.5f)
            {
                hovered.r -= _hoverDelta;
                hovered.g -= _hoverDelta;
                hovered.b -= _hoverDelta;
            }
            else
            {
                hovered.r += _hoverDelta;
                hovered.g += _hoverDelta;
                hovered.b += _hoverDelta;
            }
            _target.color = hovered;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_target == null) return;
            _target.color = _originalColor;
        }

        /// <summary>
        /// Call this if the base color changes (e.g., theme update).
        /// </summary>
        public void UpdateBaseColor(Color newColor)
        {
            _originalColor = newColor;
            if (_target != null)
                _target.color = newColor;
        }
    }
}
