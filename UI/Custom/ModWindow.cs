using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A fully custom window built from raw GameObjects.
/// Supports: drag, resize, collapse, pin (always-on-top), close, and geometry persistence.
/// 
/// Usage:
///   var window = ModWindow.Create("My Window", 400, 300);
///   // Add widgets to window.ContentPanel
///   window.Show();
/// </summary>
namespace ModFramework.UI.Custom
{
    public class ModWindow
    {
        // --- Public Properties ---
        public string Title { get; private set; }
        public GameObject Root { get; private set; }
        public GameObject ContentPanel { get; private set; }
        public bool IsVisible { get { return Root != null && Root.activeSelf; } }
        public bool IsCollapsed { get; private set; }
        public bool IsPinned { get; private set; }

        // --- Resize ---
        private ResizeHandler _resizeHandler;
        private bool _resizable = true;

        // --- Live Refresh ---
        private ModRefreshDriver _refreshDriver;

        /// <summary>
        /// The refresh driver for this window. Lazily created on first access.
        /// Use OnRefresh() for a simpler API.
        /// </summary>
        public ModRefreshDriver RefreshDriver
        {
            get
            {
                if (_refreshDriver == null && Root != null)
                {
                    _refreshDriver = Root.AddComponent<ModRefreshDriver>();
                }
                return _refreshDriver;
            }
        }

        // --- Events ---
        public event Action OnClose;
        public event Action OnCollapse;
        public event Action OnPin;

        // --- Internals ---
        private RectTransform _rootRect;
        private GameObject _titleBar;
        private Text _titleText;
        private GameObject _contentContainer;
        private float _expandedHeight;
        private float _windowWidth;
        private float _windowHeight;
        private string _persistKey;

        // --- Fallback Canvas (only used if WindowManager is entirely missing) ---
        private static Canvas _fallbackCanvas;

        /// <summary>
        /// Creates a new ModWindow. Does NOT show it yet - call Show() when ready.
        /// </summary>
        public static ModWindow Create(string title, float width, float height, string singletonKey = null)
        {
            GameTheme.Initialize();

            // Singleton check
            if (!string.IsNullOrEmpty(singletonKey))
            {
                ModWindow existing = ModWindowRegistry.GetSingleton(singletonKey);
                if (existing != null && existing.Root != null)
                {
                    existing.Show();
                    return existing;
                }
            }

            ModWindow window = new ModWindow();
            window.Title = title;
            window._windowWidth = width;
            window._windowHeight = height;
            window._expandedHeight = height;
            window._persistKey = !string.IsNullOrEmpty(singletonKey) ? singletonKey : title;

            window.BuildWindow();

            // Register singleton
            if (!string.IsNullOrEmpty(singletonKey))
            {
                ModWindowRegistry.TryRegisterSingleton(singletonKey, window);
            }

            return window;
        }

        // =====================================================================
        //  PUBLIC API
        // =====================================================================

        public void Show()
        {
            if (Root == null) return;

            // Re-parent in case game canvas was destroyed (e.g. reload save)
            Transform canvasTransform = GetCanvasTransform();
            if (_rootRect.parent != canvasTransform)
            {
                _rootRect.SetParent(canvasTransform, false);
            }

            Root.SetActive(true);
            _rootRect.SetAsLastSibling();
            ModWindowRegistry.Register(this);
            ModWindowRegistry.SetFocused(this);
            RestoreGeometry();
        }

        public void Hide()
        {
            if (Root == null) return;
            SaveGeometry();
            Root.SetActive(false);
        }

        public void Close()
        {
            SaveGeometry();
            ModWindowRegistry.Unregister(this);
            if (OnClose != null) OnClose.Invoke();
            if (Root != null) UnityEngine.Object.Destroy(Root);
            Root = null;
            ContentPanel = null;
        }

        public void ToggleCollapse()
        {
            IsCollapsed = !IsCollapsed;
            if (_contentContainer != null)
                _contentContainer.SetActive(!IsCollapsed);

            if (IsCollapsed)
                _rootRect.sizeDelta = new Vector2(_windowWidth, GameTheme.TitleBarHeight);
            else
                _rootRect.sizeDelta = new Vector2(_windowWidth, _expandedHeight);

            if (OnCollapse != null) OnCollapse.Invoke();
        }

        public void TogglePin()
        {
            IsPinned = !IsPinned;
            if (IsPinned) _rootRect.SetAsLastSibling();
            if (OnPin != null) OnPin.Invoke();
        }

        /// <summary>
        /// Register a callback that fires every N seconds while the window is visible.
        /// Updates are in-place (no destroy/recreate). Default interval: 3 seconds.
        /// </summary>
        public void OnRefresh(Action callback)
        {
            RefreshDriver.Register(callback);
        }

        /// <summary>
        /// Set the refresh interval in seconds (default 3s).
        /// Lower values = more frequent updates but higher CPU cost.
        /// </summary>
        public void SetRefreshInterval(float seconds)
        {
            RefreshDriver.Interval = seconds;
        }

        /// <summary>
        /// Force an immediate refresh tick (resets timer). 
        /// Useful after initial setup to populate live widgets right away.
        /// </summary>
        public void RefreshNow()
        {
            if (_refreshDriver != null) _refreshDriver.RefreshNow();
        }

        // =====================================================================
        //  RESIZING
        // =====================================================================

        /// <summary>
        /// Set minimum dimensions for the window. Default is 200x150.
        /// </summary>
        public void SetMinSize(float width, float height)
        {
            if (_resizeHandler != null) _resizeHandler.SetMinSize(new Vector2(width, height));
        }

        /// <summary>
        /// Set maximum dimensions for the window. Default is 1600x1000.
        /// </summary>
        public void SetMaxSize(float width, float height)
        {
            if (_resizeHandler != null) _resizeHandler.SetMaxSize(new Vector2(width, height));
        }

        /// <summary>
        /// Enable or disable the window resize handle.
        /// </summary>
        public void SetResizable(bool resizable)
        {
            _resizable = resizable;
            if (_resizeHandler != null) _resizeHandler.gameObject.SetActive(resizable);
        }

        // =====================================================================
        //  WINDOW CONSTRUCTION
        // =====================================================================

        private void BuildWindow()
        {
            // --- Root ---
            Root = new GameObject("ModWindow_" + Title);
            _rootRect = Root.AddComponent<RectTransform>();
            _rootRect.SetParent(GetCanvasTransform(), false);
            _rootRect.sizeDelta = new Vector2(_windowWidth, _windowHeight);
            _rootRect.anchoredPosition = Vector2.zero;
            _rootRect.pivot = new Vector2(0.5f, 1f); // Pivot at Top to collapse UP

            // Window background (acts as the colored header and border)
            Image windowBg = Root.AddComponent<Image>();
            windowBg.color = GameTheme.TitleBarColor;
            if (GameTheme.WindowSprite != null)
            {
                windowBg.sprite = GameTheme.WindowSprite;
                windowBg.type = Image.Type.Sliced;
            }

            // Make entire window clickable for focus
            FocusTracker focus = Root.AddComponent<FocusTracker>();
            focus.Setup(this, _rootRect);

            // --- Title Bar (manually positioned at top) ---
            BuildTitleBar();

            // --- Content Container (fills area below title bar) ---
            _contentContainer = new GameObject("Content");
            RectTransform contentRect = _contentContainer.AddComponent<RectTransform>();
            contentRect.SetParent(Root.transform, false);

            // Anchor to fill below title bar (creating a 2px border around it too)
            contentRect.anchorMin = new Vector2(0f, 0f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.offsetMin = new Vector2(2f, 2f); // bottom/left border
            contentRect.offsetMax = new Vector2(-2f, -GameTheme.TitleBarHeight); // top/right border

            // Container background (gray body with rounded corners from the sprite)
            Image ccBg = _contentContainer.AddComponent<Image>();
            ccBg.color = GameTheme.WindowBackground;
            if (GameTheme.WindowSprite != null)
            {
                ccBg.sprite = GameTheme.WindowSprite;
                ccBg.type = Image.Type.Sliced;
            }

            // Content panel - the actual area users add widgets to
            ContentPanel = new GameObject("ContentPanel");
            RectTransform cpRect = ContentPanel.AddComponent<RectTransform>();
            cpRect.SetParent(contentRect, false);
            cpRect.anchorMin = Vector2.zero;
            cpRect.anchorMax = Vector2.one;
            cpRect.sizeDelta = Vector2.zero;
            cpRect.anchoredPosition = Vector2.zero;

            // Vertical layout for stacking widgets
            VerticalLayoutGroup layout = ContentPanel.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = GameTheme.SectionSpacing;
            layout.padding = new RectOffset(
                (int)GameTheme.WindowPadding,
                (int)GameTheme.WindowPadding,
                (int)GameTheme.WindowPadding,
                (int)GameTheme.WindowPadding
            );

            // --- Resize Handle (bottom-right corner grip) ---
            GameObject resizeObj = new GameObject("ResizeHandle");
            RectTransform resizeRect = resizeObj.AddComponent<RectTransform>();
            resizeRect.SetParent(Root.transform, false);

            // Anchor to bottom-right corner of Root
            resizeRect.anchorMin = new Vector2(1f, 0f);
            resizeRect.anchorMax = new Vector2(1f, 0f);
            resizeRect.pivot = new Vector2(1f, 0f);
            resizeRect.sizeDelta = new Vector2(16f, 16f);
            resizeRect.anchoredPosition = Vector2.zero;

            // Grip visual container (we don't tint the background itself so it stays transparent)
            Image resizeBg = resizeObj.AddComponent<Image>();
            resizeBg.color = new Color(0, 0, 0, 0); // invisible hit box

            GameObject gripLabel = new GameObject("Grip");
            RectTransform gripRect = gripLabel.AddComponent<RectTransform>();
            gripRect.SetParent(resizeObj.transform, false);
            gripRect.anchorMin = Vector2.zero;
            gripRect.anchorMax = Vector2.one;
            gripRect.sizeDelta = Vector2.zero;

            Text gripText = gripLabel.AddComponent<Text>();
            // Use unicode bottom right triangle
            gripText.text = "\u25E2";
            gripText.font = GameTheme.GameFont;
            gripText.fontSize = 12; // Slightly larger for clarity
            gripText.color = new Color(0.6f, 0.6f, 0.6f, 0.5f); // Semi-transparent grey
            gripText.alignment = TextAnchor.LowerRight;
            
            // Add handler
            _resizeHandler = resizeObj.AddComponent<ResizeHandler>();
            _resizeHandler.SetTarget(_rootRect);
            
            // Toggle visibility based on initial state
            resizeObj.SetActive(_resizable);
        }

        private void BuildTitleBar()
        {
            _titleBar = new GameObject("TitleBar");
            RectTransform tbRect = _titleBar.AddComponent<RectTransform>();
            tbRect.SetParent(Root.transform, false);

            // Anchor to top of window, full width
            tbRect.anchorMin = new Vector2(0f, 1f);
            tbRect.anchorMax = new Vector2(1f, 1f);
            tbRect.pivot = new Vector2(0.5f, 1f);
            tbRect.sizeDelta = new Vector2(0f, GameTheme.TitleBarHeight);
            tbRect.anchoredPosition = Vector2.zero;

            // Title bar background is clear because the Root shows the colored header background!
            Image tbBg = _titleBar.AddComponent<Image>();
            tbBg.color = Color.clear;

            // Make title bar draggable
            DragHandler drag = _titleBar.AddComponent<DragHandler>();
            drag.SetTarget(_rootRect);

            // --- Title Text ---
            GameObject titleObj = new GameObject("Title");
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.SetParent(_titleBar.transform, false);
            titleRect.anchorMin = new Vector2(0f, 0f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(12f, 0f);
            titleRect.offsetMax = new Vector2(-70f, 0f); // Leave room for buttons

            _titleText = titleObj.AddComponent<Text>();
            _titleText.text = Title;
            _titleText.font = GameTheme.GameFont;
            _titleText.fontSize = GameTheme.HeaderFontSize; // Slightly larger for title
            _titleText.fontStyle = FontStyle.Bold;
            _titleText.color = Color.white; // White text on colored header
            _titleText.alignment = TextAnchor.MiddleLeft;
            _titleText.horizontalOverflow = HorizontalWrapMode.Overflow;

            Outline outline = titleObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.4f); // Subtle dark outline
            outline.effectDistance = new Vector2(1f, -1f);

            // --- Close button (Right) ---
            CreateTitleBarButton("X", 0f, GameTheme.HeaderFontSize + 2, () => Close());

            // --- Collapse button (Left of Close) ---
            CreateTitleBarButton("−", -28f, GameTheme.HeaderFontSize + 6, () => ToggleCollapse());
        }

        private void CreateTitleBarButton(string label, float rightOffset, int fontSize, Action onClick)
        {
            GameObject btnObj = new GameObject("TitleBtn_" + label);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.SetParent(_titleBar.transform, false);

            // Anchor to right
            btnRect.anchorMin = new Vector2(1f, 0.5f);
            btnRect.anchorMax = new Vector2(1f, 0.5f);
            btnRect.pivot = new Vector2(1f, 0.5f);
            btnRect.sizeDelta = new Vector2(28f, 28f); // 28x28 square fills title bar
            btnRect.anchoredPosition = new Vector2(rightOffset, 0f);

            Image btnBg = btnObj.AddComponent<Image>();
            // MUST be white! If Image graphic is completely clear, ColorBlock tinting mathematically does nothing.
            btnBg.color = Color.white; 

            Button btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(new UnityEngine.Events.UnityAction(() => onClick()));

            ColorBlock colors = btn.colors;
            // Normal state is completely transparent
            colors.normalColor = Color.clear;
            // Hover/Pressed overlay translucent dark
            colors.highlightedColor = new Color(0f, 0f, 0f, 0.1f);
            colors.pressedColor = new Color(0f, 0f, 0f, 0.2f);
            colors.colorMultiplier = 1f;
            btn.colors = colors;
            btn.targetGraphic = btnBg;

            // Button label
            GameObject labelObj = new GameObject("Label");
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.SetParent(btnObj.transform, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;

            Text text = labelObj.AddComponent<Text>();
            text.text = label;
            text.font = GameTheme.GameFont;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark grey text
            text.alignment = TextAnchor.MiddleCenter;
        }

        // =====================================================================
        //  GEOMETRY PERSISTENCE
        // =====================================================================

        private void SaveGeometry()
        {
            if (_rootRect == null || string.IsNullOrEmpty(_persistKey)) return;
            string prefix = "ModWindow_" + _persistKey;
            ModSettings.SetFloat(prefix + "_x", _rootRect.anchoredPosition.x);
            ModSettings.SetFloat(prefix + "_y", _rootRect.anchoredPosition.y);
            ModSettings.SetFloat(prefix + "_w", _rootRect.sizeDelta.x);
            ModSettings.SetFloat(prefix + "_h", IsCollapsed ? _expandedHeight : _rootRect.sizeDelta.y);
        }

        private void RestoreGeometry()
        {
            if (_rootRect == null || string.IsNullOrEmpty(_persistKey)) return;
            string prefix = "ModWindow_" + _persistKey;
            float x = ModSettings.GetFloat(prefix + "_x", float.NaN);
            if (float.IsNaN(x)) return;

            float y = ModSettings.GetFloat(prefix + "_y", 0f);
            float w = ModSettings.GetFloat(prefix + "_w", _windowWidth);
            float h = ModSettings.GetFloat(prefix + "_h", _windowHeight);

            _rootRect.anchoredPosition = new Vector2(x, y);
            _windowWidth = w;
            _expandedHeight = h;
            _rootRect.sizeDelta = new Vector2(w, IsCollapsed ? GameTheme.TitleBarHeight : h);
        }

        // =====================================================================
        //  CANVAS MANAGEMENT
        // =====================================================================

        private static Transform GetCanvasTransform()
        {
            // Always try to attach to the game's actual Canvas so we sort correctly with native UI
            if (WindowManager.Instance != null && WindowManager.Instance.Canvas != null)
            {
                return WindowManager.Instance.Canvas.transform;
            }

            // Fallback independent canvas if WindowManager is missing or we're running early
            if (_fallbackCanvas == null)
            {
                GameObject canvasObj = new GameObject("ModFramework_FallbackCanvas");
                UnityEngine.Object.DontDestroyOnLoad(canvasObj);

                _fallbackCanvas = canvasObj.AddComponent<Canvas>();
                _fallbackCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _fallbackCanvas.sortingOrder = 10;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            return _fallbackCanvas.transform;
        }
    }
}
