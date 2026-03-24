using System;
using UnityEngine;

/// <summary>
/// Safe, reliable entry points for your mod.
/// Instead of writing complex Unity Update() logic or hooking into random game methods,
/// subscribe your methods to these events.
///
/// ModFramework automatically handles the complex Unity lifecycle and ensures your
/// custom logic only runs when it is safe to do so.
///
/// All events are strictly wrapped in try/catch blocks; if your mod crashes here,
/// it will not crash the rest of the game.
/// </summary>
namespace ModFramework.Core
{
    public static class ModLifecycle
    {
        /// <summary>
        /// Fires exactly once when the mod is first loaded by the game.
        /// Use this for setting up Harmony patches, initializing your ModSettings, or loading assets.
        /// </summary>
        public static event Action OnModInitialized;

        /// <summary>
        /// Fires when the player has actually entered a simulation (after loading a save or starting a new game).
        /// Use this to spawn your UI windows, cache company data, or initialize game-specific logic.
        /// </summary>
        public static event Action OnGameReady;

        /// <summary>
        /// Fires when the player exits their save and returns to the Main Menu.
        /// Use this to clean up your UI, clear caches, or reset mod state.
        /// </summary>
        public static event Action OnGameExit;

        /// <summary>
        /// Fires at the exact moment a new day begins in the simulation.
        /// Extremely useful for daily calculations, checking budgets, or triggering daily random events.
        /// </summary>
        public static event Action OnDayPassed;

        /// <summary>
        /// Fires at the exact moment a new month begins in the simulation.
        /// Useful for monthly reports, generating rival companies, or triggering monthly events.
        /// </summary>
        public static event Action OnMonthPassed;

        /// <summary>
        /// Fires at the exact moment a new year begins in the simulation.
        /// </summary>
        public static event Action OnYearPassed;


        // ================= INTERNAL FRAMEWORK TRIGGERS =================
        // These are called by ModFramework.cs or your Main Behaviour script.
        // DO NOT call these manually from your mod logic unless you know what you're doing.

        private static bool _isInGame = false;
        private static int _lastKnownDay = -1;
        private static int _lastKnownMonth = -1;
        private static int _lastKnownYear = -1;

        internal static void TriggerModInitialized()
        {
            ModSafety.Try(() => OnModInitialized?.Invoke(), "OnModInitialized");
        }

        internal static void TriggerGameReady()
        {
            if (_isInGame) return;
            _isInGame = true;
            ModSafety.Try(() => OnGameReady?.Invoke(), "OnGameReady");
            
            // Prime the date trackers so they don't instantly fire
            try
            {
                var now = SDateTime.Now();
                _lastKnownDay = now.Day;
                _lastKnownMonth = now.Month;
                _lastKnownYear = now.Year;
            }
            catch { }
        }

        internal static void TriggerGameExit()
        {
            if (!_isInGame) return;
            _isInGame = false;
            ModSafety.Try(() => OnGameExit?.Invoke(), "OnGameExit");
        }

        /// <summary>
        /// This should be called exactly once per Unity frame (e.g., from a ModSingleton Update loop).
        /// It detects state changes and safely fires the appropriate lifecycle events.
        /// </summary>
        internal static void UpdateLifecycle()
        {
            try
            {
                bool currentlyInGame = GameData.ModCompanyHelper.IsInGame();

                // Game state transition
                if (currentlyInGame && !_isInGame)
                {
                    TriggerGameReady();
                }
                else if (!currentlyInGame && _isInGame)
                {
                    TriggerGameExit();
                }

                // Date tracking
                if (currentlyInGame && !GameData.ModMarketHelper.IsPaused())
                {
                    var now = SDateTime.Now();
                    
                    if (now.Day != _lastKnownDay && _lastKnownDay != -1)
                    {
                        ModSafety.Try(() => OnDayPassed?.Invoke(), "OnDayPassed");
                    }
                    if (now.Month != _lastKnownMonth && _lastKnownMonth != -1)
                    {
                        ModSafety.Try(() => OnMonthPassed?.Invoke(), "OnMonthPassed");
                    }
                    if (now.Year != _lastKnownYear && _lastKnownYear != -1)
                    {
                        ModSafety.Try(() => OnYearPassed?.Invoke(), "OnYearPassed");
                    }

                    _lastKnownDay = now.Day;
                    _lastKnownMonth = now.Month;
                    _lastKnownYear = now.Year;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] Error in ModLifecycle update loop: " + e.Message);
            }
        }
    }
}
