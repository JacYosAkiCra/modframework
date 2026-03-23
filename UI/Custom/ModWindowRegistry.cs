using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized registry for all ModWindows.
/// Handles: z-ordering, focus tracking, singleton enforcement, and hotkey management.
/// </summary>
namespace ModFramework.UI.Custom
{
    public static class ModWindowRegistry
    {
        private static readonly List<ModWindow> _windows = new List<ModWindow>();
        private static ModWindow _focused;
        private static readonly Dictionary<string, ModWindow> _singletons = new Dictionary<string, ModWindow>();

        /// <summary>
        /// Register a window. Called automatically by ModWindow.Show().
        /// </summary>
        public static void Register(ModWindow window)
        {
            if (window == null || _windows.Contains(window)) return;
            _windows.Add(window);
        }

        /// <summary>
        /// Unregister a window. Called automatically by ModWindow.Close().
        /// </summary>
        public static void Unregister(ModWindow window)
        {
            if (window == null) return;
            _windows.Remove(window);
            if (_focused == window)
                _focused = _windows.Count > 0 ? _windows[_windows.Count - 1] : null;

            // Clean up singleton reference
            string key = null;
            foreach (var kvp in _singletons)
            {
                if (kvp.Value == window) { key = kvp.Key; break; }
            }
            if (key != null) _singletons.Remove(key);
        }

        /// <summary>
        /// Set a window as focused (brought to front).
        /// Called by FocusTracker on click.
        /// </summary>
        public static void SetFocused(ModWindow window)
        {
            _focused = window;
        }

        /// <summary>
        /// Get the currently focused window, or null.
        /// </summary>
        public static ModWindow Focused { get { return _focused; } }

        /// <summary>
        /// Get all currently open windows (read-only copy).
        /// </summary>
        public static List<ModWindow> GetAll()
        {
            return new List<ModWindow>(_windows);
        }

        /// <summary>
        /// Number of open windows.
        /// </summary>
        public static int Count { get { return _windows.Count; } }

        // --- Singleton Support ---

        /// <summary>
        /// Register a singleton window. If one already exists with this key,
        /// it brings the existing one to front instead of creating a new one.
        /// Returns true if the caller should create a new window, false if an existing one was brought forward.
        /// </summary>
        public static bool TryRegisterSingleton(string key, ModWindow window)
        {
            if (string.IsNullOrEmpty(key)) return true;

            ModWindow existing;
            if (_singletons.TryGetValue(key, out existing) && existing != null && existing.Root != null)
            {
                // Already exists — bring to front
                existing.Show();
                return false;
            }

            _singletons[key] = window;
            return true;
        }

        /// <summary>
        /// Check if a singleton window with this key already exists.
        /// </summary>
        public static bool HasSingleton(string key)
        {
            ModWindow existing;
            return _singletons.TryGetValue(key, out existing) && existing != null && existing.Root != null;
        }

        /// <summary>
        /// Get a singleton window by key, or null.
        /// </summary>
        public static ModWindow GetSingleton(string key)
        {
            ModWindow existing;
            if (_singletons.TryGetValue(key, out existing) && existing != null && existing.Root != null)
                return existing;
            return null;
        }

        /// <summary>
        /// Close all open windows (e.g., on scene unload).
        /// </summary>
        public static void CloseAll()
        {
            var copy = new List<ModWindow>(_windows);
            foreach (var w in copy)
            {
                try { w.Close(); } catch { }
            }
            _windows.Clear();
            _singletons.Clear();
            _focused = null;
        }
    }
}
