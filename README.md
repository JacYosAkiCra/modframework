# ModFramework for Software Inc

![Software Inc](https://img.shields.io/badge/Game-Software_Inc-blue?style=for-the-badge&logo=steam) ![Unity 2019.4](https://img.shields.io/badge/Unity-2019.4-black?style=for-the-badge&logo=unity) ![License](https://img.shields.io/badge/License-MIT-success?style=for-the-badge)

A complete UI and utility framework for building Software Inc mods.

### What is this and why use it?
Modding the user interface in Software Inc can be difficult for beginners. The base game uses complex native prefabs and `WindowManager` calls that are hard to modify or extend. 
The **ModFramework** solves this by providing 31 custom UI components (buttons, labels, windows, charts, lists, etc.) built entirely from code that **automatically theme themselves to look exactly like the base game.** 

You get the native Software Inc look and feel, but with the ease of a modern C# framework. You don't need to load any prefabs, and you don't need to do any complex math to position things—everything uses Unity's automatic layout groups!

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

---

## Core Utilities

### ModLogger

Buffered logging with severity levels. All output goes to the game's console (F12).

```csharp
using ModFramework.Core;

ModLogger.Log("General message");
ModLogger.LogSuccess("Operation completed!");     // Prefixed with checkmark
ModLogger.LogWarning("Something unexpected");      // Prefixed with warning icon
ModLogger.LogError("Critical failure: " + ex.Message);
```

### ModSettings

Persistent key-value settings stored on disk (Base64 encoded).

```csharp
using ModFramework.Core;

// Save settings
ModSettings.SetFloat("multiplier", 1.5f);
ModSettings.SetBool("feature_on", true);
ModSettings.SetString("hotkey", "F3");
ModSettings.SetInt("count", 42);

// Load settings (with default fallback)
float mult = ModSettings.GetFloat("multiplier", 1.0f);
bool enabled = ModSettings.GetBool("feature_on", false);
string key = ModSettings.GetString("hotkey", "F5");
int count = ModSettings.GetInt("count", 10);
```

### ModEvents

Publish/subscribe event bus for decoupled communication between mod components.

```csharp
using ModFramework.Core;

// Subscribe
ModEvents.OnGameLoaded += () => { /* initialize */ };

// Custom events
ModEvents.Subscribe("mymod.refresh", () => RebuildUI());
ModEvents.Publish("mymod.refresh");
```

### Notifications

In-game toast notifications (appears in the game's notification area).

```csharp
using ModFramework.Core;

Notifications.ShowSuccess("Settings saved!");
Notifications.ShowWarning("Low funds detected");
Notifications.ShowError("Failed to load config");
```

### ModUtils

General-purpose utility functions.

```csharp
using ModFramework.Core;

string path = ModUtils.GetSafeFilePath("config.json");
```

---

## Custom UI Overview

The Custom UI system builds UI elements from raw Unity `GameObjects`, with no dependency on the game's `WindowManager` prefabs. All widgets:

- **Auto-theme** by sampling colors/fonts from the game's own prefabs at runtime
- **Use LayoutGroups** for automatic positioning (no manual `Rect` math)
- **Return their Unity component** so you can further customize them
- **Follow a consistent `Create()` factory pattern**

### Widget Return Types

| Widget | Returns | What You Get Back |
|--------|---------|-------------------|
| `ModButton` | `GameObject` | The button root (get `Button` via `GetComponent`) |
| `ModLabel` | `Text` | Direct access to change `.text`, `.color`, etc. |
| `ModHeader` | `Text` | Bold section header text |
| `ModToggle` | `Toggle` | Access `.isOn`, attach listeners |
| `ModSlider` | `Slider` | Access `.value`, `.minValue`, `.maxValue` |
| `ModInputField` | `InputField` | Access `.text`, add validation |
| `ModWindow` | `ModWindow` | Full window instance with `.Show()`, `.Hide()`, `.Close()` |
| `ModListView<T>` | `ModListView<T>` | Data-bound list with `.SetItems()`, `.Refresh()` |
| `ModTable<T>` | `ModTable<T>` | Table with headers built on `ModListView<T>` |

---

## GameTheme

`GameTheme` is a static class that samples the game's actual UI prefabs at runtime to extract colors, fonts, and sizes. It initializes automatically when any widget is created.

### Available Theme Tokens

```csharp
// Colors
GameTheme.WindowBackground    // Main window body color
GameTheme.PanelBackground     // Subtle grouping panel color
GameTheme.ButtonNormal        // Default button color
GameTheme.ButtonHover         // Hovered button color
GameTheme.ButtonPressed       // Pressed button color
GameTheme.LabelColor          // Standard text color
GameTheme.HeaderColor         // Section header text color
GameTheme.TitleBarColor       // Window title bar background (green)
GameTheme.InputBackground     // Input field background
GameTheme.ScrollbarHandle     // Scrollbar thumb color
GameTheme.Separator           // Divider line color

// Typography
GameTheme.GameFont            // The game's font (sampled from labels)
GameTheme.DefaultFontSize     // Standard text size (typically 14)
GameTheme.HeaderFontSize      // Header text size (DefaultFontSize + 2)
GameTheme.SmallFontSize       // Small text size (DefaultFontSize - 2)

// Spacing
GameTheme.WindowPadding       // 10f - inner window padding
GameTheme.SectionSpacing      // 8f  - gap between stacked widgets
GameTheme.RowHeight           // 28f - standard row height
GameTheme.ButtonHeight        // 26f - standard button height
GameTheme.TitleBarHeight      // 30f - window title bar height
```

### Manual Initialization

GameTheme auto-initializes when any widget calls `Create()`. You can also:

```csharp
GameTheme.Initialize();       // Safe to call multiple times (no-op after first)
GameTheme.Reinitialize();     // Force re-sample (e.g., if game UI scale changes)
```

---

## Windows

### ModWindow

A fully custom window with drag, collapse, pin (always-on-top), close, and position persistence.

```csharp
// Basic window
ModWindow window = ModWindow.Create("Title", 500, 400);

// Singleton window (only one instance, re-shows if already created)
ModWindow window = ModWindow.Create("Title", 500, 400, singletonKey: "unique_id");

// Add widgets to the content panel
ModLabel.Create("Hello!", window.ContentPanel);

// Control visibility
window.Show();              // Show and bring to front
window.Hide();              // Hide (preserves state)
window.Close();             // Destroy permanently

// Collapse/expand
window.ToggleCollapse();    // Minimizes to title bar only

// Pin on top
window.TogglePin();         // Stays above other windows

// Events
window.OnClose += () => { /* cleanup */ };
window.OnCollapse += () => { /* react */ };

// Properties
bool visible = window.IsVisible;
bool collapsed = window.IsCollapsed;
bool pinned = window.IsPinned;
```

**Window structure:**
```
Root (green title bar background)
  |-- TitleBar (draggable, contains title text + close/collapse buttons)
  |-- Content (gray body)
       |-- ContentPanel (VerticalLayoutGroup - add your widgets here)
```

### ModWindowRegistry

Manages all open ModWindows. Handles z-ordering, focus tracking, and singleton enforcement.

```csharp
// Get a singleton window
ModWindow win = ModWindowRegistry.GetSingleton("my_mod_window");

// Focus management (handled automatically on click, but available manually)
ModWindowRegistry.SetFocused(myWindow);
```

---

## Live Refresh (Auto-Updating Data)

The framework supports in-place live data updates without any destroy/recreate flicker. Data updates happen by directly setting `.text`, `.value`, or anchor properties on existing Unity components.

### Quick Start

```csharp
var window = ModWindow.Create("Dashboard", 500, 400, "dashboard");

// Enable refresh (must be called BEFORE CreateLive() widgets)
window.SetRefreshInterval(2f);  // Tick every 2 seconds

// Live labels - text updates automatically every tick
ModLabel.CreateLive(() => "Revenue: $" + company.Revenue.ToString("N0"), window.ContentPanel);
ModLabel.CreateLive(() => "Employees: " + company.Workers.Count, window.ContentPanel, bold: true);

// Live progress bar - fill updates automatically
ModProgressBar.CreateLive(() => project.Progress, window.ContentPanel);

// Custom refresh callback for anything else
window.OnRefresh(() => {
    myListView.SetData(GetLatestCompanies());
    myChart.SetData(GetLatestMetrics());
    myChart.Rebuild();
});

// Force immediate first tick so data is populated right away
window.RefreshNow();
window.Show();
```

### How It Works

`ModRefreshDriver` is a `MonoBehaviour` that ticks registered callbacks every N seconds. It:

- **Auto-pauses** when the window is hidden (inactive GameObjects don't tick `Update()`)
- **Auto-resumes** when the window is shown again
- **Catches exceptions** in callbacks and removes broken ones to prevent log spam
- **Zero overhead** for windows that don't use refresh (driver is lazily created)

### ModWindow Refresh API

```csharp
// Register a refresh callback
window.OnRefresh(() => { /* update UI in-place */ });

// Set refresh interval (default 3 seconds)
window.SetRefreshInterval(1.5f);

// Force immediate refresh
window.RefreshNow();

// Access the driver directly for advanced use
window.RefreshDriver.Register(myCallback);
window.RefreshDriver.Unregister(myCallback);
```

### Live Widget Variants

| Widget | Static Version | Live Version |
|--------|---------------|--------------|
| `ModLabel` | `Create(string, parent)` | `CreateLive(Func<string>, parent)` |
| `ModProgressBar` | `Create(float, parent)` | `CreateLive(Func<float>, parent)` |

Live variants call the `Func<>` on every refresh tick and update the widget in-place.

### Important: Initialization Order

`CreateLive()` uses `GetComponentInParent<ModRefreshDriver>()` to find the refresh driver. You **must** call `window.SetRefreshInterval()` or `window.OnRefresh()` **before** any `CreateLive()` calls, so the driver exists in the hierarchy.

```csharp
// CORRECT order:
window.SetRefreshInterval(2f);                    // Driver created on Root
ModLabel.CreateLive(() => "...", window.ContentPanel); // Finds driver via GetComponentInParent

// WRONG order:
ModLabel.CreateLive(() => "...", window.ContentPanel); // No driver found! Label won't update
window.SetRefreshInterval(2f);                    // Too late
```

### Using ModRefreshDriver Standalone

You can also use `ModRefreshDriver` directly on any `GameObject`, not just windows:

```csharp
var driver = myGameObject.AddComponent<ModRefreshDriver>();
driver.Interval = 5f;
driver.Register(() => UpdateSomething());
driver.RefreshNow();
```

---

## Basic Widgets

### ModButton

```csharp
// Full-width stretchy button
GameObject btn = ModButton.Create("Click Me", () => {
    Debug.Log("Clicked!");
}, parent);

// Fixed-width button
GameObject btn = ModButton.Create("OK", () => { }, parent, 100f);
```

### ModLabel

```csharp
// Normal text
Text label = ModLabel.Create("Some text", parent);

// Bold text
Text label = ModLabel.Create("Important text", parent, bold: true);

// Update text later
label.text = "Updated!";
label.color = Color.red;
```

### ModHeader

```csharp
// Bold section header with themed color
Text header = ModHeader.Create("Section Title", parent);
```

---

## Input Widgets

### ModToggle (Checkbox)

```csharp
Toggle toggle = ModToggle.Create("Enable Feature", true, val => {
    Debug.Log("Toggle changed to: " + val);
}, parent);

// Read state later
bool isOn = toggle.isOn;
```

### ModSlider

```csharp
Slider slider = ModSlider.Create(
    min: 0f,
    max: 100f,
    value: 50f,
    onChange: val => Debug.Log("Value: " + val),
    parent: parent
);

// Read/write value
slider.value = 75f;
slider.wholeNumbers = true;  // Snap to integers
```

### ModInputField (Single-line Text)

```csharp
InputField input = ModInputField.Create("initial text", val => {
    Debug.Log("Text: " + val);
}, parent);

// Read value
string text = input.text;
```

### ModTextArea (Multi-line Text)

```csharp
InputField textarea = ModTextArea.Create("initial", lines: 4, val => {
    Debug.Log("Content: " + val);
}, parent);
```

### ModSearchField

```csharp
InputField search = ModSearchField.Create("Search companies...", query => {
    FilterList(query);
}, parent);
```

### ModNumericInput

```csharp
InputField numInput = ModNumericInput.Create(
    value: 10,
    min: 1,
    max: 100,
    onChange: val => Debug.Log("Number: " + val),
    parent: parent
);
```

---

## Layout Widgets

### ModPanel

Container panels for grouping widgets together.

```csharp
// Vertical layout panel (default)
GameObject panel = ModPanel.Create(parent);

// With subtle background tint
GameObject panel = ModPanel.Create(parent, withBackground: true);

// Horizontal layout panel
GameObject hPanel = ModPanel.CreateHorizontal(parent);
```

### ModScrollView

Scrollable container for content that exceeds available space.

```csharp
// Create a scroll view with a fixed visible height
GameObject scrollContent = ModScrollView.Create(parent, visibleHeight: 300f);

// Add widgets to scrollContent - they will scroll when they exceed 300px
ModLabel.Create("Item 1", scrollContent);
ModLabel.Create("Item 2", scrollContent);
// ... many more items
```

### ModAccordion

Collapsible sections with click-to-expand headers.

```csharp
ModAccordion accordion = ModAccordion.Create(parent, singleExpand: false);

// Add sections - returns the content panel to add widgets to
GameObject section1 = accordion.AddSection("General Settings", startExpanded: true);
ModToggle.Create("Enable logging", true, val => { }, section1);
ModSlider.Create(0, 100, 50, val => { }, section1);

GameObject section2 = accordion.AddSection("Advanced");
ModLabel.Create("Advanced options go here", section2);

// singleExpand: true = opening one section auto-closes others (like a tab bar)
```

### ModSplitPane

Side-by-side panel split with adjustable divider.

```csharp
// Creates two panels side by side
ModSplitPane split = ModSplitPane.Create(parent, splitRatio: 0.3f);

// Left panel (30% width)
ModLabel.Create("Navigation", split.LeftPanel);

// Right panel (70% width)
ModLabel.Create("Content", split.RightPanel);
```

### ModCardLayout

Grid of cards for displaying collections visually.

```csharp
ModCardLayout cards = ModCardLayout.Create(parent, cardWidth: 150f, cardHeight: 100f);
GameObject card1 = cards.AddCard();
ModLabel.Create("Card 1", card1);
```

---

## Data Views

### ModListView<T>

High-performance generic list with object pooling, search filtering, and pagination.

```csharp
// Define how rows are created and bound to data
var listView = ModListView<Employee>.Create(
    parent: parent,
    rowHeight: 30f,
    onCreateRow: row => {
        // Called ONCE per pooled row to build its internal UI
        ModLabel.Create("", row);                     // Name
        ModLabel.Create("", row);                     // Skill
    },
    onBindRow: (employee, row) => {
        // Called when binding data to a row
        var texts = row.GetComponentsInChildren<Text>();
        texts[0].text = employee.Name;
        texts[1].text = employee.GetSkill().ToString("F1");
    },
    onFilter: (employee, search) => {
        // Optional: enables built-in search bar
        return employee.Name.ToLower().Contains(search.ToLower());
    },
    itemsPerPage: 50
);

// Set data
listView.SetItems(employeeList);

// Refresh after data changes
listView.Refresh();
```

### ModTable<T>

Table with header row, column widths, and built-in sorting. Built on top of `ModListView<T>`.

```csharp
var columns = new List<ModTableColumn<Company>> {
    new ModTableColumn<Company> {
        HeaderName = "Name",
        Width = 0,  // 0 = flexible width (fills remaining space)
        OnBindCell = (company, cell) => {
            ModLabel.Create(company.Name, cell);
        }
    },
    new ModTableColumn<Company> {
        HeaderName = "Revenue",
        Width = 100,  // Fixed 100px column
        OnBindCell = (company, cell) => {
            ModLabel.Create("$" + company.Revenue.ToString("N0"), cell);
        }
    },
    new ModTableColumn<Company> {
        HeaderName = "Employees",
        Width = 80,
        OnBindCell = (company, cell) => {
            ModLabel.Create(company.EmployeeCount.ToString(), cell);
        }
    }
};

var table = ModTable<Company>.Create(
    parent: parent,
    rowHeight: 28f,
    columns: columns,
    onFilter: (company, search) => company.Name.ToLower().Contains(search.ToLower()),
    itemsPerPage: 50
);

table.ListView.SetItems(companyList);
```

---

## Charts

### ModBarChart

Horizontal bar chart with labels, proportional fills, and value text.

```csharp
ModBarChart barChart = ModBarChart.Create(parent, height: 200f);

barChart.SetData(new BarChartEntry[] {
    new BarChartEntry("Windows", 0.65f, Color.blue),
    new BarChartEntry("Mac",     0.20f, Color.gray),
    new BarChartEntry("Linux",   0.15f, new Color(1f, 0.5f, 0f))
});

// Values <= 1 display as percentages, > 1 display as raw numbers
barChart.SetData(new BarChartEntry[] {
    new BarChartEntry("Revenue", 5000000f, Color.green),
    new BarChartEntry("Costs",   3500000f, Color.red)
});
```

### ModLineChart

Smooth line chart using custom `MaskableGraphic` mesh rendering. Supports multiple series with auto-scaling Y-axis.

```csharp
ModLineChart lineChart = ModLineChart.Create(parent, height: 200f);

// Add data series
lineChart.AddSeries("Revenue", new float[] {
    100, 150, 200, 180, 250, 300, 280, 350
}, Color.green);

lineChart.AddSeries("Costs", new float[] {
    80, 90, 110, 120, 130, 150, 160, 170
}, Color.red);

// Build the chart (call after adding all series)
lineChart.Rebuild();

// Clear and rebuild with new data
lineChart.ClearSeries();
lineChart.AddSeries("New Data", newValues, Color.cyan);
lineChart.Rebuild();
```

### ModPieChart

Radial fill pie chart with in-slice labels, dark outline ring, and color-swatch legend.

```csharp
ModPieChart pieChart = ModPieChart.Create(parent, size: 180f);

pieChart.SetData(new PieSlice[] {
    new PieSlice("Windows",  65f, Color.blue),
    new PieSlice("Mac",      20f, Color.gray),
    new PieSlice("Linux",    10f, new Color(1f, 0.5f, 0f)),
    new PieSlice("Mobile",    5f, Color.green)
});

// Labels automatically appear on slices >= 5% proportion
// Legend with colored swatches appears below the chart
```

---

## Overlays and Dialogs

### ModDialog

Modal dialog boxes for confirmations and messages. Includes a full-screen dim overlay that blocks clicks.

```csharp
// Simple message (OK button)
ModDialog.ShowMessage("Success", "Your mod has been configured!");

// Confirmation dialog (Yes/No buttons)
ModDialog.ShowConfirm(
    "Delete All Data",
    "Are you sure you want to reset all settings?",
    onConfirm: () => {
        // User clicked "Yes"
        ResetEverything();
    },
    onCancel: () => {
        // User clicked "No" (optional)
    }
);
```

### ModHUD

Lightweight Heads-Up Display overlay. Attaches to screen edges. Used for persistent, unclosable elements like resource trackers.

```csharp
// Create a HUD in the top-right corner
ModHUD hud = ModHUD.Create(
    name: "ResourceTracker",
    anchorTarget: TextAnchor.UpperRight,
    offset: new Vector2(-10f, -10f),
    blocksRaycasts: false  // Clicks pass through to the game
);

// Add widgets to hud.Root
ModLabel.Create("$1,000,000", hud.Root);

// Clean up
hud.Destroy();
```

**Anchor positions:** `UpperLeft`, `UpperCenter`, `UpperRight`, `MiddleLeft`, `MiddleCenter`, `MiddleRight`, `LowerLeft`, `LowerCenter`, `LowerRight`

### ModTooltip

Tooltip that follows the mouse and appears on hover.

```csharp
ModTooltip.Show("Hover text appears here", targetElement);
```

### ModContextMenu

Right-click context menu positioned at the mouse cursor. Auto-dismisses on click outside or Escape.

```csharp
// Show a context menu (typically in response to right-click)
ModContextMenu.Show(new List<ContextMenuItem> {
    new ContextMenuItem("Copy", () => CopyToClipboard()),
    new ContextMenuItem("Delete", () => DeleteItem(), Color.red),
    ContextMenuItem.CreateSeparator(),
    new ContextMenuItem("Properties", () => ShowProperties())
});
```

### ModNotificationBadge

Small counter badge (red circle with number) that attaches to any UI element. Auto-hides when count is 0.

```csharp
// Attach to any existing element
ModNotificationBadge badge = ModNotificationBadge.Create(
    target: someButton,
    initialCount: 3,
    badgeColor: Color.red  // Optional, defaults to red
);

// Update count
badge.SetCount(5);
badge.Increment();

// Read count
int count = badge.Count;

// Show/hide manually
badge.Show();
badge.Hide();

// Auto-hides when count reaches 0
badge.SetCount(0);  // Badge disappears
```

---

### ModHotkeyRegistry

A centralized static registry for managing all mod hotkeys. Ensures no conflicts and handles polling automatically.

```csharp
// Register a global hotkey (replaces per-mod Update polling)
ModHotkeyRegistry.Register("mymod.toggle", "Toggle Window", KeyCode.F4, () => {
    ToggleMyModWindow();
});

// Rebind (e.g. from settings screen)
ModHotkeyRegistry.Rebind("mymod.toggle", KeyCode.F5);

// Check current binding
KeyCode currentKey = ModHotkeyRegistry.GetKey("mymod.toggle");
```

### ModNodeGraph

A visual node graph with automatic tree layout, scrolling, and custom edge rendering.

```csharp
var graph = ModNodeGraph.Create(parent, width: -1f, height: 350f);

// Add nodes
graph.AddNode(new NodeGraphNode { Id = "engine", Label = "Game Engine" });
graph.AddNode(new NodeGraphNode { Id = "render", Label = "Rendering" });

// Add directional edges
graph.AddEdge("engine", "render");

// Build and auto-layout
graph.AutoLayoutTree();
graph.Rebuild();
```

### Window Resizing

`ModWindow` instances are resizable by default. Users can click and drag the bottom right corner to resize the window dynamically. Content within the window will automatically reflow to match the new dimensions. The new geometry is also saved to disk.

```csharp
// Optional: restrict sizing
window.SetMinSize(300f, 200f);
window.SetMaxSize(1200f, 800f);

// Disable resizing altogether
window.SetResizable(false);
```

---

## Advanced Widgets

### ModConsoleWindow

A debug console window for viewing logs during development.

```csharp
// Typically opened via a hotkey (e.g., F8)
ModConsoleWindow console = ModConsoleWindow.Create();
console.Log("Debug message");
console.LogWarning("Warning!");
console.LogError("Error occurred");
```

### ModKeybind

A "press any key to bind" input widget for configurable hotkeys in settings screens.

```csharp
// In your settings UI:
ModKeybind.Create("Toggle Window", KeyCode.F3, newKey => {
    ToggleKey = newKey;
    ModSettings.SetString("hotkey", newKey.ToString());
}, parent);
```

### ModCombobox

Themed dropdown/combobox for selecting from a list of options.

```csharp
ModCombobox.Create(options, selectedIndex, newIndex => {
    // Selection changed
}, parent);
```

### ModProgressBar

Visual progress indicator.

```csharp
var progress = ModProgressBar.Create(parent);
progress.SetProgress(0.75f);  // 75%
```

---

## Gotchas and Tips

### Unity Dual-Graphic Conflict

Unity does NOT allow two `Graphic` components (e.g., `Image` + custom `MaskableGraphic`) on the same `GameObject`. If you need both a background image and a custom mesh renderer, put the `MaskableGraphic` on a **child** object.

**Symptom:** `NullReferenceException` on `OnPopulateMesh`

### Runtime Sprite Generation

Unity's `Image.Type.Filled` with `FillMethod.Radial360` requires a **sprite** to render as a circle. Without one, radial fill draws a filled square. The framework generates circle sprites at runtime using `Texture2D` with anti-aliased edges.

### Window Position Persistence

`ModWindow` automatically saves/restores position using `ModSettings`. The key is based on either the `singletonKey` (if provided) or the window title. If you change your window's title, previously saved positions won't apply.

### Layout Groups vs Manual Positioning

All Custom UI widgets are designed for Unity's `LayoutGroup` system.
Do NOT manually set `anchoredPosition` on widgets added to a `ModWindow.ContentPanel` -- the `VerticalLayoutGroup` manages positioning automatically.

If you need manual control, create a raw `GameObject`, add a `RectTransform`, set `LayoutElement.ignoreLayout = true`, and position it yourself.

### Color Tinting Gotcha

Unity's `Button.colors` (ColorBlock) works by **multiplying** the `Image.color` with the `ColorBlock` color.
If your `Image.color` is `Color.clear` (all zeros), the multiplication produces zero regardless of the ColorBlock value -- hover/press effects become invisible.
Always set `Image.color = Color.white` on button backgrounds, then use `ColorBlock.normalColor = Color.clear` for transparency.

### Singleton Windows

Use the `singletonKey` parameter in `ModWindow.Create()` for windows that should only have one instance:

```csharp
// First call creates the window
ModWindow win = ModWindow.Create("Settings", 400, 300, singletonKey: "settings");

// Second call returns the same window and shows it
ModWindow sameWin = ModWindow.Create("Settings", 400, 300, singletonKey: "settings");
// sameWin == win (same object)
```

### Safely Accessing Game Data

Game objects (companies, employees, products) can be garbage collected mid-game. Always null-check:

```csharp
foreach (uint id in trackedIds.ToList()) {
    if (market.Companies.TryGetValue(id, out var company) && company != null && !company.Bankrupt) {
        // Safe to use
    } else {
        trackedIds.Remove(id);  // Cleanup
    }
}
```

---

## Architecture

```
ModFramework/
|-- Core/                          (5 files - Utilities)
|   |-- ModLogger.cs               Buffered logging with severity levels
|   |-- ModEvents.cs               Pub/sub event bus
|   |-- ModSettings.cs             Persistent key-value storage (Base64)
|   |-- ModUtils.cs                General utilities
|   |-- Notifications.cs           In-game toast notifications
|
|-- UI/
|   |-- Vanilla/
|   |   |-- UIHelper.cs            Legacy game-prefab based UI (kept for compat)
|   |
|   |-- Custom/                    (32 files - Custom UI Framework)
|       |
|       |-- [Foundation - 10 files]
|       |   |-- GameTheme.cs       Auto-samples game colors/fonts/sizes
|       |   |-- ModWindow.cs       Draggable, collapsible, pinnable window
|       |   |-- ModWindowRegistry.cs  Singleton tracking, z-order, focus
|       |   |-- ModRefreshDriver.cs   Live refresh MonoBehaviour (ticks callbacks)
|       |   |-- DragHandler.cs     Drag-to-move MonoBehaviour
|       |   |-- HoverHandler.cs    Hover color shift MonoBehaviour
|       |   |-- FocusTracker.cs    Click-to-focus MonoBehaviour
|       |   |-- ResizeHandler.cs   Bottom right drag-to-resize grip
|       |   |-- ModHotkeyRegistry.cs  Centralized keybind manager
|       |   |-- ModHotkeyPoller.cs    Input polling loop
|       |
|       |-- [Widgets - 12 files]
|       |   |-- ModButton.cs       Themed button with hover/press
|       |   |-- ModLabel.cs        Text label + ModHeader (bold section header)
|       |   |-- ModInputField.cs   Single-line input + ModTextArea + ModSearchField + ModNumericInput
|       |   |-- ModToggle.cs       Checkbox + ModSlider
|       |   |-- ModScrollView.cs   Scrollable container
|       |   |-- ModCombobox.cs     Dropdown selector
|       |   |-- ModProgressBar.cs  Visual progress indicator
|       |   |-- ModPanel.cs        Vertical/horizontal layout container
|       |   |-- ModKeybind.cs      Press-any-key hotkey binder
|       |   |-- + 3 more
|       |
|       |-- [Data Views - 6 files]
|       |   |-- ModListView.cs     Generic pooled list with search + pagination
|       |   |-- ModTable.cs        Column-based table built on ModListView
|       |   |-- ModHUD.cs          Screen-edge overlay
|       |   |-- ModDialog.cs       Modal message/confirm dialogs
|       |   |-- ModTooltip.cs      Mouse-follow tooltip
|       |   |-- ModConsoleWindow.cs  Debug log viewer
|       |
|       |-- [Advanced - 9 files]
|           |-- ModBarChart.cs     Horizontal bar chart
|           |-- ModPieChart.cs     Radial fill pie chart with labels
|           |-- ModLineChart.cs    Mesh-based smooth line chart (MaskableGraphic)
|           |-- ModAccordion.cs    Collapsible sections
|           |-- ModContextMenu.cs  Right-click context menu
|           |-- ModSplitPane.cs    Side-by-side split panels
|           |-- ModCardLayout.cs   Grid card layout
|           |-- ModNotificationBadge.cs  Counter badge (attaches to any element)
|           |-- ModNodeGraph.cs    Visual node graph and tech tree (MaskableGraphic edges)
```

---

## Production Examples

These mods in this workspace use ModFramework in production:

| Mod | UI Components Used | Notes |
|-----|-------------------|-------|
| **RivalRadar** | ModWindow, ModListView, ModSearchField, ModPanel, ModScrollView | Full Custom UI showcase. Competitor tracking window with tabs, search, and live data. |
| **AIRevolution** | ModWindow, ModButton, ModLabel, ModToggle, ModSlider | AI automation management center with multiple modules. |
| **FoundersPlus** | Legacy UIHelper (CS mod) | CS script mod, cannot use Custom UI. |
| **ImmortalFounder** | Raw Unity Canvas + ModFramework Core | Custom ScrollRect + RectMask2D for 1200+ employee list. |
| **MegaPlots** | Core only (ModLogger, ModSettings) | No custom UI needed, settings via ModMeta screen. |

**Best reference implementation:** `RivalRadar/RivalRadarWindow.cs` - Shows how to build a complete tabbed window with searchable lists, live data refresh, and hotkey integration using the Custom UI framework.

## Deferred Features / Known Limitations

### Emoji Support (TextMeshPro)
The ModFramework intentionally uses the standard `UnityEngine.UI.Text` component because it perfectly mirrors how the game renders its native UI (via `WindowManager.SpawnLabel`). 

Because of this, **full-color Emojis are not supported**. While the game's code does contain `Unity.TextMeshPro.dll`, switching the framework to `TextMeshProUGUI` would mean we can no longer use the game's built-in `GameFont`. We would have to bundle our own `TMP_FontAsset`, which breaks visual consistency with the base game and creates severe risks of breaking foreign language translations (like Chinese or Russian) if the bundled font doesn't contain those glyphs. 

To ensure maximum stability and localization support, standard `Text` is retained.

---

## Changelog

- **v3.0** (March 2026) - Complete Custom UI system (31 files), replaced legacy UIHelper as primary UI approach, added Resize, Hotkeys, and Node Graphs
- **v2.0** (October 2025) - Core split into 5 files, added UIHelper
- **v1.0** (September 2025) - Initial single-file ModFramework.cs
