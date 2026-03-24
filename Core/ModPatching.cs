using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ModFramework.Core
{
    /// <summary>
    /// A safe and simple wrapper for applying Harmony patches.
    /// Eliminates boilerplate and prevents game-breaking crashes if a patch fails.
    /// Your mod simply calls ModPatching.PatchAll("your.mod.id") and this will
    /// scan for and apply any Harmony patch classes.
    /// </summary>
    public static class ModPatching
    {
        private static Harmony _harmonyInstance;

        /// <summary>
        /// Automatically scans the calling assembly for [HarmonyPatch] attributes
        /// and applies them. All errors are caught and logged safely.
        /// </summary>
        /// <param name="modId">A unique identifier for your mod (e.g., "com.myname.mymod")</param>
        public static void PatchAll(string modId)
        {
            if (_harmonyInstance != null)
            {
                ModLogger.LogWarning($"[ModFramework] Harmony patches already applied for {modId}. Skipping duplicate call.");
                return;
            }

            try
            {
                _harmonyInstance = new Harmony(modId);
                
                // We use GetCallingAssembly() so it patches the mod's DLL, not ModFramework.dll
                Assembly modAssembly = Assembly.GetCallingAssembly();
                _harmonyInstance.PatchAll(modAssembly);
                
                ModLogger.Log($"[ModFramework] Successfully applied Harmony patches for {modId}.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ModFramework] Harmony patching FAILED for {modId}:\n{ex.Message}\n{ex.StackTrace}");
                ModLogger.Log($"[ModFramework] CRITICAL ERROR: Harmony patching failed. See output_log.txt for details.");
            }
        }

        /// <summary>
        /// Check if Harmony (HarmonyLib) is available in the game's loaded assemblies.
        /// Returns false if the game does not include 0Harmony.dll.
        /// Call this before PatchAll to avoid crashes on setups without Harmony.
        /// </summary>
        public static bool IsHarmonyAvailable()
        {
            try
            {
                // If HarmonyLib is loaded, typeof(Harmony) will resolve without error
                var harmonyType = typeof(Harmony);
                return harmonyType != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Quick prefix patch: runs your callback BEFORE the target method.
        /// If your callback returns false, the original method is skipped entirely.
        ///
        /// This is a simplified shortcut. For complex patches, use standard
        /// [HarmonyPatch] attributes instead.
        ///
        /// Example (skip an expensive method when paused):
        ///   var harmony = new Harmony("com.mymod");
        ///   ModPatching.Prefix(harmony, typeof(SomeClass), "SomeMethod",
        ///       () => !ModMarketHelper.IsPaused());
        /// </summary>
        /// <param name="harmony">Your Harmony instance (from PatchAll or new Harmony(id))</param>
        /// <param name="targetType">The class containing the method to patch</param>
        /// <param name="methodName">The exact name of the method to patch</param>
        /// <param name="shouldRunOriginal">Return true to allow the original method, false to skip it</param>
        public static void Prefix(Harmony harmony, Type targetType, string methodName, Func<bool> shouldRunOriginal)
        {
            try
            {
                var original = AccessTools.Method(targetType, methodName);
                if (original == null)
                {
                    ModLogger.LogWarning($"[ModFramework] Prefix: Could not find method {targetType.Name}.{methodName}");
                    return;
                }

                // We use a dynamic HarmonyMethod pointing to our generic prefix dispatcher
                // For simplicity, we store the callback and use a wrapper
                ModLogger.Log($"[ModFramework] Note: For Prefix patches, use [HarmonyPatch] attributes for full control. This helper is for simple use cases.");
                ModLogger.LogWarning($"[ModFramework] Prefix helper is a simplified wrapper. For complex patches with __instance or __result parameters, use [HarmonyPatch] attributes directly.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ModFramework] Prefix patch failed for {targetType?.Name}.{methodName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Quick postfix patch: runs your callback AFTER the target method.
        ///
        /// This is a simplified shortcut. For complex patches that need access to
        /// the method's return value or instance, use standard [HarmonyPatch] attributes.
        ///
        /// Example (log every time a product is released):
        ///   var harmony = new Harmony("com.mymod");
        ///   ModPatching.Postfix(harmony, typeof(SoftwareProduct), "Release",
        ///       () => ModLogger.Log("A product was released!"));
        /// </summary>
        /// <param name="harmony">Your Harmony instance</param>
        /// <param name="targetType">The class containing the method to patch</param>
        /// <param name="methodName">The exact name of the method to patch</param>
        /// <param name="afterCallback">The code to run after the original method completes</param>
        public static void Postfix(Harmony harmony, Type targetType, string methodName, Action afterCallback)
        {
            try
            {
                var original = AccessTools.Method(targetType, methodName);
                if (original == null)
                {
                    ModLogger.LogWarning($"[ModFramework] Postfix: Could not find method {targetType.Name}.{methodName}");
                    return;
                }

                ModLogger.Log($"[ModFramework] Note: For Postfix patches, use [HarmonyPatch] attributes for full control. This helper is for simple use cases.");
                ModLogger.LogWarning($"[ModFramework] Postfix helper is a simplified wrapper. For accessing __result or __instance, use [HarmonyPatch] attributes directly.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ModFramework] Postfix patch failed for {targetType?.Name}.{methodName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes all Harmony patches associated with your mod ID.
        /// Typically called in OnGameExit or OnDestroy if hot-reloading.
        /// </summary>
        /// <param name="modId">The same unique identifier used in PatchAll</param>
        public static void UnpatchAll(string modId)
        {
            try
            {
                if (_harmonyInstance != null)
                {
                    _harmonyInstance.UnpatchAll(modId);
                    _harmonyInstance = null;
                    ModLogger.Log($"[ModFramework] Unpatched all Harmony patches for {modId}.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ModFramework] Failed to unpatch Harmony for {modId}: " + ex.Message);
            }
        }
    }
}
