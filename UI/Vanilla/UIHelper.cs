using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Helper methods for creating UI elements easily.
/// This wraps the game's native WindowManager prefab spawners.
/// </summary>
namespace ModFramework
{
    public static class UIHelper
    {
        // ========== PANELS ==========
        
        public static RectTransform AddPanel(Rect rect, GUIWindow window)
        {
            RectTransform panel = WindowManager.SpawnPanel();
            WindowManager.AddElementToWindow(panel.gameObject, window, rect, new Rect(0, 0, 0, 0));
            return panel;
        }

        public static RectTransform AddPanel(Rect rect, GameObject parent)
        {
            RectTransform panel = WindowManager.SpawnPanel();
            WindowManager.AddElementToElement(panel.gameObject, parent, rect, new Rect(0, 0, 0, 0));
            return panel;
        }

        // ========== BUTTONS ==========
        
        public static Button AddButton(string text, Rect rect, UnityAction action, GUIWindow window)
        {
            return AddButton(text, rect, action, window.MainPanel);
        }

        public static Button AddButton(string text, Rect rect, UnityAction action, GameObject panel)
        {
            Button btn = WindowManager.SpawnButton();
            btn.GetComponentInChildren<Text>().text = text;
            btn.onClick.AddListener(action);
            WindowManager.AddElementToElement(btn.gameObject, panel, rect, new Rect(0, 0, 0, 0));
            return btn;
        }

        // ========== LABELS ==========
        
        public static Text AddLabel(string text, Rect rect, GUIWindow window)
        {
            return AddLabel(text, rect, window.MainPanel);
        }

        public static Text AddLabel(string text, Rect rect, GameObject panel)
        {
            return AddLabel(text, rect, panel, 14);
        }

        public static Text AddLabel(string text, Rect rect, GameObject panel, int fontSize)
        {
            return AddLabel(text, rect, panel, fontSize, Color.black);
        }

        public static Text AddLabel(string text, Rect rect, GameObject panel, int fontSize, Color color)
        {
            Text label = WindowManager.SpawnLabel();
            label.text = text;
            label.color = color;
            label.fontSize = fontSize;
            WindowManager.AddElementToElement(label.gameObject, panel, rect, new Rect(0, 0, 0, 0));
            return label;
        }

        public static Text AddLabelBold(string text, Rect rect, GameObject panel)
        {
            return AddLabelBold(text, rect, panel, 14);
        }

        public static Text AddLabelBold(string text, Rect rect, GameObject panel, int fontSize)
        {
            return AddLabelBold(text, rect, panel, fontSize, new Color(0.1f, 0.1f, 0.1f));
        }

        public static Text AddLabelBold(string text, Rect rect, GameObject panel, int fontSize, Color color)
        {
            Text label = WindowManager.SpawnLabel();
            label.text = text;
            label.color = color;
            label.fontSize = fontSize;
            label.fontStyle = FontStyle.Bold;
            WindowManager.AddElementToElement(label.gameObject, panel, rect, new Rect(0, 0, 0, 0));
            return label;
        }

        public static Text AddSectionHeader(string text, Rect rect, GameObject panel)
        {
            Text header = WindowManager.SpawnLabel();
            header.text = text;
            header.color = new Color(0.2f, 0.4f, 0.2f);
            header.fontSize = 16;
            header.fontStyle = FontStyle.Bold;
            WindowManager.AddElementToElement(header.gameObject, panel, rect, new Rect(0, 0, 0, 0));
            return header;
        }

        // ========== INPUT FIELDS ==========
        
        public static InputField AddInputField(string text, Rect rect, UnityAction<string> onValueChanged, GUIWindow window)
        {
            return AddInputField(text, rect, onValueChanged, window.MainPanel);
        }

        public static InputField AddInputField(string text, Rect rect, UnityAction<string> onValueChanged, GameObject panel)
        {
            InputField input = WindowManager.SpawnInputbox();
            input.text = text;
            input.onValueChanged.AddListener(onValueChanged);
            WindowManager.AddElementToElement(input.gameObject, panel, rect, new Rect(0, 0, 0, 0));
            return input;
        }

        public static InputField AddIntField(int value, Rect rect, UnityAction<int> onValueChanged, GameObject panel)
        {
            InputField input = WindowManager.SpawnInputbox();
            input.text = value.ToString();
            input.contentType = InputField.ContentType.IntegerNumber;
            input.onValueChanged.AddListener(val => onValueChanged.Invoke(int.Parse(val)));
            WindowManager.AddElementToElement(input.gameObject, panel, rect, new Rect(0, 0, 0, 0));
            return input;
        }

        // ========== TOGGLES ==========
        
        public static Toggle AddToggle(string text, Rect rect, bool isOn, UnityAction<bool> onValueChanged, GUIWindow window)
        {
            return AddToggle(text, rect, isOn, onValueChanged, window.MainPanel);
        }

        public static Toggle AddToggle(string text, Rect rect, bool isOn, UnityAction<bool> onValueChanged, GameObject panel)
        {
            Toggle toggle = WindowManager.SpawnCheckbox();
            Text label = toggle.GetComponentInChildren<Text>();
            label.text = text;
            toggle.isOn = isOn;
            toggle.onValueChanged.AddListener(onValueChanged);
            WindowManager.AddElementToElement(toggle.gameObject, panel, rect, new Rect(0, 0, 0, 0));
            return toggle;
        }

        public static InputField AddInput(string text, Rect rect, UnityAction<string> onValueChanged, GameObject panel)
        {
            InputField input = WindowManager.SpawnInputbox();
            input.text = text;
            input.onValueChanged.AddListener(onValueChanged);
            WindowManager.AddElementToElement(input.gameObject, panel, rect, new Rect(0, 0, 0, 0));
            return input;
        }

        public static InputField AddInput(string text, Rect rect, UnityAction<string> onValueChanged, GUIWindow window)
        {
            return AddInput(text, rect, onValueChanged, window.MainPanel);
        }

        public static Scrollbar AddScrollbar(Rect rect, GameObject panel)
        {
            GameObject scrollObj = new GameObject("Scrollbar");
            scrollObj.AddComponent<RectTransform>();
            Scrollbar scrollbar = scrollObj.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            
            GameObject bgArea = new GameObject("Background Area");
            bgArea.transform.SetParent(scrollObj.transform);
            RectTransform bgRect = bgArea.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImage = bgArea.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.2f);

            GameObject handleArea = new GameObject("Sliding Area");
            handleArea.transform.SetParent(scrollObj.transform);
            RectTransform haRect = handleArea.AddComponent<RectTransform>();
            haRect.anchorMin = Vector2.zero;
            haRect.anchorMax = Vector2.one;
            haRect.sizeDelta = Vector2.zero; 

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform);
            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = Vector2.zero;
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = new Color(0.8f, 0.8f, 0.8f, 0.7f);

            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImg;
            
            WindowManager.AddElementToElement(scrollObj, panel, rect, new Rect(0,0,0,0));
            return scrollbar;
        }

        // ========== SCROLL VIEWS ==========
        
        /// <summary>
        /// Creates a scrollable area for content that exceeds the visible space
        /// </summary>
        /// <param name="rect">Position and size of the scroll view</param>
        /// <param name="parent">Parent panel or window</param>
        /// <param name="contentHeight">Total height of the content (if known, otherwise 1000)</param>
        /// <returns>The content panel where you should add scrollable elements</returns>
        public static GameObject AddScrollView(Rect rect, GameObject parent, float contentHeight = 1000f)
        {
            // Create scroll view container
            GameObject scrollViewObj = new GameObject("ScrollView");
            RectTransform scrollRect = scrollViewObj.AddComponent<RectTransform>();
            scrollViewObj.AddComponent<Image>().color = Color.clear; // FULLY TRANSPARENT - don't cover content!
            WindowManager.AddElementToElement(scrollViewObj, parent, rect, new Rect(0, 0, 0, 0));
            
            // Create viewport (masks content)
            GameObject viewportObj = new GameObject("Viewport");
            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportObj.AddComponent<Image>().color = Color.white; // Must be white so the mask alpha is 1, showMaskGraphic will hide it
            viewportObj.AddComponent<Mask>().showMaskGraphic = false;
            viewportRect.SetParent(scrollRect, false);
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;
            viewportRect.localScale = Vector3.one;
            
            // Create content panel (this is where you add your actual content)
            GameObject contentObj = new GameObject("Content");
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.SetParent(viewportRect, false);
            contentRect.anchorMin = new Vector2(0, 1);  // Top-left anchor
            contentRect.anchorMax = new Vector2(1, 1);  // Top-right anchor
            contentRect.pivot = new Vector2(0, 1);      // Pivot at top-left (NOT center!)
            contentRect.sizeDelta = new Vector2(0, contentHeight);
            contentRect.anchoredPosition = new Vector2(0, 0);  // Position at top-left
            contentRect.localScale = Vector3.one;
            
            // Add ScrollRect component
            ScrollRect scrollComponent = scrollViewObj.AddComponent<ScrollRect>();
            scrollComponent.content = contentRect;
            scrollComponent.viewport = viewportRect;
            scrollComponent.horizontal = false;
            scrollComponent.vertical = true;
            scrollComponent.movementType = ScrollRect.MovementType.Clamped;
            scrollComponent.inertia = true;
            scrollComponent.scrollSensitivity = 20f;
            
            // Create vertical scrollbar
            GameObject scrollbarObj = new GameObject("Scrollbar");
            RectTransform scrollbarRect = scrollbarObj.AddComponent<RectTransform>();
            WindowManager.AddElementToElement(scrollbarObj, scrollViewObj, 
                new Rect(rect.width - 15, 0, 15, rect.height), new Rect(0, 0, 0, 0));
            
            Scrollbar scrollbar = scrollbarObj.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            
            // Scrollbar handle
            GameObject handleObj = new GameObject("Handle");
            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleObj.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            handleRect.SetParent(scrollbarRect, false);
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.sizeDelta = Vector2.zero;
            handleRect.localScale = Vector3.one;
            
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleObj.GetComponent<Image>();
            
            // Link scrollbar to scroll view
            scrollComponent.verticalScrollbar = scrollbar;
            scrollComponent.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            
            return contentObj;
        }
        
        /// <summary>
        /// Updates the content height of an existing scroll view
        /// Useful when dynamically adding/removing content
        /// </summary>
        public static void UpdateScrollViewContentHeight(GameObject contentPanel, float newHeight)
        {
            if (contentPanel != null)
            {
                RectTransform contentRect = contentPanel.GetComponent<RectTransform>();
                if (contentRect != null)
                {
                    contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, newHeight);
                }
            }
        }

        // ========== SLIDERS ==========

        /// <summary>
        /// Adds a slider to a GUIWindow.
        /// ⚠️ CRITICAL: Parameter order is (minValue, maxValue, value) - NOT (value, minValue, maxValue)!
        /// Wrong order will cause binary slider behavior (only min/max values accessible).
        /// </summary>
        public static Slider AddSlider(float minValue, float maxValue, float value, Rect rect, UnityAction<float> onValueChanged, GUIWindow window)
        {
            return AddSlider(minValue, maxValue, value, rect, onValueChanged, window.MainPanel);
        }

        /// <summary>
        /// Adds a slider to a GameObject panel.
        /// ⚠️ CRITICAL: Parameter order is (minValue, maxValue, value) - NOT (value, minValue, maxValue)!
        /// </summary>
        public static Slider AddSlider(float minValue, float maxValue, float value, Rect rect, UnityAction<float> onValueChanged, GameObject panel)
        {
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.AddComponent<RectTransform>();
            
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = value;
            slider.onValueChanged.AddListener(onValueChanged);
            
            // Create background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObj.transform);
            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.25f);
            bgRect.anchorMax = new Vector2(1, 0.75f);
            bgRect.sizeDelta = Vector2.zero;
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            
            // Create fill area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.sizeDelta = new Vector2(-10, 0);
            
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.sizeDelta = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.6f, 1f, 1f);
            
            slider.fillRect = fillRect;
            
            // Create handle
            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObj.transform);
            RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.sizeDelta = new Vector2(-10, 0);
            handleAreaRect.anchorMin = new Vector2(0, 0);
            handleAreaRect.anchorMax = new Vector2(1, 1);
            
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform);
            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(10, 10);
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(1f, 1f, 1f, 1f);
            
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            
            WindowManager.AddElementToElement(sliderObj, panel, rect, new Rect(0, 0, 0, 0));
            return slider;
        }

        // ========== COMBOBOXES (DROPDOWNS) ==========

        /// <summary>
        /// Add a combobox/dropdown menu with string options (uses game's native GUICombobox)
        /// </summary>
        public static GUICombobox AddCombobox(string[] options, int selectedIndex, Rect rect, UnityAction onSelectedChanged, GUIWindow window)
        {
            return AddCombobox(options, selectedIndex, rect, onSelectedChanged, window.MainPanel);
        }

        /// <summary>
        /// Add a combobox/dropdown menu with string options (uses game's native GUICombobox)
        /// </summary>
        public static GUICombobox AddCombobox(string[] options, int selectedIndex, Rect rect, UnityAction onSelectedChanged, GameObject panel)
        {
            // Use game's native combobox spawner
            GUICombobox comboBox = WindowManager.SpawnComboBox();
            
            // Add options as objects (GUICombobox uses List<object>)
            comboBox.Items.Clear();
            foreach (string option in options)
            {
                comboBox.Items.Add(option);
            }
            
            // Set initial selection
            if (selectedIndex >= 0 && selectedIndex < options.Length)
            {
                comboBox.Selected = selectedIndex;
            }
            
            // Add listener
            if (onSelectedChanged != null)
            {
                comboBox.OnSelectedChanged.AddListener(onSelectedChanged);
            }
            
            // Add to parent
            WindowManager.AddElementToElement(comboBox.gameObject, panel, rect, new Rect(0, 0, 0, 0));
            
            return comboBox;
        }

        // ========== WINDOWS ==========
        
        public static GUIWindow CreateWindow(string title)
        {
            return CreateWindow(title, 800, 500);
        }

        public static GUIWindow CreateWindow(string title, float width, float height)
        {
            GUIWindow window = WindowManager.SpawnWindow();
            window.InitialTitle = window.TitleText.text = window.NonLocTitle = title;
            window.MinSize.x = width;
            window.MinSize.y = height;
            return window;
        }
    }
}
