using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Safe, read-only helper methods for accessing market and game-state data.
/// All methods include null-checks and error handling so they never crash your mod.
///
/// Usage:
///   SDateTime now = ModMarketHelper.GetCurrentDate();
///   int speed = ModMarketHelper.GetGameSpeed();
///   var types = ModMarketHelper.GetSoftwareTypes();
/// </summary>
namespace ModFramework.GameData
{
    public static class ModMarketHelper
    {
        /// <summary>
        /// Get the current in-game date.
        /// Returns a default SDateTime if not in a game.
        /// 
        /// The returned value can be used with SDateTime static methods:
        ///   SDateTime.GetMonths(date1, date2)  - months between two dates.
        ///   SDateTime.Now()                    - alternative way to get current date.
        /// </summary>
        public static SDateTime GetCurrentDate()
        {
            try
            {
                return SDateTime.Now();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModMarketHelper.GetCurrentDate failed: " + e.Message);
                return default(SDateTime);
            }
        }

        /// <summary>
        /// Get the current game speed multiplier (1 = normal, 2 = fast, etc.).
        /// Returns 0 if not in a game or paused.
        /// </summary>
        public static int GetGameSpeed()
        {
            try
            {
                return (int)GameSettings.GameSpeed;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Check if the game is currently paused.
        /// Returns true if paused or if not in a game.
        /// </summary>
        public static bool IsPaused()
        {
            try
            {
                return GameSettings.GameSpeed == 0f;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Get all available software types in the game (e.g. "Operating System", "Antivirus", etc.).
        /// These are the types that companies can develop products for.
        /// Returns an empty list if not in a game.
        /// </summary>
        public static List<SoftwareType> GetSoftwareTypes()
        {
            var result = new List<SoftwareType>();
            try
            {
                if (MarketSimulation.Active == null || MarketSimulation.Active.SoftwareTypes == null)
                    return result;

                foreach (var kvp in MarketSimulation.Active.SoftwareTypes)
                {
                    if (kvp.Value != null)
                        result.Add(kvp.Value);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModMarketHelper.GetSoftwareTypes failed: " + e.Message);
            }
            return result;
        }

        /// <summary>
        /// Get all available software categories across all software types (e.g. "Business", "Home", etc.).
        /// Returns an empty list if not in a game.
        /// </summary>
        public static List<SoftwareCategory> GetCategories()
        {
            var result = new List<SoftwareCategory>();
            try
            {
                if (MarketSimulation.Active == null || MarketSimulation.Active.SoftwareTypes == null)
                    return result;

                foreach (var type in MarketSimulation.Active.SoftwareTypes.Values)
                {
                    if (type != null && type.Categories != null)
                    {
                        foreach (var cat in type.Categories.Values)
                        {
                            if (cat != null && !result.Contains(cat))
                                result.Add(cat);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModMarketHelper.GetCategories failed: " + e.Message);
            }
            return result;
        }

        /// <summary>
        /// Get the total number of active (non-bankrupt) companies in the current game.
        /// Returns 0 if not in a game.
        /// </summary>
        public static int GetTotalCompanyCount()
        {
            try
            {
                return ModCompanyHelper.GetActiveCompanies().Count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Format a money value into a readable string with abbreviations.
        /// Examples: $1,500 -> "$1.5K", $2,300,000 -> "$2.3M", $5,000,000,000 -> "$5.0B"
        /// </summary>
        public static string FormatMoney(double amount)
        {
            try
            {
                if (amount >= 1000000000) return "$" + (amount / 1000000000).ToString("F1") + "B";
                if (amount >= 1000000) return "$" + (amount / 1000000).ToString("F1") + "M";
                if (amount >= 1000) return "$" + (amount / 1000).ToString("F1") + "K";
                return "$" + amount.ToString("F0");
            }
            catch
            {
                return "$0";
            }
        }

        /// <summary>
        /// Format a money value into a readable string with abbreviations.
        /// Overload that accepts float for convenience.
        /// </summary>
        public static string FormatMoney(float amount)
        {
            return FormatMoney((double)amount);
        }
    }
}
