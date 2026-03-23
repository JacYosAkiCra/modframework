using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enhanced logging system with filtering, formatting, and in-game console buffer.
/// All log messages are buffered for the ModConsoleWindow.
/// Logging can be enabled/disabled at runtime via settings.
/// </summary>
namespace ModFramework
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Success
    }

    public struct LogEntry
    {
        public string Message;
        public LogLevel Level;
        public string Timestamp;
        public string Prefix;
    }

    public static class ModLogger
    {
        private static string modPrefix = "[Mod]";
        
        // Ring buffer for console display
        private static readonly Queue<LogEntry> _buffer = new Queue<LogEntry>();
        private const int MaxBufferSize = 500;
        
        // Enable/disable logging (persisted in settings)
        private static bool _enabled = true;
        private static bool _settingsLoaded;

        public static void SetPrefix(string prefix)
        {
            modPrefix = "[" + prefix + "]";
        }

        /// <summary>
        /// Enable or disable logging. Persisted to ModSettings.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                EnsureSettingsLoaded();
                return _enabled;
            }
            set
            {
                _enabled = value;
                ModSettings.SetBool("ModLogger_Enabled", value);
            }
        }

        public static void Log(string message)
        {
            if (!Enabled) return;
            var entry = CreateEntry(message, LogLevel.Info);
            BufferEntry(entry);
            Debug.Log(string.Format("{0} {1}", modPrefix, message));
        }

        public static void LogWarning(string message)
        {
            if (!Enabled) return;
            var entry = CreateEntry(message, LogLevel.Warning);
            BufferEntry(entry);
            Debug.LogWarning(string.Format("{0} {1}", modPrefix, message));
        }

        public static void LogError(string message)
        {
            // Errors always log even when disabled
            var entry = CreateEntry(message, LogLevel.Error);
            BufferEntry(entry);
            Debug.LogError(string.Format("{0} {1}", modPrefix, message));
        }

        public static void LogSuccess(string message)
        {
            if (!Enabled) return;
            var entry = CreateEntry(message, LogLevel.Success);
            BufferEntry(entry);
            Debug.Log(string.Format("{0} ✓ {1}", modPrefix, message));
        }

        public static void LogSeparator()
        {
            if (!Enabled) return;
            var entry = CreateEntry("========================", LogLevel.Info);
            BufferEntry(entry);
            Debug.Log(modPrefix + " ========================");
        }

        public static void LogSection(string sectionName)
        {
            if (!Enabled) return;
            string msg = string.Format("===== {0} =====", sectionName);
            var entry = CreateEntry(msg, LogLevel.Info);
            BufferEntry(entry);
            Debug.Log(string.Format("{0} ===== {1} =====", modPrefix, sectionName));
        }

        /// <summary>
        /// Get all buffered log entries (for console display).
        /// </summary>
        public static List<LogEntry> GetBuffer()
        {
            return new List<LogEntry>(_buffer);
        }

        /// <summary>
        /// Clear all buffered log entries.
        /// </summary>
        public static void ClearBuffer()
        {
            _buffer.Clear();
            ModEvents.Trigger("OnLogBufferCleared");
        }

        /// <summary>
        /// Number of entries in the buffer.
        /// </summary>
        public static int BufferCount { get { return _buffer.Count; } }

        private static LogEntry CreateEntry(string message, LogLevel level)
        {
            return new LogEntry
            {
                Message = message,
                Level = level,
                Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                Prefix = modPrefix
            };
        }

        private static void BufferEntry(LogEntry entry)
        {
            while (_buffer.Count >= MaxBufferSize)
                _buffer.Dequeue();
            
            _buffer.Enqueue(entry);
            ModEvents.Trigger("OnLogMessage", entry);
        }

        private static void EnsureSettingsLoaded()
        {
            if (_settingsLoaded) return;
            _settingsLoaded = true;
            _enabled = ModSettings.GetBool("ModLogger_Enabled", true);
        }
    }
}
