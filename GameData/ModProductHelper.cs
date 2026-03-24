using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Safe, read-only helper methods for accessing software product data from the game.
/// All methods include null-checks and error handling so they never crash your mod.
///
/// Usage:
///   var myProducts = ModProductHelper.GetPlayerProducts();
///   float users = ModProductHelper.GetUserbase(someProduct);
///   var osList = ModProductHelper.GetByType("Operating System");
/// </summary>
namespace ModFramework.GameData
{
    public static class ModProductHelper
    {
        /// <summary>
        /// Get all products released by the player's company.
        /// Returns an empty list if not in a game or if the player has no products.
        /// </summary>
        public static List<SoftwareProduct> GetPlayerProducts()
        {
            var result = new List<SoftwareProduct>();
            try
            {
                var company = ModCompanyHelper.GetPlayerCompany();
                if (company == null || company.Products == null) return result;

                foreach (var product in company.Products)
                {
                    if (product != null)
                        result.Add(product);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModProductHelper.GetPlayerProducts failed: " + e.Message);
            }
            return result;
        }

        /// <summary>
        /// Get all products on the market from all companies.
        /// Returns an empty list if not in a game.
        /// Note: This can be a large list in late-game saves.
        /// </summary>
        public static List<SoftwareProduct> GetAllProducts()
        {
            var result = new List<SoftwareProduct>();
            try
            {
                if (GameSettings.Instance == null || GameSettings.Instance.simulation == null) return result;

                var companies = GameSettings.Instance.simulation.Companies;
                if (companies == null) return result;

                foreach (var kvp in companies)
                {
                    if (kvp.Value != null && kvp.Value.Products != null)
                    {
                        foreach (var product in kvp.Value.Products)
                        {
                            if (product != null)
                                result.Add(product);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModProductHelper.GetAllProducts failed: " + e.Message);
            }
            return result;
        }

        /// <summary>
        /// Get all products of a specific software type name (e.g. "Operating System", "Game").
        /// The type name is case-insensitive.
        /// Returns an empty list if no matching products are found.
        /// </summary>
        public static List<SoftwareProduct> GetByType(string typeName)
        {
            try
            {
                if (string.IsNullOrEmpty(typeName)) return new List<SoftwareProduct>();
                return GetAllProducts().Where(p =>
                    p.Type != null &&
                    p.Type.Name != null &&
                    p.Type.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ModFramework] ModProductHelper.GetByType failed: " + e.Message);
                return new List<SoftwareProduct>();
            }
        }

        /// <summary>
        /// Get the software type name for a product (e.g. "Operating System", "Antivirus").
        /// Returns "Unknown" if the product or its type is null.
        /// </summary>
        public static string GetTypeName(SoftwareProduct product)
        {
            try
            {
                if (product == null || product.Type == null) return "Unknown";
                return product.Type.Name ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get the category name for a product (e.g. "Business", "Entertainment").
        /// Returns "Unknown" if the product or its category is null.
        /// </summary>
        public static string GetCategoryName(SoftwareProduct product)
        {
            try
            {
                if (product == null || product.Category == null) return "Unknown";
                return product.Category.Name ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Get the company that developed a specific product.
        /// Returns null if the product has no associated company.
        /// </summary>
        public static Company GetDeveloper(SoftwareProduct product)
        {
            try
            {
                if (product == null || product.DevCompany == null) return null;
                return product.DevCompany;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get the quality of a product as a value between 0.0 and 1.0.
        /// Returns 0 if the product is null.
        /// </summary>
        public static float GetQuality(SoftwareProduct product)
        {
            try
            {
                if (product == null || product.Quality == null || product.Quality.Length == 0) return 0f;
                return (float)product.Quality.Average();
            }
            catch
            {
                return 0f;
            }
        }

        /// <summary>
        /// Get the current bug count for a product.
        /// Returns 0 if the product is null.
        /// </summary>
        public static int GetBugCount(SoftwareProduct product)
        {
            try
            {
                if (product == null) return 0;
                return product.Bugss;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get the name of a product.
        /// Returns "Unknown" if the product is null or has no name.
        /// </summary>
        public static string GetName(SoftwareProduct product)
        {
            try
            {
                if (product == null) return "Unknown";
                return product.Name ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
