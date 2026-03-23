using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-game developer console window.
/// Displays ModLogger's buffered log entries with color-coding.
/// Toggled via configurable keybind (default F11).
/// Logger can be enabled/disabled from this window.
/// </summary>
namespace ModFramework.UI.Custom
{
    public static class ModConsoleWindow
    {
        private static ModWindow _window;
        private static GameObject _logContent;
        private static bool _autoScroll = true;
        private static bool _isDirty = true;
        private static ScrollRect _scrollRect;
        private static Text _statusLabel;
        private static Toggle _loggerToggle;
        private static Text _keybindButtonLabel;
        private static bool _isListeningForKey;
        
        // Settings keys
        private const string KeybindSettingKey = "ModConsole_Keybind";
        private const string DefaultKeybindStr = "F11";

        /// <summary>
        /// Initialize the console keybind. Call once from mod startup.
        /// </summary>
        public static void Initialize()
        {
            // Load keybind from settings
            string savedKey = ModSettings.GetString(KeybindSettingKey, DefaultKeybindStr);
            KeyCode consoleKey;
            try { consoleKey = (KeyCode)Enum.Parse(typeof(KeyCode), savedKey, true); }
            catch { consoleKey = KeyCode.F11; }

            // Register global keybind
            ModKeybind.RegisterGlobal("ModConsole_Toggle", consoleKey, Toggle);

            // Listen for new log messages
            ModEvents.Subscribe("OnLogMessage", (data) => { _isDirty = true; });
            ModEvents.Subscribe("OnLogBufferCleared", (data) => { _isDirty = true; });
        }

        /// <summary>
        /// Update the console's keybind and persist it to settings.
        /// </summary>
        public static void SetKeybind(KeyCode key)
        {
            ModSettings.SetString(KeybindSettingKey, key.ToString());
            ModKeybind.UpdateKey("ModConsole_Toggle", key);
        }

        /// <summary>
        /// Get the current console keybind.
        /// </summary>
        public static KeyCode GetKeybind()
        {
            return ModKeybind.GetKey("ModConsole_Toggle");
        }

        /// <summary>
        /// Toggle the console window.
        /// </summary>
        public static void Toggle()
        {
            if (_window != null && _window.Root != null)
            {
                _window.Close();
                _window = null;
                return;
            }

            Show();
        }

        /// <summary>
        /// Show the console window.
        /// </summary>
        public static void Show()
        {
            if (_window != null && _window.Root != null)
            {
                _window.Show();
                return;
            }

            _window = ModWindow.Create("ModConsole", 700, 400, "ModConsole_v1");
            if (_window == null) return;

            BuildUI(_window.ContentPanel);
            _isDirty = true;
        }

        private static void BuildUI(GameObject parent)
        {
            // Toolbar row
            GameObject toolbar = ModPanel.CreateHorizontal(parent);
            LayoutElement toolbarLe = toolbar.GetComponent<LayoutElement>();
            if (toolbarLe != null)
            {
                toolbarLe.preferredHeight = 30f;
                toolbarLe.flexibleHeight = 0;
            }

            // Clear button
            ModButton.Create("Clear", () =>
            {
                ModLogger.ClearBuffer();
                _isDirty = true;
            }, toolbar, 60f);

            // Auto-scroll toggle
            ModToggle.Create("Auto-scroll", _autoScroll, (val) => { _autoScroll = val; }, toolbar);

            // Logger enabled toggle
            _loggerToggle = ModToggle.Create("Logger On", ModLogger.Enabled, (val) =>
            {
                ModLogger.Enabled = val;
                UpdateStatusLabel();
            }, toolbar);

            // Change keybind button
            _keybindButtonLabel = null;
            ModButton.Create("Key: " + GetKeybind(), () =>
            {
                _isListeningForKey = true;
                if (_keybindButtonLabel != null)
                    _keybindButtonLabel.text = "Press a key...";
            }, toolbar, 120f);
            // Grab the label from the button we just created
            var lastBtn = toolbar.transform.GetChild(toolbar.transform.childCount - 1);
            if (lastBtn != null)
                _keybindButtonLabel = lastBtn.GetComponentInChildren<Text>();

            // Status label (entry count)
            _statusLabel = ModLabel.Create("", toolbar);

            // Scroll view for log entries
            _logContent = ModScrollView.Create(0, parent);
            
            // Get the ScrollRect for auto-scroll
            Transform scrollParent = _logContent.transform.parent;
            while (scrollParent != null)
            {
                _scrollRect = scrollParent.GetComponent<ScrollRect>();
                if (_scrollRect != null) break;
                scrollParent = scrollParent.parent;
            }

            // Give scroll view flexible height
            if (_logContent.transform.parent != null)
            {
                Transform scrollObj = _logContent.transform.parent;
                while (scrollObj != null)
                {
                    LayoutElement scrollLe = scrollObj.GetComponent<LayoutElement>();
                    if (scrollLe != null)
                    {
                        scrollLe.flexibleHeight = 1f;
                        break;
                    }
                    scrollObj = scrollObj.parent;
                }
            }

            // Start update loop (also handles press-to-bind key capture)
            var updater = _window.Root.AddComponent<ConsoleUpdater>();
            updater.Setup();
        }

        /// <summary>
        /// Rebuild the log display from buffer. Called when dirty.
        /// </summary>
        internal static void RebuildLogDisplay()
        {
            if (!_isDirty) return;
            if (_logContent == null) return;
            _isDirty = false;

            // Clear existing labels
            for (int i = _logContent.transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(_logContent.transform.GetChild(i).gameObject);
            }

            // Rebuild from buffer
            var entries = ModLogger.GetBuffer();
            foreach (var entry in entries)
            {
                Color color;
                string levelTag;
                switch (entry.Level)
                {
                    case LogLevel.Error:
                        color = new Color(0.9f, 0.2f, 0.2f, 1f);
                        levelTag = "[ERR]";
                        break;
                    case LogLevel.Warning:
                        color = new Color(0.7f, 0.45f, 0.0f, 1f);
                        levelTag = "[WRN]";
                        break;
                    case LogLevel.Success:
                        color = new Color(0.1f, 0.5f, 0.1f, 1f);
                        levelTag = "[OK]";
                        break;
                    default:
                        color = GameTheme.LabelColor;
                        levelTag = "[INF]";
                        break;
                }

                string line = string.Format("[{0}] {1} {2} {3}", entry.Timestamp, levelTag, entry.Prefix, entry.Message);
                Text label = ModLabel.Create(line, _logContent);
                label.color = color;
                label.fontSize = GameTheme.DefaultFontSize;
            }

            UpdateStatusLabel();

            // Auto-scroll to bottom
            if (_autoScroll && _scrollRect != null)
            {
                // Force layout rebuild then scroll
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_logContent.GetComponent<RectTransform>());
                _scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private static void UpdateStatusLabel()
        {
            if (_statusLabel != null)
            {
                string status = string.Format("{0} entries | Logger: {1}", 
                    ModLogger.BufferCount, 
                    ModLogger.Enabled ? "ON" : "OFF");
                _statusLabel.text = status;
            }
        }

        /// <summary>
        /// Process press-to-bind for console keybind. Called from ConsoleUpdater.Update().
        /// </summary>
        internal static void ProcessKeybindCapture()
        {
            if (!_isListeningForKey) return;

            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (key == KeyCode.None) continue;
                if (key == KeyCode.Escape)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        _isListeningForKey = false;
                        if (_keybindButtonLabel != null)
                            _keybindButtonLabel.text = "Key: " + GetKeybind();
                        return;
                    }
                    continue;
                }

                if (Input.GetKeyDown(key))
                {
                    SetKeybind(key);
                    _isListeningForKey = false;
                    if (_keybindButtonLabel != null)
                        _keybindButtonLabel.text = "Key: " + key;
                    Notifications.ShowSuccess("Console hotkey set to " + key);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// MonoBehaviour that calls RebuildLogDisplay in LateUpdate (dirty flag pattern).
    /// </summary>
    internal class ConsoleUpdater : MonoBehaviour
    {
        public void Setup() { }

        void Update()
        {
            ModConsoleWindow.ProcessKeybindCapture();
        }

        void LateUpdate()
        {
            ModConsoleWindow.RebuildLogDisplay();
        }
    }
}
