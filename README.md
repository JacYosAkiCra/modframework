# ModFramework for Software Inc

![Software Inc](https://img.shields.io/badge/Game-Software_Inc-blue?style=for-the-badge&logo=steam) ![Unity 2019.4](https://img.shields.io/badge/Unity-2019.4-black?style=for-the-badge&logo=unity) ![License](https://img.shields.io/badge/License-MIT-success?style=for-the-badge)

A complete UI and utility framework for building Software Inc mods.

### What is this and why use it?
Modding the user interface in Software Inc can be difficult for beginners. The base game uses complex native prefabs and `WindowManager` calls that are hard to modify or extend. 
The **ModFramework** solves this by providing 31 custom UI components (buttons, labels, windows, charts, lists, etc.) built entirely from code that **automatically theme themselves to look exactly like the base game.** 

You get the native Software Inc look and feel, but with the ease of a modern C# framework. You don't need to load any prefabs, and you don't need to do any complex math to position things-everything uses Unity's automatic layout groups!

**Namespace:** `ModFramework.UI.Custom` (UI) / `ModFramework.Core` (utilities)
**Requires:** Unity 2019.4 (game's engine), .NET Framework 4.x
**License:** MIT - Free to use in any Software Inc mod

---

## Table of Contents

1. [Quick Start](#quick-start)
2. [Installation](#installation)
3. [Core Utilities](#core-utilities)
4. [Custom UI Overview](#custom-ui-overview)
5. [GameTheme (Auto-Theming)](#gametheme)
6. [Windows](#windows)
7. [Basic Widgets](#basic-widgets)
8. [Input Widgets](#input-widgets)
9. [Layout Widgets](#layout-widgets)
10. [Data Views](#data-views)
11. [Charts](#charts)
12. [Overlays and Dialogs](#overlays-and-dialogs)
13. [Advanced Widgets](#advanced-widgets)
14. [Gotchas and Tips](#gotchas-and-tips)
15. [Architecture](#architecture)
16. [Production Examples](#production-examples)

---

## Quick Start

Create a themed window with widgets in just a few lines:

```csharp
using ModFramework.UI.Custom;

// Create a window (auto-themed to match the game)
ModWindow window = ModWindow.Create("My Mod", 500, 400, singletonKey: "mymod_main");

// Add widgets to window.ContentPanel
ModHeader.Create("Settings", window.ContentPanel);
ModLabel.Create("Configure your mod below.", window.ContentPanel);

ModToggle.Create("Enable Feature", true, val => {
    ModSettings.SetBool("feature_enabled", val);
}, window.ContentPanel);

ModSlider.Create(0.5f, 2.0f, 1.0f, val => {
    ModSettings.SetFloat("multiplier", val);
}, window.ContentPanel);

ModButton.Create("Apply", () => {
    Notifications.ShowSuccess("Settings saved!");
}, window.ContentPanel);

// Show the window
window.Show();
```

That's it. No manual `Rect` positioning, no hardcoded colors, no game prefab dependencies.

---

## Installation

### For DLL Mods (Recommended)

Add all framework files to your `.csproj`:

```xml
<!-- Core utilities -->
<Compile Include="..\ModFramework\Core\ModLogger.cs" Link="ModFramework\Core\ModLogger.cs" />
<Compile Include="..\ModFramework\Core\ModEvents.cs" Link="ModFramework\Core\ModEvents.cs" />
<Compile Include="..\ModFramework\Core\ModSettings.cs" Link="ModFramework\Core\ModSettings.cs" />
<Compile Include="..\ModFramework\Core\ModUtils.cs" Link="ModFramework\Core\ModUtils.cs" />
<Compile Include="..\ModFramework\Core\Notifications.cs" Link="ModFramework\Core\Notifications.cs" />

<!-- Custom UI widgets (include only what you need, or include all) -->
<Compile Include="..\ModFramework\UI\Custom\GameTheme.cs" Link="ModFramework\UI\Custom\GameTheme.cs" />
<Compile Include="..\ModFramework\UI\Custom\ModWindow.cs" Link="ModFramework\UI\Custom\ModWindow.cs" />
<Compile Include="..\ModFramework\UI\Custom\ModButton.cs" Link="ModFramework\UI\Custom\ModButton.cs" />
<!-- ... add more as needed ... -->
```

Everything compiles directly into your mod's DLL. Do NOT deploy `.cs` files alongside the DLL.

### For CS Script Mods

CS script mods can only use the legacy `ModFramework.cs` single-file version (in `UI/Vanilla/`).
The Custom UI system uses namespaces and multi-file architecture which CS script mods cannot handle.

Copy `ModFramework.cs` into your mod folder and deploy it as a `.cs` file to `DLLMods/YourMod/`.

## Documentation
For the complete setup guide and full API reference, please see the [Documentation](DOCUMENTATION.md).
