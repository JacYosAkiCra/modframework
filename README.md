# ModFramework for Software Inc

![Software Inc](https://img.shields.io/badge/Game-Software_Inc-blue?style=for-the-badge&logo=steam) ![Unity 2019.4](https://img.shields.io/badge/Unity-2019.4-black?style=for-the-badge&logo=unity) ![License](https://img.shields.io/badge/License-MIT-success?style=for-the-badge)

A complete UI, utility, and gameplay framework for building Software Inc mods.

### What is this and why use it?
Modding Software Inc can be difficult for beginners. The base game uses complex native prefabs and internal APIs that are hard to work with directly.

**ModFramework** solves this by providing:
- **31 custom UI components** (buttons, labels, windows, charts, lists, etc.) that automatically theme themselves to look exactly like the base game
- **Game Data Wrappers** for safely reading company, product, employee, and market data
- **Lifecycle Hooks** so you know exactly when the game is ready, when a day passes, and when the player exits
- **Error Safety** tools that prevent your mod from crashing the game
- **Harmony Helpers** for patching game methods with a single line of code
- **Project Scaffolding** to create a new mod project in 30 seconds

**Namespace:** `ModFramework.UI.Custom` (UI) / `ModFramework.Core` (utilities) / `ModFramework.GameData` (data wrappers)
**Requires:** Unity 2019.4 (game's engine), .NET Framework 4.8
**Dependencies:** Harmony 2.4.1 (bundled, MIT license)
**License:** MIT - Free to use in any Software Inc mod

---

## Quick Start - Create a New Mod (v4)

The fastest way to start is with the scaffolding script:

```powershell
# First run - provide your game install path (cached for future runs)
.\ModFramework\Scaffolding\CreateMod.ps1 -ModName "MyAwesomeMod" -GameDir "C:\SteamLibrary\steamapps\common\Software Inc"

# After first run - path is remembered
.\ModFramework\Scaffolding\CreateMod.ps1 -ModName "AnotherMod"
```

This generates a complete, ready-to-build mod project with all references configured to your local game installation and a post-build event that automatically copies your DLL to the game's mod folder.

## Quick Start - UI Example

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

ModButton.Create("Apply", () => {
    Notifications.ShowSuccess("Settings saved!");
}, window.ContentPanel);

window.Show();
```

No manual `Rect` positioning, no hardcoded colors, no game prefab dependencies.

## Quick Start - Game Data Example (v4)

Read game data safely without any null-reference worries:

```csharp
using ModFramework.GameData;
using ModFramework.Core;

// These will never crash your mod, even if the game isn't fully loaded yet
float cash = ModCompanyHelper.GetPlayerCash();
int employees = ModEmployeeHelper.GetPlayerEmployeeCount();
string money = ModMarketHelper.FormatMoney(cash);  // "$2.5M"

ModLogger.Log("Cash: " + money + ", Employees: " + employees);
```

---

## Installation

### For DLL Mods (Recommended)

**Use the scaffolding script** (see Quick Start above). It generates a complete `.csproj` with all references pointing to your local game installation. No manual path editing needed.

**Harmony is bundled** in `ModFramework/Harmony/0Harmony.dll` so you do not need to install it via NuGet or download it separately.

If you prefer manual setup, add framework files to your `.csproj`:

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
For the complete setup guide, full API reference, v4 Game Data Wrappers, Lifecycle Hooks, and UI cookbook, please see the [Documentation](DOCUMENTATION.md).

---

## Third-Party Licenses

### Harmony

This project bundles [Harmony](https://github.com/pardeike/Harmony) v2.4.1 by Andreas Pardeike for runtime method patching.

**License:** MIT
**Copyright:** (c) 2017 Andreas Pardeike
**Full license:** [Harmony/LICENSE](Harmony/LICENSE)

---

*ModFramework is authored by Zicarius.*
