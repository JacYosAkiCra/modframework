using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small notification badge (counter) attachable to any UI element.
/// Renders as a colored circle with a number, positioned at the top-right corner.
/// </summary>
namespace ModFramework.UI.Custom
{
    public class ModNotificationBadge
    {
        public GameObject Root;
        private Text _countText;
        private int _count;

        /// <summary>
        /// Creates a white filled circle sprite at runtime for the badge background.
        /// </summary>
        private static Sprite _circleSprite;
        private static Sprite GetCircleSprite()
        {
            if (_circleSprite != null) return _circleSprite;

            int size = 64;
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
        /// Create a notification badge on the target element.
        /// </summary>
        public static ModNotificationBadge Create(GameObject target, int initialCount = 0, Color? badgeColor = null)
        {
            var badge = new ModNotificationBadge();
            badge._count = initialCount;

            // Badge container
            badge.Root = new GameObject("NotificationBadge");
            RectTransform badgeRect = badge.Root.AddComponent<RectTransform>();
            badgeRect.SetParent(target.transform, false);

            // Position at top-right corner
            badgeRect.anchorMin = new Vector2(1f, 1f);
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.anchoredPosition = new Vector2(-2f, -2f);
            badgeRect.sizeDelta = new Vector2(22f, 22f);

            // Circular background using sprite
            Image bg = badge.Root.AddComponent<Image>();
            bg.sprite = GetCircleSprite();
            bg.color = badgeColor ?? new Color(0.85f, 0.15f, 0.15f, 1f);
            bg.type = Image.Type.Simple;
            bg.preserveAspect = true;

            // Count text
            GameObject textObj = new GameObject("Count");
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.SetParent(badge.Root.transform, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            badge._countText = textObj.AddComponent<Text>();
            badge._countText.font = GameTheme.GameFont;
            badge._countText.fontSize = Mathf.Max(10, GameTheme.SmallFontSize - 1);
            badge._countText.color = Color.white;
            badge._countText.alignment = TextAnchor.MiddleCenter;
            badge._countText.fontStyle = FontStyle.Bold;

            // Set initial count
            badge.UpdateDisplay();

            return badge;
        }

        /// <summary>
        /// Set the badge count.
        /// </summary>
        public void SetCount(int count)
        {
            _count = count;
            UpdateDisplay();
        }

        /// <summary>
        /// Increment the badge count by 1.
        /// </summary>
        public void Increment()
        {
            _count++;
            UpdateDisplay();
        }

        /// <summary>
        /// Get the current count.
        /// </summary>
        public int Count { get { return _count; } }

        /// <summary>
        /// Show the badge.
        /// </summary>
        public void Show()
        {
            if (Root != null) Root.SetActive(true);
        }

        /// <summary>
        /// Hide the badge.
        /// </summary>
        public void Hide()
        {
            if (Root != null) Root.SetActive(false);
        }

        private void UpdateDisplay()
        {
            if (_countText != null)
            {
                _countText.text = _count > 99 ? "99+" : _count.ToString();
            }

            // Auto-hide when count is 0
            if (Root != null)
            {
                Root.SetActive(_count > 0);
            }

            // Grow badge for larger numbers
            if (Root != null)
            {
                RectTransform rect = Root.GetComponent<RectTransform>();
                if (rect != null)
                {
                    float width = _count > 99 ? 30f : (_count > 9 ? 26f : 22f);
                    rect.sizeDelta = new Vector2(width, 22f);
                }
            }
        }
    }
}
