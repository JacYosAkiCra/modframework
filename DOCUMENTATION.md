# ModFramework Documentation

Full API reference, setup guide, and project structure for ModFramework v4.1.

---

## Project Structure

```
ModFramework/
|-- ModFramework.cs                 Module index / version header
|-- ModFramework.csproj             Project file
|-- README.md                       High-level overview
|-- DOCUMENTATION.md                This file - full API reference
|-- LICENSE                         MIT License (ModFramework)
|
|-- Core/                           (8 files - Utilities & Infrastructure)
|   |-- ModLogger.cs                Buffered logging with severity levels
|   |-- ModEvents.cs                Pub/sub event bus
|   |-- ModSettings.cs              Persistent key-value storage (Base64)
|   |-- ModUtils.cs                 General utilities
|   |-- Notifications.cs            In-game toast notifications
|   |-- ModLifecycle.cs             Mod activation/deactivation lifecycle hooks
|   |-- ModPatching.cs              Runtime method patching helpers (uses Harmony)
|   |-- ModSafety.cs                Error handling and safety wrappers
|
|-- GameData/                       (4 files - Game Data Helpers)
|   |-- ModCompanyHelper.cs         Company data access and utilities
|   |-- ModEmployeeHelper.cs        Employee data access and utilities
|   |-- ModMarketHelper.cs          Market/industry data access and utilities
|   |-- ModProductHelper.cs         Product data access and utilities
|
|-- Harmony/                        (Bundled dependency - no separate install needed)
|   |-- 0Harmony.dll                Harmony 2.4.1 runtime patching library
|   |-- LICENSE                     MIT License (Andreas Pardeike)
|
|-- UI/
|   |-- Vanilla/
|   |   |-- UIHelper.cs             Legacy game-prefab based UI (kept for compat)
|   |
|   |-- Custom/                     (35 files - Custom UI Framework)
|       |
|       |-- [Foundation]
|       |   |-- GameTheme.cs        Auto-samples game colors/fonts/sizes
|       |   |-- ModWindow.cs        Draggable, collapsible, pinnable window
|       |   |-- ModWindowRegistry.cs   Singleton tracking, z-order, focus
|       |   |-- ModRefreshDriver.cs    Live refresh MonoBehaviour (ticks callbacks)
|       |   |-- DragHandler.cs      Drag-to-move MonoBehaviour
|       |   |-- HoverHandler.cs     Hover color shift MonoBehaviour
|       |   |-- FocusTracker.cs     Click-to-focus MonoBehaviour
|       |   |-- ResizeHandler.cs    Bottom right drag-to-resize grip
|       |   |-- ModHotkeyRegistry.cs   Centralized keybind manager
|       |   |-- ModHotkeyPoller.cs     Input polling loop
|       |
|       |-- [Widgets]
|       |   |-- ModButton.cs        Themed button with hover/press
|       |   |-- ModLabel.cs         Text label + ModHeader (bold section header)
|       |   |-- ModInputField.cs    Single-line input + ModTextArea + ModSearchField + ModNumericInput
|       |   |-- ModToggle.cs        Checkbox + ModSlider
|       |   |-- ModScrollView.cs    Scrollable container
|       |   |-- ModCombobox.cs      Dropdown selector
|       |   |-- ModProgressBar.cs   Visual progress indicator
|       |   |-- ModPanel.cs         Vertical/horizontal layout container
|       |   |-- ModKeybind.cs       Press-any-key hotkey binder
|       |
|       |-- [Data Views]
|       |   |-- ModListView.cs      Generic pooled list with search + pagination
|       |   |-- ModTable.cs         Column-based table built on ModListView
|       |   |-- ModHUD.cs           Screen-edge overlay
|       |   |-- ModDialog.cs        Modal message/confirm dialogs
|       |   |-- ModTooltip.cs       Mouse-follow tooltip
|       |   |-- ModConsoleWindow.cs Debug log viewer
|       |
|       |-- [Advanced]
|           |-- ModBarChart.cs      Horizontal bar chart
|           |-- ModPieChart.cs      Radial fill pie chart with labels
|           |-- ModLineChart.cs     Mesh-based smooth line chart (MaskableGraphic)
|           |-- ModAccordion.cs     Collapsible sections
|           |-- ModContextMenu.cs   Right-click context menu
|           |-- ModSplitPane.cs     Side-by-side split panels
|           |-- ModCardLayout.cs    Grid card layout
|           |-- ModNotificationBadge.cs  Counter badge (attaches to any element)
|           |-- ModNodeGraph.cs     Visual node graph and tech tree (MaskableGraphic edges)
|           |-- ModUITestWindow.cs  Built-in test/demo window
|
|-- Scaffolding/                    (Mod Generator)
|   |-- CreateMod.ps1               PowerShell script to scaffold a new mod project using PowerShell
|   |-- CreateModGUI.ps1            PowerShell script to scaffold a new mod project using GUI
    |-- Templates/
|       |-- MainBehaviour.cs_template
|       |-- Mod.csproj_template
        |-- ModFramework.csproj_template
|       |-- ModMeta.json_template
|       |-- meta.tyd_template
```

---

## Getting Started - Create a New Mod

The fastest way to create a new mod is with the scaffolding tools. They generate a complete, ready-to-build mod project with all references pre-configured.

### Option 1: CLI (CreateMod.ps1)

### Help
The below command will display parameters, usage, and examples.

```powershell
.\ModFramework\Scaffolding\CreateMod.ps1 -help
```

### First Run (provide your game directory)

```powershell
.\ModFramework\Scaffolding\CreateMod.ps1 -ModName "MyAwesomeMod" -GameDir "E:\SteamLibrary\steamapps\common\Software Inc"
```

The game directory path is validated (it checks for `Assembly-CSharp.dll`) and cached in `.game-directory` so you only need to provide it once.

### Subsequent Runs

```powershell
.\ModFramework\Scaffolding\CreateMod.ps1 -ModName "AnotherMod"
```

### Option 2: GUI (CreateModGUI.ps1)

Launch the GUI from the repository root:

```powershell
.\ModFramework\Scaffolding\CreateModGUI.ps1
```

Or, if you are already inside `Scaffolding/`:

```powershell
.\CreateModGUI.ps1
```

#### GUI Steps

1. Enter **Mod Name** (valid C# identifier).
2. Set **Game Directory** (or click **Browse Game Dir**).
3. Optionally set **Mod Directory** (or click **Browse Mod Dir**).
4. Optionally check **Build ModFramework immediately (-Build)**.
5. Click **Run CreateMod** and monitor **CLI Output**.

#### GUI Help Menu

- Open **Help -> Usage** (or press **F1**) to view GUI instructions.
- Open **Help -> About** for a quick summary of the GUI tool.

### What It Generates

```
YourMod/
|-- YourModBehaviour.cs      Main mod class with lifecycle hooks
|-- YourMod.csproj           Pre-configured project file (references game DLLs + ModFramework)
|-- ModMeta.json             Mod metadata for Software Inc
|-- meta.tyd                 Mod discovery file
```

The generated `.csproj` includes a post-build event that automatically copies the compiled DLL and metadata to the game's mod folder.

### Build and Test

1. Add the generated `.csproj` to your Visual Studio solution
2. Build the solution (Ctrl+Shift+B)
3. Launch Software Inc and enable your mod in the mod menu

---

## Harmony (Bundled)

ModFramework bundles **Harmony 2.4.1** (`Harmony/0Harmony.dll`) so you do not need to install it separately. The `ModPatching` helper in `Core/` wraps Harmony for common use cases like prefix/postfix patches.

Harmony is licensed under the MIT License by Andreas Pardeike. See `Harmony/LICENSE` for details.

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
|-- Core/                          (8 files - Utilities)
|   |-- ModLogger.cs               Buffered logging with severity levels
|   |-- ModEvents.cs               Pub/sub event bus
|   |-- ModSettings.cs             Persistent key-value storage (Base64)
|   |-- ModUtils.cs                General utilities
|   |-- Notifications.cs           In-game toast notifications
|   |-- ModLifecycle.cs            Safe game lifecycle hooks (v4)
|   |-- ModSafety.cs               Error safety wrappers (v4)
|   |-- ModPatching.cs             Harmony patch helpers (v4)
|
|-- GameData/                      (4 files - Safe Data Wrappers, v4)
|   |-- ModCompanyHelper.cs        Company data (player, rivals, revenue)
|   |-- ModProductHelper.cs        Product data (type, quality, bugs)
|   |-- ModEmployeeHelper.cs       Employee and team data
|   |-- ModMarketHelper.cs         Market state, dates, game speed
|
|-- Scaffolding/                   (Project generator, v4)
|   |-- CreateMod.ps1              PowerShell script to generate new mods
|   |-- Templates/
|       |-- MainBehaviour.cs_template
|       |-- Mod.csproj_template
|       |-- ModMeta.json_template
|       |-- meta.tyd_template
|
|-- UI/
|   |-- Vanilla/
|   |   |-- UIHelper.cs            Legacy game-prefab based UI (kept for compat)
|   |
|   |-- Custom/                    (35 files - Custom UI Framework)
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




## ModFramework v4 - Accessible DLL Modding

v4 introduces tools that make DLL modding accessible to developers who have never touched the game's internals. You do not need to open dnSpy, read decompiled code, or understand the game's internal class hierarchy. Everything is wrapped in safe, easy-to-use helper methods.

### What is new in v4?

| Feature | What it solves |
|---------|---------------|
| **Scaffolding** | Creates a ready-to-build mod project in one command |
| **Game Data Wrappers** | Read company, product, employee, and market data safely |
| **Lifecycle Hooks** | Know exactly when the game is ready, when a day passes, etc. |
| **Error Safety** | Prevents your mod from crashing the entire game |
| **Harmony Helpers** | Apply code patches with a single line |

---

### Scaffolding - Create a New Mod in 30 Seconds

**What is this?** A PowerShell script that generates an entire mod project for you, with all the references, build settings, and deployment automation already configured.

**How to use it:**

1. Open PowerShell (press Win+R, type `powershell`, press Enter)
2. Navigate to your repository folder:
   ```powershell
   cd "C:\Users\YourName\Documents\Visual Studio 2022\SoftwareIncMods\SoftwareIncMods"
   ```
3. Run the scaffolding script:
   ```powershell
   .\ModFramework\Scaffolding\CreateMod.ps1 -ModName "MyAwesomeMod"
   ```
4. Open Visual Studio, right-click your Solution, choose "Add Existing Project", and select `MyAwesomeMod\MyAwesomeMod.csproj`
5. Build the project (Ctrl+Shift+B). The DLL is automatically copied to your game's Mods folder.

**What gets generated:**

| File | Purpose |
|------|---------|
| `MyAwesomeModBehaviour.cs` | Your mod's main entry point, with step-by-step comments |
| `MyAwesomeMod.csproj` | Pre-configured project file with all DLL references |
| `ModMeta.json` | Metadata file describing your mod |
| `meta.tyd` | Required by Software Inc for mod discovery in the mod menu |

The generated `.csproj` includes a **post-build event** that automatically copies your compiled DLL and `ModMeta.json` to the game's local mods folder. You just hit Build, then launch the game.

---

### Game Data Wrappers - Read Game Data Without Crashing

**What is this?** Four helper classes that let you safely read game simulation data (companies, products, employees, market info) without worrying about null references, missing data, or crashes.

**Why do you need this?** Without these wrappers, reading the player's company looks like this:

```csharp
// WITHOUT wrappers (risky, can crash if any part is null)
Company player = GameSettings.Instance.MyCompany;
float cash = (float)player.Money;
```

With wrappers, the same code is completely safe:

```csharp
// WITH wrappers (safe, returns 0 if anything is wrong)
float cash = ModCompanyHelper.GetPlayerCash();
```

#### ModCompanyHelper

Provides safe access to company data.

```csharp
using ModFramework.GameData;

// Get the player's company object (returns null if not in a game)
Company myCompany = ModCompanyHelper.GetPlayerCompany();

// Get the player's cash balance (returns 0 if not in a game)
float cash = ModCompanyHelper.GetPlayerCash();

// Get all active AI companies (excludes bankrupt ones)
List<SimulatedCompany> rivals = ModCompanyHelper.GetActiveCompanies();

// Find a company by name (case-insensitive)
SimulatedCompany target = ModCompanyHelper.FindByName("Macrosoft");

// Check if a company is bankrupt
bool broke = ModCompanyHelper.IsBankrupt(someCompany);

// Get companies sorted by revenue (richest first)
List<SimulatedCompany> leaderboard = ModCompanyHelper.GetByRevenue();

// Check if the player is currently in a game (not on main menu)
bool inGame = ModCompanyHelper.IsInGame();
```

#### ModProductHelper

Provides safe access to software product data.

```csharp
using ModFramework.GameData;

// Get all products the player has released
List<SoftwareProduct> myProducts = ModProductHelper.GetPlayerProducts();

// Get ALL products on the market (from every company)
List<SoftwareProduct> allProducts = ModProductHelper.GetAllProducts();

// Get all products of a specific type (e.g. "Operating System")
List<SoftwareProduct> osList = ModProductHelper.GetByType("Operating System");

// Get details about a specific product
string typeName = ModProductHelper.GetTypeName(product);      // "Operating System"
string category = ModProductHelper.GetCategoryName(product);  // "Business"
float quality = ModProductHelper.GetQuality(product);         // 0.0 to 1.0
int bugs = ModProductHelper.GetBugCount(product);             // number of bugs
string name = ModProductHelper.GetName(product);              // product name
Company dev = ModProductHelper.GetDeveloper(product);         // who made it
```

#### ModEmployeeHelper

Provides safe access to employee and team data.

```csharp
using ModFramework.GameData;

// Get all employees working for the player
List<Actor> employees = ModEmployeeHelper.GetPlayerEmployees();

// Get employee count (quick shortcut)
int headcount = ModEmployeeHelper.GetPlayerEmployeeCount();

// Get all teams in the player's company
List<Team> teams = ModEmployeeHelper.GetPlayerTeams();

// Get an employee's name
string name = ModEmployeeHelper.GetName(someActor);

// Find which team an employee belongs to
string teamName = ModEmployeeHelper.GetTeamName(someActor);
```

#### ModMarketHelper

Provides safe access to market and game-state information.

```csharp
using ModFramework.GameData;

// Get the current in-game date
SDateTime today = ModMarketHelper.GetCurrentDate();

// Get the current game speed (1 = normal, 2 = fast, etc.)
int speed = ModMarketHelper.GetGameSpeed();

// Check if the game is paused
bool paused = ModMarketHelper.IsPaused();

// Get all software types in the game (Operating System, Antivirus, etc.)
List<SoftwareType> types = ModMarketHelper.GetSoftwareTypes();

// Get all software categories across all types (Business, Home, etc.)
List<SoftwareCategory> categories = ModMarketHelper.GetCategories();

// Format a money value into a readable string
string display = ModMarketHelper.FormatMoney(2500000);  // "$2.5M"
```

---

### Lifecycle Hooks - Know When Things Happen

**What is this?** A set of events you can subscribe to that fire at specific moments in the game. Instead of writing complex `Update()` loops that check game state every frame, you simply tell ModFramework "call my method when the game is ready" or "call my method every day."

**Why do you need this?** Without lifecycle hooks, you have to manually check if the game has loaded every single frame. With hooks, the framework does this for you.

```csharp
using ModFramework.Core;

// In your mod's Awake() method:
ModLifecycle.OnGameReady += () => {
    // This code runs ONCE when the player loads a save or starts a new game.
    // It is safe to read any game data here.
    ModLogger.Log("The game is loaded! Player company: " + ModCompanyHelper.GetPlayerCompany()?.Name);
};

ModLifecycle.OnGameExit += () => {
    // This code runs when the player goes back to the main menu.
    // Clean up your windows, caches, or state here.
    ModLogger.Log("Player left the game.");
};

ModLifecycle.OnDayPassed += () => {
    // This code runs once every in-game day at midnight.
    // Great for periodic checks without burning CPU in Update().
    var date = ModMarketHelper.GetCurrentDate();
    if (date.Day == 1) {
        ModLogger.Log("New month started!");
    }
};

ModLifecycle.OnMonthPassed += () => {
    // This code runs once at the start of each in-game month.
    int employees = ModEmployeeHelper.GetPlayerEmployeeCount();
    ModLogger.Log("Monthly report: " + employees + " employees on payroll.");
};

ModLifecycle.OnYearPassed += () => {
    // This code runs once at the start of each in-game year.
    ModLogger.Log("Happy New Year!");
};
```

---

### Error Safety - Never Crash the Game

**What is this?** Utility methods that wrap your code in try/catch blocks so that if your mod encounters an error, the game keeps running normally. The error is logged to the console instead of freezing everything.

#### ModSafety.Try()

Wrap any risky operation. Returns `true` if it ran successfully, `false` if it crashed.

```csharp
using ModFramework.Core;

// Basic usage: wrap risky code
bool success = ModSafety.Try(() => {
    var companies = ModCompanyHelper.GetActiveCompanies();
    BuildLeaderboardUI(companies);
}, "Building Leaderboard");

if (!success) {
    ModLogger.LogWarning("Leaderboard failed to load, using cached data.");
}
```

#### ModSafety.TryGet()

Same as Try, but for functions that return a value. If it crashes, returns a fallback value.

```csharp
// Get player cash safely (returns 0 if anything goes wrong)
float cash = ModSafety.TryGet(() => ModCompanyHelper.GetPlayerCash(), 0f, "Getting Cash");
```

#### ModSafety.ThrottledErrorHandler()

Wraps an action so it only logs errors **once** instead of spamming every frame. Essential for code that runs in Update() loops.

```csharp
private Action _safeUpdate;

void Awake() {
    _safeUpdate = ModSafety.ThrottledErrorHandler("MyMod Update", () => {
        // This code runs every frame, but if it crashes,
        // it only logs the error once and then stops trying.
        UpdateDashboard();
    });
}

void Update() {
    _safeUpdate();
}
```

#### ModSafety.Assert()

Quick sanity check during development. Logs a warning if a condition is false. Does NOT throw an exception.

```csharp
var company = ModCompanyHelper.GetPlayerCompany();
ModSafety.Assert(company != null, "Expected the player's company to exist");
// If company is null, you'll see: "[ModFramework] ASSERT FAILED: Expected the player's company to exist"
```

---

### Harmony Helpers - Patch Game Methods Safely

**What is this?** Harmony is a library that lets you modify ("patch") the game's methods without changing the game's actual files. ModFramework wraps Harmony so you can apply patches with minimal boilerplate.

**Before you start:** Make sure `0Harmony.dll` is referenced in your `.csproj`. If you used the scaffolding script, it is already included.

#### Basic Usage (Attribute-Based Patching)

The recommended approach is to use Harmony's `[HarmonyPatch]` attributes on your patch classes, then call `ModPatching.PatchAll()` to apply them all at once:

```csharp
using ModFramework.Core;
using HarmonyLib;

// In your mod's Awake():
if (ModPatching.IsHarmonyAvailable()) {
    ModPatching.PatchAll("com.yourname.mymod");
} else {
    ModLogger.LogWarning("Harmony is not available. Patches will not be applied.");
}

// Define your patches as separate classes:
[HarmonyPatch(typeof(SomeGameClass), "SomeMethod")]
public class MyPatch
{
    static void Postfix()
    {
        // This runs AFTER SomeGameClass.SomeMethod() finishes
        ModLogger.Log("SomeMethod was called!");
    }
}
```

#### Cleanup

Remove all your patches when the mod is deactivated:

```csharp
ModLifecycle.OnGameExit += () => {
    ModPatching.UnpatchAll("com.yourname.mymod");
};
```

---

### Putting It All Together - Complete Example

Here is a complete minimal mod that uses all v4 features:

```csharp
using System;
using UnityEngine;
using ModFramework.Core;
using ModFramework.GameData;

namespace MyFirstMod
{
    public class MyFirstModBehaviour : ModBehaviour
    {
        private void Awake()
        {
            ModLogger.SetPrefix("MyFirstMod");
            ModSettings.SetPrefix("MyFirstMod");

            // Subscribe to lifecycle events
            ModLifecycle.OnGameReady += OnGameReady;
            ModLifecycle.OnMonthPassed += OnMonthPassed;

            ModLogger.Log("Mod loaded!");
        }

        private void OnGameReady()
        {
            // Safely read company data
            ModSafety.Try(() => {
                var company = ModCompanyHelper.GetPlayerCompany();
                float cash = ModCompanyHelper.GetPlayerCash();
                int employees = ModEmployeeHelper.GetPlayerEmployeeCount();

                ModLogger.Log("Company: " + company?.Name);
                ModLogger.Log("Cash: " + ModMarketHelper.FormatMoney(cash));
                ModLogger.Log("Employees: " + employees);
            }, "Initial Company Report");
        }

        private void OnMonthPassed()
        {
            // Monthly competitor scan
            var rivals = ModCompanyHelper.GetActiveCompanies();
            ModLogger.Log("Active competitors: " + rivals.Count);
        }
    }
}
```

---

## Changelog

- **v4.1** (April 2026) - Bundled Harmony 2.4.1 DLL (no separate install needed), updated scaffolding with `-GameDir` parameter and path validation, replaced all hardcoded game paths with `{GAME_DIRECTORY}` tokens
- **v4.0** (March 2026) - Accessible DLL modding: Game Data Wrappers, Lifecycle Hooks, Error Safety, Harmony Helpers, Project Scaffolding
- **v3.0** (March 2026) - Complete Custom UI system (31 files), replaced legacy UIHelper as primary UI approach, added Resize, Hotkeys, and Node Graphs
- **v2.0** (October 2025) - Core split into 5 files, added UIHelper
- **v1.0** (September 2025) - Initial single-file ModFramework.cs
