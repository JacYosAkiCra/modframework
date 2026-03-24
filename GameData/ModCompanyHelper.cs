using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Safe, read-only helper methods for accessing company data from the game.
/// All methods include null-checks and error handling so they never crash your mod.
/// 
/// These wrap the game's public API surface only. No private or internal fields are accessed.
///
/// Usage:
///   var player = ModCompanyHelper.GetPlayerCompany();
///   var rivals = ModCompanyHelper.GetActiveCompanies();
///   float cash = ModCompanyHelper.GetPlayerCash();
/// </summary>
namespace ModFramework.GameData
{
    public static class ModCompanyHelper
    {
        /// <summary>
        /// Get the player's own company.
        /// Returns null if not currently in a game (e.g. on the main menu).
        /// </summary>
        public static Company GetPlayerCompany()
        {
            try
            {
                if (GameSettings.Instance == null) return null;
                return GameSettings.Instance.MyCompany;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModCompanyHelper.GetPlayerCompany failed: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Get all active AI companies (excludes the player's company and bankrupt companies).
        /// Returns an empty list if not in a game or if no companies exist.
        /// </summary>
        public static List<SimulatedCompany> GetActiveCompanies()
        {
            var result = new List<SimulatedCompany>();
            try
            {
                if (GameSettings.Instance == null || GameSettings.Instance.simulation == null) return result;
                var companies = GameSettings.Instance.simulation.Companies;
                if (companies == null) return result;

                foreach (var kvp in companies)
                {
                    if (kvp.Value != null && !kvp.Value.Bankrupt)
                    {
                        result.Add(kvp.Value);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModCompanyHelper.GetActiveCompanies failed: " + e.Message);
            }
            return result;
        }

        /// <summary>
        /// Get all active AI companies as key-value pairs where the key is the company's uint ID.
        /// This is useful if you need to store or reference companies by their unique ID.
        /// Excludes bankrupt companies.
        /// </summary>
        public static List<KeyValuePair<uint, SimulatedCompany>> GetActiveCompaniesWithIds()
        {
            var result = new List<KeyValuePair<uint, SimulatedCompany>>();
            try
            {
                if (GameSettings.Instance == null || GameSettings.Instance.simulation == null) return result;
                var companies = GameSettings.Instance.simulation.Companies;
                if (companies == null) return result;

                foreach (var kvp in companies)
                {
                    if (kvp.Value != null && !kvp.Value.Bankrupt)
                    {
                        result.Add(kvp);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModCompanyHelper.GetActiveCompaniesWithIds failed: " + e.Message);
            }
            return result;
        }

        /// <summary>
        /// Find a specific AI company by its name (case-insensitive search).
        /// Returns null if no company with that name is found.
        /// </summary>
        public static SimulatedCompany FindByName(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name)) return null;
                var companies = GetActiveCompanies();
                return companies.FirstOrDefault(c =>
                    c.Name != null && c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModCompanyHelper.FindByName failed: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Find a specific AI company by its unique uint ID.
        /// Returns null if not found or if the company is bankrupt.
        /// </summary>
        public static SimulatedCompany FindById(uint companyId)
        {
            try
            {
                if (GameSettings.Instance == null || GameSettings.Instance.simulation == null) return null;
                var companies = GameSettings.Instance.simulation.Companies;
                if (companies == null) return null;

                SimulatedCompany comp;
                if (companies.TryGetValue(companyId, out comp) && comp != null && !comp.Bankrupt)
                {
                    return comp;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModCompanyHelper.FindById failed: " + e.Message);
            }
            return null;
        }

        /// <summary>
        /// Get the player's current cash balance.
        /// Returns 0 if not in a game.
        /// </summary>
        public static float GetPlayerCash()
        {
            try
            {
                var company = GetPlayerCompany();
                if (company == null) return 0f;
                return (float)company.Money;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModCompanyHelper.GetPlayerCash failed: " + e.Message);
                return 0f;
            }
        }

        /// <summary>
        /// Get all active companies sorted by revenue (highest first).
        /// Useful for leaderboard-style displays.
        /// </summary>
        public static List<SimulatedCompany> GetByRevenue()
        {
            try
            {
                var companies = GetActiveCompanies();
                companies.Sort((a, b) =>
                {
                    try { return b.Money.CompareTo(a.Money); }
                    catch { return 0; }
                });
                return companies;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModCompanyHelper.GetByRevenue failed: " + e.Message);
                return new List<SimulatedCompany>();
            }
        }

        /// <summary>
        /// Get all active companies sorted alphabetically by name.
        /// </summary>
        public static List<SimulatedCompany> GetByName()
        {
            try
            {
                var companies = GetActiveCompanies();
                companies.Sort((a, b) =>
                    string.Compare(a.Name ?? "", b.Name ?? "", StringComparison.OrdinalIgnoreCase));
                return companies;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModCompanyHelper.GetByName failed: " + e.Message);
                return new List<SimulatedCompany>();
            }
        }

        /// <summary>
        /// Check if a specific company is bankrupt.
        /// Returns true if the company is null or bankrupt.
        /// </summary>
        public static bool IsBankrupt(SimulatedCompany company)
        {
            try
            {
                if (company == null) return true;
                return company.Bankrupt;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Get the total number of products for a company.
        /// Returns 0 if the company is null or has no products.
        /// </summary>
        public static int GetProductCount(SimulatedCompany company)
        {
            try
            {
                if (company == null || company.Products == null) return 0;
                return company.Products.Count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get the number of products the company is currently developing.
        /// Returns 0 if the company is null or has no active projects.
        /// </summary>
        public static int GetActiveProjectCount(SimulatedCompany company)
        {
            try
            {
                if (company == null || company.Releases == null) return 0;
                return company.Releases.Count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get the company type name (e.g. "Software", "Hardware").
        /// Returns "Unknown" if the company or type is null.
        /// </summary>
        public static string GetCompanyTypeName(SimulatedCompany company)
        {
            try
            {
                if (company == null || company.Type == null) return "Unknown";
                return company.Type.Name ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Check if the game is currently loaded and the player has a company.
        /// Use this before calling any other helper methods to avoid errors.
        /// </summary>
        public static bool IsInGame()
        {
            try
            {
                return GameSettings.Instance != null
                    && GameSettings.Instance.MyCompany != null
                    && HUD.Instance != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
