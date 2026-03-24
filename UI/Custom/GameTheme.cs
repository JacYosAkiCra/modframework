using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Samples colors, fonts, and sizes from the game's native UI prefabs at runtime.
/// All values are cached as static fields after Initialize() is called once.
/// If a prefab can't be sampled, hardcoded fallbacks matching the game's look are used.
/// </summary>
namespace ModFramework.UI.Custom
{
    public static class GameTheme
    {
        // --- Colors ---
        public static Color WindowBackground { get; private set; }
        public static Color PanelBackground { get; private set; }
        public static Color ButtonNormal { get; private set; }
        public static Color ButtonHover { get; private set; }
        public static Color ButtonPressed { get; private set; }
        public static Color LabelColor { get; private set; }
        public static Color HeaderColor { get; private set; }       // Text color for section headers
        public static Color TitleBarColor { get; private set; }     // Background color for window title bar
        public static Color InputBackground { get; private set; }
        public static Color InputBorder { get; private set; }
        public static Color ScrollbarHandle { get; private set; }
        public static Color Separator { get; private set; }

        // --- Sprites ---
        public static Sprite WindowSprite { get; private set; }

        // --- Typography ---
        public static Font GameFont { get; private set; }
        public static int DefaultFontSize { get; private set; }
        public static int HeaderFontSize { get; private set; }
        public static int SmallFontSize { get; private set; }

        // --- Spacing tokens (visual language) ---
        public static float WindowPadding { get { return 10f; } }
        public static float SectionSpacing { get { return 8f; } }
        public static float RowHeight { get { return 28f; } }
        public static float ButtonHeight { get { return 26f; } }
        public static float TitleBarHeight { get { return 30f; } }
        public static float CornerRadius { get { return 0f; } } // Unity UI uses Image slicing, not CSS radius
        public static float HoverAlphaDelta { get { return 0.08f; } }

        // --- State ---
        private static bool _initialized;

        /// <summary>
        /// Call once (e.g., from mod Init or first window open).
        /// Spawns temporary prefabs, reads their styles, then destroys them.
        /// Safe to call multiple times - subsequent calls are no-ops.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            // --- Fallback defaults (match vanilla game look) ---
            WindowBackground = new Color(0.93f, 0.93f, 0.93f, 1f);
            PanelBackground = new Color(0.88f, 0.88f, 0.88f, 0.5f);
            ButtonNormal = Color.white;
            ButtonHover = new Color(0.9f, 0.95f, 1f, 1f);
            ButtonPressed = new Color(0.75f, 0.75f, 0.75f, 1f);
            LabelColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            HeaderColor = new Color(0.15f, 0.3f, 0.15f, 1f);
            TitleBarColor = new Color(0.48f, 0.76f, 0.43f, 1f); // Vanilla window header green
            InputBackground = Color.white;
            InputBorder = new Color(0.7f, 0.7f, 0.7f, 1f);
            ScrollbarHandle = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            Separator = new Color(0.7f, 0.7f, 0.7f, 0.5f);
            GameFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            DefaultFontSize = 14;
            HeaderFontSize = 16;
            SmallFontSize = 12;

            // --- Sample from live game prefabs ---
            TrySampleFromPrefabs();

            Debug.Log("[ModFramework] GameTheme initialized - " +
                      "Font: " + (GameFont != null ? GameFont.name : "null") +
                      ", FontSize: " + DefaultFontSize +
                      ", WindowBG: " + WindowBackground);
        }

        /// <summary>
        /// Force re-sampling (e.g., if user changes UI scale mid-game).
        /// </summary>
        public static void Reinitialize()
        {
            _initialized = false;
            Initialize();
        }

        private static void TrySampleFromPrefabs()
        {
            try
            {
                // --- Sample from a window ---
                GUIWindow tempWindow = WindowManager.SpawnWindow();
                if (tempWindow != null)
                {
                    Image windowImage = tempWindow.MainPanel != null
                        ? tempWindow.MainPanel.GetComponent<Image>()
                        : null;
                    if (windowImage != null)
                    {
                        WindowBackground = windowImage.color;
                        WindowSprite = windowImage.sprite;
                    }

                    Object.Destroy(tempWindow.gameObject);
                }

                // --- Sample from a button ---
                Button tempButton = WindowManager.SpawnButton();
                if (tempButton != null)
                {
                    Image btnImage = tempButton.GetComponent<Image>();
                    if (btnImage != null)
                        ButtonNormal = btnImage.color;

                    ColorBlock cb = tempButton.colors;
                    ButtonHover = cb.highlightedColor;
                    ButtonPressed = cb.pressedColor;

                    Object.Destroy(tempButton.gameObject);
                }

                // --- Sample from a label ---
                Text tempLabel = WindowManager.SpawnLabel();
                if (tempLabel != null)
                {
                    LabelColor = tempLabel.color;
                    if (tempLabel.font != null)
                        GameFont = tempLabel.font;
                    if (tempLabel.fontSize > 0)
                        DefaultFontSize = tempLabel.fontSize;

                    // Derive header size
                    HeaderFontSize = DefaultFontSize + 2;
                    SmallFontSize = Mathf.Max(10, DefaultFontSize - 2);

                    // Derive header color (slightly darker than label)
                    HeaderColor = new Color(
                        LabelColor.r * 0.6f,
                        LabelColor.g * 0.8f,
                        LabelColor.b * 0.6f,
                        1f
                    );

                    Object.Destroy(tempLabel.gameObject);
                }

                // --- Sample from an input field ---
                InputField tempInput = WindowManager.SpawnInputbox();
                if (tempInput != null)
                {
                    Image inputImage = tempInput.GetComponent<Image>();
                    if (inputImage != null)
                        InputBackground = inputImage.color;

                    Object.Destroy(tempInput.gameObject);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ModFramework] GameTheme sampling failed (using fallbacks): " + e.Message);
            }
        }
    }
}
