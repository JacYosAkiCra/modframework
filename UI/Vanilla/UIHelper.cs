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

        // ========== SETTINGS HELPERS ==========
        // High-level methods for mod settings screens (ConstructOptionsScreen).
        // Each method creates a label + widget + status feedback, saves via ModSettings,
        // and returns the updated yOffset for vertical layout.

        /// <summary>
        /// Adds a labeled slider setting that auto-persists via ModSettings.
        /// Best for small ranges (e.g. 0-100%) where a slider feels natural.
        /// Returns the updated yOffset for the next element.
        /// </summary>
        /// <param name="parent">The parent RectTransform (from ConstructOptionsScreen)</param>
        /// <param name="yOffset">Current vertical offset</param>
        /// <param name="displayName">Label text shown to the user</param>
        /// <param name="settingsKey">ModSettings key to read/write</param>
        /// <param name="defaultValue">Default value if setting doesn't exist</param>
        /// <param name="min">Minimum slider value</param>
        /// <param name="max">Maximum slider value</param>
        /// <param name="wholeNumbers">If true, slider snaps to integers</param>
        /// <param name="formatLabel">Function to format the label text from the current value. 
        /// If null, displays "{displayName}: {value}"</param>
        public static float AddSettingSlider(
            RectTransform parent, float yOffset,
            string displayName, string settingsKey, float defaultValue,
            float min, float max, bool wholeNumbers,
            System.Func<float, string> formatLabel = null)
        {
            float currentValue = wholeNumbers
                ? ModSettings.GetInt(settingsKey, (int)defaultValue)
                : ModSettings.GetFloat(settingsKey, defaultValue);

            // Label
            Text label = WindowManager.SpawnLabel();
            label.text = formatLabel != null
                ? formatLabel(currentValue)
                : $"{displayName}: {(wholeNumbers ? ((int)currentValue).ToString() : currentValue.ToString("F2"))}";
            label.color = Color.black;
            label.fontSize = 13;
            WindowManager.AddElementToElement(label.gameObject, parent.gameObject,
                new Rect(0, yOffset, 350, 25), new Rect(0.01f, 0.01f, 0, 0));
            yOffset += 25f;

            // Slider
            Slider slider = WindowManager.SpawnSlider();
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = wholeNumbers;
            slider.value = currentValue;
            slider.onValueChanged.AddListener(val =>
            {
                if (wholeNumbers)
                    ModSettings.SetInt(settingsKey, Mathf.RoundToInt(val));
                else
                    ModSettings.SetFloat(settingsKey, val);

                label.text = formatLabel != null
                    ? formatLabel(val)
                    : $"{displayName}: {(wholeNumbers ? Mathf.RoundToInt(val).ToString() : val.ToString("F2"))}";
            });
            WindowManager.AddElementToElement(slider.gameObject, parent.gameObject,
                new Rect(0, yOffset, 300, 20), new Rect(0.01f, 0.01f, 0, 0));
            yOffset += 35f;

            return yOffset;
        }

        /// <summary>
        /// Adds a labeled text input setting that auto-persists via ModSettings.
        /// Best for wide numeric ranges (e.g. 1-999) where sliders are too sensitive.
        /// The value is validated, clamped, and saved on end-edit (Enter key or click away).
        /// Returns the updated yOffset for the next element.
        /// </summary>
        /// <param name="parent">The parent RectTransform (from ConstructOptionsScreen)</param>
        /// <param name="yOffset">Current vertical offset</param>
        /// <param name="displayName">Label text shown to the user</param>
        /// <param name="settingsKey">ModSettings key to read/write</param>
        /// <param name="defaultValue">Default value if setting doesn't exist</param>
        /// <param name="min">Minimum accepted value</param>
        /// <param name="max">Maximum accepted value</param>
        /// <param name="wholeNumbers">If true, values are rounded to integers</param>
        /// <param name="suffix">Optional suffix for the status label (e.g. "x" for multipliers)</param>
        public static float AddSettingInput(
            RectTransform parent, float yOffset,
            string displayName, string settingsKey, float defaultValue,
            float min, float max, bool wholeNumbers, string suffix = "")
        {
            float currentValue = wholeNumbers
                ? ModSettings.GetInt(settingsKey, (int)defaultValue)
                : ModSettings.GetFloat(settingsKey, defaultValue);

            // Label with range hint
            Text label = WindowManager.SpawnLabel();
            string rangeHint = wholeNumbers
                ? $"{(int)min} - {(int)max}"
                : $"{min:F1} - {max:F1}";
            label.text = $"{displayName} ({rangeHint}):";
            label.color = Color.black;
            label.fontSize = 13;
            WindowManager.AddElementToElement(label.gameObject, parent.gameObject,
                new Rect(0, yOffset, 350, 25), new Rect(0.01f, 0.01f, 0, 0));
            yOffset += 25f;

            // Status label (shows current value or error)
            Text statusLabel = WindowManager.SpawnLabel();
            statusLabel.text = FormatSettingStatus(currentValue, wholeNumbers, suffix);
            statusLabel.color = new Color(0.2f, 0.5f, 0.2f); // dark green = valid
            statusLabel.fontSize = 12;
            WindowManager.AddElementToElement(statusLabel.gameObject, parent.gameObject,
                new Rect(200, yOffset, 180, 22), new Rect(0.01f, 0.01f, 0, 0));

            // Text input field
            InputField input = WindowManager.SpawnInputbox();
            input.contentType = wholeNumbers
                ? InputField.ContentType.IntegerNumber
                : InputField.ContentType.DecimalNumber;
            // Ensure text is visible on light backgrounds
            if (input.textComponent != null)
                input.textComponent.color = Color.black;
            input.text = wholeNumbers
                ? ((int)currentValue).ToString()
                : currentValue.ToString("F2");

            // Validate, clamp, and save on end-edit
            input.onEndEdit.AddListener(text =>
            {
                float parsed;
                if (float.TryParse(text, out parsed))
                {
                    parsed = Mathf.Clamp(parsed, min, max);
                    if (wholeNumbers)
                    {
                        int intVal = Mathf.RoundToInt(parsed);
                        ModSettings.SetInt(settingsKey, intVal);
                        input.text = intVal.ToString();
                        statusLabel.text = FormatSettingStatus(intVal, true, suffix);
                    }
                    else
                    {
                        ModSettings.SetFloat(settingsKey, parsed);
                        input.text = parsed.ToString("F2");
                        statusLabel.text = FormatSettingStatus(parsed, false, suffix);
                    }
                    statusLabel.color = new Color(0.2f, 0.5f, 0.2f);
                }
                else
                {
                    statusLabel.text = "Invalid number";
                    statusLabel.color = Color.red;
                }
            });

            WindowManager.AddElementToElement(input.gameObject, parent.gameObject,
                new Rect(0, yOffset, 190, 22), new Rect(0.01f, 0.01f, 0, 0));
            yOffset += 32f;

            return yOffset;
        }

        private static string FormatSettingStatus(float value, bool wholeNumbers, string suffix)
        {
            string formatted = wholeNumbers ? ((int)value).ToString() : value.ToString("F2");
            return $"Current: {formatted}{suffix}";
        }

        // ========== SCOPED SETTINGS HELPERS (RECOMMENDED) ==========
        // These overloads accept a ModSettingsScope, making them safe for multi-mod use.

        /// <summary>
        /// Scoped version of AddSettingSlider. Uses ModSettingsScope instead of global prefix.
        /// </summary>
        public static float AddSettingSlider(
            RectTransform parent, float yOffset,
            string displayName, string settingsKey, float defaultValue,
            float min, float max, bool wholeNumbers,
            ModSettingsScope scope,
            System.Func<float, string> formatLabel = null)
        {
            float currentValue = wholeNumbers
                ? scope.GetInt(settingsKey, (int)defaultValue)
                : scope.GetFloat(settingsKey, defaultValue);

            Text label = WindowManager.SpawnLabel();
            label.text = formatLabel != null
                ? formatLabel(currentValue)
                : $"{displayName}: {(wholeNumbers ? ((int)currentValue).ToString() : currentValue.ToString("F2"))}";
            label.color = Color.black;
            label.fontSize = 13;
            WindowManager.AddElementToElement(label.gameObject, parent.gameObject,
                new Rect(0, yOffset, 350, 25), new Rect(0.01f, 0.01f, 0, 0));
            yOffset += 25f;

            Slider slider = WindowManager.SpawnSlider();
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = wholeNumbers;
            slider.value = currentValue;
            slider.onValueChanged.AddListener(val =>
            {
                if (wholeNumbers)
                    scope.SetInt(settingsKey, Mathf.RoundToInt(val));
                else
                    scope.SetFloat(settingsKey, val);

                label.text = formatLabel != null
                    ? formatLabel(val)
                    : $"{displayName}: {(wholeNumbers ? Mathf.RoundToInt(val).ToString() : val.ToString("F2"))}";
            });
            WindowManager.AddElementToElement(slider.gameObject, parent.gameObject,
                new Rect(0, yOffset, 300, 20), new Rect(0.01f, 0.01f, 0, 0));
            yOffset += 35f;

            return yOffset;
        }

        /// <summary>
        /// Scoped version of AddSettingInput. Uses ModSettingsScope instead of global prefix.
        /// </summary>
        public static float AddSettingInput(
            RectTransform parent, float yOffset,
            string displayName, string settingsKey, float defaultValue,
            float min, float max, bool wholeNumbers,
            ModSettingsScope scope, string suffix = "")
        {
            float currentValue = wholeNumbers
                ? scope.GetInt(settingsKey, (int)defaultValue)
                : scope.GetFloat(settingsKey, defaultValue);

            Text label = WindowManager.SpawnLabel();
            string rangeHint = wholeNumbers
                ? $"{(int)min} - {(int)max}"
                : $"{min:F1} - {max:F1}";
            label.text = $"{displayName} ({rangeHint}):";
            label.color = Color.black;
            label.fontSize = 13;
            WindowManager.AddElementToElement(label.gameObject, parent.gameObject,
                new Rect(0, yOffset, 350, 25), new Rect(0.01f, 0.01f, 0, 0));
            yOffset += 25f;

            Text statusLabel = WindowManager.SpawnLabel();
            statusLabel.text = FormatSettingStatus(currentValue, wholeNumbers, suffix);
            statusLabel.color = new Color(0.2f, 0.5f, 0.2f);
            statusLabel.fontSize = 12;
            WindowManager.AddElementToElement(statusLabel.gameObject, parent.gameObject,
                new Rect(200, yOffset, 180, 30), new Rect(0.01f, 0.01f, 0, 0));

            InputField input = WindowManager.SpawnInputbox();
            input.contentType = wholeNumbers
                ? InputField.ContentType.IntegerNumber
                : InputField.ContentType.DecimalNumber;

            // Parent the input FIRST so its RectTransform is properly sized
            WindowManager.AddElementToElement(input.gameObject, parent.gameObject,
                new Rect(0, yOffset, 190, 30), new Rect(0.01f, 0.01f, 0, 0));

            // Force internal text component RectTransform to fill the input field
            if (input.textComponent != null)
            {
                RectTransform textRect = input.textComponent.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = new Vector2(5, 2);
                    textRect.offsetMax = new Vector2(-5, -6);
                }
                input.textComponent.color = Color.black;
                input.textComponent.fontSize = 13;
            }

            // Fix placeholder RectTransform too
            if (input.placeholder != null)
            {
                RectTransform phRect = input.placeholder.GetComponent<RectTransform>();
                if (phRect != null)
                {
                    phRect.anchorMin = Vector2.zero;
                    phRect.anchorMax = Vector2.one;
                    phRect.offsetMin = new Vector2(5, 2);
                    phRect.offsetMax = new Vector2(-5, -6);
                }
            }

            // Set text AFTER parenting and layout fix so it renders correctly
            input.text = wholeNumbers
                ? ((int)currentValue).ToString()
                : currentValue.ToString("F2");
            input.ForceLabelUpdate();

            input.onEndEdit.AddListener(text =>
            {
                float parsed;
                if (float.TryParse(text, out parsed))
                {
                    parsed = Mathf.Clamp(parsed, min, max);
                    if (wholeNumbers)
                    {
                        int intVal = Mathf.RoundToInt(parsed);
                        scope.SetInt(settingsKey, intVal);
                        input.text = intVal.ToString();
                        statusLabel.text = FormatSettingStatus(intVal, true, suffix);
                    }
                    else
                    {
                        scope.SetFloat(settingsKey, parsed);
                        input.text = parsed.ToString("F2");
                        statusLabel.text = FormatSettingStatus(parsed, false, suffix);
                    }
                    statusLabel.color = new Color(0.2f, 0.5f, 0.2f);
                }
                else
                {
                    statusLabel.text = "Invalid number";
                    statusLabel.color = Color.red;
                }
            });

            yOffset += 38f;

            return yOffset;
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
