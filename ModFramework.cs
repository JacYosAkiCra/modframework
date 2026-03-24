// =============================================================================
// ModFramework - Reusable utilities for Software Inc modding
// Created by: Zicarius
// Version: 4.0
//
// This file previously contained all framework code in a single 893-line file.
// It has been split into organized modules for maintainability:
//
//   Core/ModLogger.cs       - Logging system (Log, Warn, Error, Success)
//   Core/ModEvents.cs       - Pub/sub event system for mod communication
//   Core/ModSettings.cs     - Disk-backed persistent settings store
//   Core/ModUtils.cs        - Common utilities (formatting, singletons, etc.)
//   Core/Notifications.cs   - In-game HUD notification popups
//   Core/ModLifecycle.cs    - Safe game lifecycle hooks (v4)
//   Core/ModSafety.cs       - Error safety wrappers (v4)
//   Core/ModPatching.cs     - Harmony patch helpers (v4)
//   GameData/               - Safe game data wrappers (v4)
//   Scaffolding/            - New mod project generator (v4)
//   UI/Vanilla/UIHelper.cs  - Legacy UI helpers wrapping WindowManager prefabs
//   UI/Custom/              - Custom UI framework (31 components)
//
// All classes remain in the `ModFramework` namespace.
// Existing mods using `using ModFramework;` require NO code changes.
// =============================================================================
