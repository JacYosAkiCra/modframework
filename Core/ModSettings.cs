using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Simple persistent mod settings store.
///
/// 2026-03: Software Inc security update blocks mods that reference UnityEngine's built-in prefs API.
/// This implementation persists settings to disk under Application.persistentDataPath instead.
///
/// IMPORTANT: The static methods on this class use a global prefix that is shared across all mods.
/// When multiple DLL mods are loaded, they can stomp on each other's prefix.
/// For Harmony patches and any code that runs outside of ConstructOptionsScreen,
/// use the scoped API instead:
///
///   private static readonly ModSettingsScope Settings = ModSettings.ForMod("MyMod");
///   float val = Settings.GetFloat("MyKey", 1.0f);
///
/// The scoped API is always safe to use regardless of which mod set the global prefix last.
/// </summary>
namespace ModFramework
{
    // ========== SCOPED SETTINGS (RECOMMENDED) ==========

    /// <summary>
    /// Mod-scoped settings accessor. Each instance carries its own prefix,
    /// so it is safe to use in Harmony patches when multiple mods are loaded.
    ///
    /// Create one via ModSettings.ForMod("MyModName") and store it as a static field.
    /// </summary>
    public sealed class ModSettingsScope
    {
        private readonly string _prefix;

        internal ModSettingsScope(string prefix)
        {
            _prefix = string.IsNullOrWhiteSpace(prefix) ? "Mod" : prefix.Trim();
        }

        public void SetBool(string key, bool value) { ModSettings.ScopedSetString(_prefix, key, value ? "1" : "0"); }
        public bool GetBool(string key, bool defaultValue = false)
        {
            var s = ModSettings.ScopedGetString(_prefix, key, null);
            if (string.IsNullOrEmpty(s)) return defaultValue;
            if (s == "1") return true;
            if (s == "0") return false;
            bool b;
            return bool.TryParse(s, out b) ? b : defaultValue;
        }

        public void SetInt(string key, int value) { ModSettings.ScopedSetString(_prefix, key, value.ToString(CultureInfo.InvariantCulture)); }
        public int GetInt(string key, int defaultValue = 0)
        {
            var s = ModSettings.ScopedGetString(_prefix, key, null);
            int v;
            return (s != null && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v)) ? v : defaultValue;
        }

        public void SetFloat(string key, float value) { ModSettings.ScopedSetString(_prefix, key, value.ToString(CultureInfo.InvariantCulture)); }
        public float GetFloat(string key, float defaultValue = 0f)
        {
            var s = ModSettings.ScopedGetString(_prefix, key, null);
            float v;
            return (s != null && float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v)) ? v : defaultValue;
        }

        public void SetString(string key, string value) { ModSettings.ScopedSetString(_prefix, key, value); }
        public string GetString(string key, string defaultValue = "")
        {
            return ModSettings.ScopedGetString(_prefix, key, defaultValue);
        }
    }

    // ========== STATIC SETTINGS (BACKWARD COMPAT) ==========

    public static class ModSettings
    {
        private static readonly object _gate = new object();
        private static string _prefix = "Mod";
        private static Dictionary<string, string> _values;
        private static bool _loaded;

        /// <summary>
        /// Creates a mod-scoped settings accessor that does NOT depend on the global prefix.
        /// This is the recommended way to read/write settings from Harmony patches and
        /// any code that may run while multiple mods are loaded.
        ///
        /// Usage:
        ///   private static readonly ModSettingsScope Settings = ModSettings.ForMod("MyMod");
        ///   float val = Settings.GetFloat("MyKey", 1.0f);
        /// </summary>
        public static ModSettingsScope ForMod(string modName)
        {
            return new ModSettingsScope(modName);
        }

        // --- Global prefix (legacy) ---

        public static void SetPrefix(string modName)
        {
            lock (_gate)
            {
                _prefix = string.IsNullOrWhiteSpace(modName) ? "Mod" : modName.Trim();
                _loaded = false;
                _values = null;
            }
        }

        public static void SetBool(string key, bool value) { SetString(key, value ? "1" : "0"); }
        public static bool GetBool(string key) { return GetBool(key, false); }
        public static bool GetBool(string key, bool defaultValue)
        {
            var s = GetString(key, null);
            if (string.IsNullOrEmpty(s)) return defaultValue;
            if (s == "1") return true;
            if (s == "0") return false;
            bool b;
            return bool.TryParse(s, out b) ? b : defaultValue;
        }

        public static void SetInt(string key, int value) { SetString(key, value.ToString(CultureInfo.InvariantCulture)); }
        public static int GetInt(string key) { return GetInt(key, 0); }
        public static int GetInt(string key, int defaultValue)
        {
            var s = GetString(key, null);
            int v;
            return (s != null && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v)) ? v : defaultValue;
        }

        public static void SetFloat(string key, float value) { SetString(key, value.ToString(CultureInfo.InvariantCulture)); }
        public static float GetFloat(string key) { return GetFloat(key, 0f); }
        public static float GetFloat(string key, float defaultValue)
        {
            var s = GetString(key, null);
            float v;
            return (s != null && float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v)) ? v : defaultValue;
        }

        public static void SetString(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;
            lock (_gate)
            {
                EnsureLoaded();
                _values[KeyWithPrefix(key)] = value ?? "";
                Persist();
            }
        }

        public static string GetString(string key) { return GetString(key, ""); }
        public static string GetString(string key, string defaultValue)
        {
            if (string.IsNullOrEmpty(key)) return defaultValue;
            lock (_gate)
            {
                EnsureLoaded();
                string v;
                return _values.TryGetValue(KeyWithPrefix(key), out v) ? v : defaultValue;
            }
        }

        public static void DeleteAll()
        {
            lock (_gate)
            {
                _values = new Dictionary<string, string>(StringComparer.Ordinal);
                _loaded = true;
                TryDeleteFile();
            }
        }

        // --- Internal scoped methods (used by ModSettingsScope) ---

        internal static void ScopedSetString(string prefix, string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;
            lock (_gate)
            {
                // Temporarily swap prefix to load the correct file,
                // then use the scoped key directly
                string savedPrefix = _prefix;
                if (_prefix != prefix)
                {
                    _prefix = prefix;
                    _loaded = false;
                    _values = null;
                }
                EnsureLoaded();
                _values[prefix + "_" + key] = value ?? "";
                Persist();
                if (_prefix != savedPrefix)
                {
                    _prefix = savedPrefix;
                    _loaded = false;
                    _values = null;
                }
            }
        }

        internal static string ScopedGetString(string prefix, string key, string defaultValue)
        {
            if (string.IsNullOrEmpty(key)) return defaultValue;
            lock (_gate)
            {
                string savedPrefix = _prefix;
                if (_prefix != prefix)
                {
                    _prefix = prefix;
                    _loaded = false;
                    _values = null;
                }
                EnsureLoaded();
                string v;
                var result = _values.TryGetValue(prefix + "_" + key, out v) ? v : defaultValue;
                if (_prefix != savedPrefix)
                {
                    _prefix = savedPrefix;
                    _loaded = false;
                    _values = null;
                }
                return result;
            }
        }

        // --- Private helpers ---

        private static string KeyWithPrefix(string key) { return _prefix + "_" + key; }

        private static void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            _values = new Dictionary<string, string>(StringComparer.Ordinal);

            try
            {
                var path = GetFilePath();
                if (!File.Exists(path)) return;

                var lines = File.ReadAllLines(path, Encoding.UTF8);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    int tab = line.IndexOf('\t');
                    if (tab <= 0) continue;

                    var k = line.Substring(0, tab);
                    var v64 = line.Substring(tab + 1);
                    if (string.IsNullOrEmpty(k)) continue;

                    string v;
                    try
                    {
                        var bytes = Convert.FromBase64String(v64);
                        v = Encoding.UTF8.GetString(bytes);
                    }
                    catch
                    {
                        v = "";
                    }

                    _values[k] = v;
                }
            }
            catch
            {
                // If settings can't be read, fall back to defaults.
                _values = new Dictionary<string, string>(StringComparer.Ordinal);
            }
        }

        private static void Persist()
        {
            try
            {
                var path = GetFilePath();
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                var tmp = path + ".tmp";
                using (var sw = new StreamWriter(tmp, false, Encoding.UTF8))
                {
                    foreach (var kv in _values)
                    {
                        var v64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(kv.Value ?? ""));
                        sw.Write(kv.Key);
                        sw.Write('\t');
                        sw.WriteLine(v64);
                    }
                }

                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
            }
            catch
            {
                // Ignore persistence failures; settings will behave like defaults.
            }
        }

        private static void TryDeleteFile()
        {
            try
            {
                var path = GetFilePath();
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // ignore
            }
        }

        private static string GetFilePath()
        {
            var safe = SanitizeFileName(_prefix);
            return Path.Combine(Application.persistentDataPath, "ModSettings", safe + ".txt");
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Mod";
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(name.Length);
            for (int i = 0; i < name.Length; i++)
            {
                var c = name[i];
                bool bad = false;
                for (int j = 0; j < invalid.Length; j++)
                {
                    if (c == invalid[j]) { bad = true; break; }
                }
                sb.Append(bad ? '_' : c);
            }
            return sb.ToString();
        }
    }
}
