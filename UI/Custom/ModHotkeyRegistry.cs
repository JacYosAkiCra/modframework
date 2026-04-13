using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized registry for all mod hotkeys.
/// Prevents conflicts and provides a single unified interface for managing bindings.
/// </summary>
namespace ModFramework.UI.Custom
{
    public class HotkeyEntry
    {
        public string Id;           // Internal identifier (e.g., "mymod.toggle")
        public string Label;        // Human-readable name for settings UI (e.g., "Toggle My Mod")
        public KeyCode Key;         // The currently bound key
        public Action OnPress;      // Callback to execute
        public bool Enabled = true; // Whether the hotkey is currently active
    }

    public static class ModHotkeyRegistry
    {
        private static Dictionary<string, HotkeyEntry> _entries = new Dictionary<string, HotkeyEntry>();
        private static ModHotkeyPoller _poller;

        /// <summary>
        /// Ensure the polling MonoBehaviour exists (lazy initialization).
        /// </summary>
        private static void EnsurePoller()
        {
            if (_poller != null) return;
            var obj = new GameObject("ModFramework_HotkeyPoller");
            UnityEngine.Object.DontDestroyOnLoad(obj);
            _poller = obj.AddComponent<ModHotkeyPoller>();
        }

        /// <summary>
        /// Register a hotkey. If the ID already exists, it updates the existing entry.
        /// </summary>
        public static void Register(string id, string label, KeyCode key, Action onPress)
        {
            EnsurePoller();
            _entries[id] = new HotkeyEntry
            {
                Id = id,
                Label = label,
                Key = key,
                OnPress = onPress,
                Enabled = true
            };
        }

        /// <summary>
        /// Unregister a hotkey by its ID.
        /// </summary>
        public static void Unregister(string id)
        {
            if (_entries.ContainsKey(id))
            {
                _entries.Remove(id);
            }
        }

        /// <summary>
        /// Change the key bound to a specific ID (e.g., from a settings menu).
        /// </summary>
        public static void Rebind(string id, KeyCode newKey)
        {
            if (_entries.TryGetValue(id, out HotkeyEntry entry))
            {
                entry.Key = newKey;
            }
        }

        /// <summary>
        /// Temporarily enable or disable a specific hotkey (e.g., during keybind capture).
        /// </summary>
        public static void SetEnabled(string id, bool enabled)
        {
            if (_entries.TryGetValue(id, out HotkeyEntry entry))
            {
                entry.Enabled = enabled;
            }
        }

        /// <summary>
        /// Get the currently bound key for an ID. Returns KeyCode.None if not found.
        /// </summary>
        public static KeyCode GetKey(string id)
        {
            return _entries.TryGetValue(id, out HotkeyEntry entry) ? entry.Key : KeyCode.None;
        }

        /// <summary>
        /// Check if a specific key is already used by another registered hotkey.
        /// Pass an excludeId to ignore the current binding being checked.
        /// </summary>
        public static bool HasConflict(KeyCode key, string excludeId = null)
        {
            if (key == KeyCode.None) return false;
            
            foreach (var kvp in _entries)
            {
                if (kvp.Key == excludeId) continue;
                if (kvp.Value.Key == key) return true;
            }
            return false;
        }

        /// <summary>
        /// Get a copy of all registered hotkeys (useful for building settings UI).
        /// </summary>
        public static List<HotkeyEntry> GetAll()
        {
            return new List<HotkeyEntry>(_entries.Values);
        }

        /// <summary>
        /// Called automatically by ModHotkeyPoller every frame.
        /// </summary>
        internal static void Poll()
        {
            foreach (var kvp in _entries)
            {
                var entry = kvp.Value;
                if (entry.Enabled && entry.Key != KeyCode.None && Input.GetKeyDown(entry.Key))
                {
                    try 
                    { 
                        if (entry.OnPress != null) entry.OnPress.Invoke(); 
                    }
                    catch (Exception e) 
                    { 
                        Debug.LogWarning("[ModHotkeyRegistry] Error executing hotkey '" + entry.Id + "': " + e.Message); 
                    }
                }
            }
        }
    }
}
