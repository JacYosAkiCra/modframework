using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Centralized keyboard shortcut manager.
/// Handles global hotkeys, window-scoped hotkeys, modifier keys,
/// input field suppression, and built-in Escape-to-close.
/// Call ProcessInput() from a MonoBehaviour.Update().
/// </summary>
namespace ModFramework.UI.Custom
{
    public enum KeybindScope
    {
        Global,   // Fires regardless of window focus
        Window    // Fires only when the associated window is focused
    }

    public class KeybindEntry
    {
        public string Id;
        public KeyCode Key;
        public bool RequireCtrl;
        public bool RequireShift;
        public Action Callback;
        public KeybindScope Scope;
        public ModWindow OwnerWindow; // Only used for Window scope
    }

    public static class ModKeybind
    {
        private static readonly Dictionary<string, KeybindEntry> _bindings = new Dictionary<string, KeybindEntry>();
        private static bool _initialized;

        /// <summary>
        /// Register a global hotkey (fires even when no ModWindow is focused).
        /// </summary>
        public static void RegisterGlobal(string id, KeyCode key, Action callback, bool ctrl = false, bool shift = false)
        {
            if (string.IsNullOrEmpty(id) || callback == null) return;

            _bindings[id] = new KeybindEntry
            {
                Id = id,
                Key = key,
                RequireCtrl = ctrl,
                RequireShift = shift,
                Callback = callback,
                Scope = KeybindScope.Global,
                OwnerWindow = null
            };
        }

        /// <summary>
        /// Register a window-scoped hotkey (only fires when that window is focused).
        /// </summary>
        public static void RegisterWindow(string id, KeyCode key, Action callback, ModWindow window, bool ctrl = false, bool shift = false)
        {
            if (string.IsNullOrEmpty(id) || callback == null || window == null) return;

            _bindings[id] = new KeybindEntry
            {
                Id = id,
                Key = key,
                RequireCtrl = ctrl,
                RequireShift = shift,
                Callback = callback,
                Scope = KeybindScope.Window,
                OwnerWindow = window
            };
        }

        /// <summary>
        /// Unregister a keybinding by ID.
        /// </summary>
        public static void Unregister(string id)
        {
            if (!string.IsNullOrEmpty(id))
                _bindings.Remove(id);
        }

        /// <summary>
        /// Update the key for an existing binding (e.g., when user rebinds in settings).
        /// </summary>
        public static void UpdateKey(string id, KeyCode newKey)
        {
            KeybindEntry entry;
            if (_bindings.TryGetValue(id, out entry))
            {
                entry.Key = newKey;
            }
        }

        /// <summary>
        /// Get the current key for a binding, or KeyCode.None if not found.
        /// </summary>
        public static KeyCode GetKey(string id)
        {
            KeybindEntry entry;
            if (_bindings.TryGetValue(id, out entry))
                return entry.Key;
            return KeyCode.None;
        }

        /// <summary>
        /// Check if an input field is currently focused (typing in progress).
        /// When focused, single-key hotkeys are suppressed, but Ctrl+ combos still work.
        /// </summary>
        private static bool IsInputFieldActive()
        {
            var current = EventSystem.current;
            if (current == null) return false;
            var selected = current.currentSelectedGameObject;
            if (selected == null) return false;
            // Check for any InputField component on the selected object
            return selected.GetComponent<UnityEngine.UI.InputField>() != null;
        }

        /// <summary>
        /// Process all registered keybindings. Call this from MonoBehaviour.Update().
        /// </summary>
        public static void ProcessInput()
        {
            EnsureInitialized();

            bool inputActive = IsInputFieldActive();
            bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            // Process all bindings
            // Use a snapshot to avoid modification during iteration
            var entries = new List<KeybindEntry>(_bindings.Values);

            foreach (var entry in entries)
            {
                if (!Input.GetKeyDown(entry.Key)) continue;

                // Check modifiers
                if (entry.RequireCtrl && !ctrlHeld) continue;
                if (entry.RequireShift && !shiftHeld) continue;

                // Input field guard: suppress single-key hotkeys (no Ctrl/Shift),
                // but allow Ctrl+ combos to pass through
                if (inputActive && !entry.RequireCtrl && !entry.RequireShift) continue;

                // Scope check
                if (entry.Scope == KeybindScope.Window)
                {
                    if (entry.OwnerWindow == null || entry.OwnerWindow.Root == null) continue;
                    if (ModWindowRegistry.Focused != entry.OwnerWindow) continue;
                }

                // Fire the callback
                try
                {
                    entry.Callback.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError("[ModKeybind] Error in keybind '" + entry.Id + "': " + e.Message);
                }
            }
        }

        /// <summary>
        /// Register built-in keybindings (Escape to close topmost window).
        /// </summary>
        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            RegisterGlobal("__Escape_Close", KeyCode.Escape, () =>
            {
                var focused = ModWindowRegistry.Focused;
                if (focused != null && focused.Root != null)
                {
                    focused.Close();
                }
            });
        }

        /// <summary>
        /// Clear all registered bindings (e.g., on scene unload).
        /// </summary>
        public static void ClearAll()
        {
            _bindings.Clear();
            _initialized = false;
        }
    }
}
