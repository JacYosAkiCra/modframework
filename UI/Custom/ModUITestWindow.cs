using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A test window that showcases all custom UI widgets.
/// Open with: ModUITestWindow.Show()
/// Great for verifying theme matching and widget behavior in-game.
/// </summary>
namespace ModFramework.UI.Custom
{
    public static class ModUITestWindow
    {
        /// <summary>
        /// Opens a test window showcasing all widgets.
        /// </summary>
        public static void Show()
        {
            var window = ModWindow.Create("ModFramework UI Test", 1000, 650, "ModUITest_v3");
            var content = window.ContentPanel;

            // Clear old contents if the window was already open and user pressed F8 again!
            foreach (Transform child in content.transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }

            // Use a scroll view since we have many widgets to show
            var scroll = ModScrollView.Create(1200f, content);

            // --- Section: Labels ---
            ModHeader.Create("Labels & Headers", scroll);
            ModLabel.Create("This is a normal label.", scroll);
            ModLabel.Create("This is a bold label.", scroll, true);

            // --- Section: Buttons ---
            ModHeader.Create("Buttons", scroll);
            var btnRow = ModPanel.CreateHorizontal(scroll);
            ModButton.Create("Button A", () => Notifications.Show("Button A clicked!"), btnRow, 120f);
            ModButton.Create("Button B", () => Notifications.Show("Button B clicked!"), btnRow, 120f);
            ModButton.Create("Full Width", () => Notifications.Show("Full width!"), scroll);

            // --- Section: Logger Test ---
            ModHeader.Create("Logger Test (check F11 console)", scroll);
            var logRow = ModPanel.CreateHorizontal(scroll);
            ModButton.Create("Log Samples", () =>
            {
                ModLogger.Log("This is an info message from the test window.");
                ModLogger.LogWarning("This is a warning — something might be off.");
                ModLogger.LogError("This is an error — something went wrong!");
                ModLogger.LogSuccess("This is a success — operation completed.");
                ModLogger.Log("Framework version: ModFramework v2.0");
                ModLogger.LogWarning("Performance: 47 widgets active in 3 windows.");
                Notifications.Show("6 log entries added — press F11 to view!");
            }, logRow, 140f);
            ModButton.Create("Open Console", () => ModConsoleWindow.Show(), logRow, 140f);

            // --- Section: Input Fields ---
            ModHeader.Create("Input Fields", scroll);
            ModLabel.Create("Single-line input:", scroll);
            ModInputField.Create("Type here...", val => Debug.Log("Input: " + val), scroll);

            ModLabel.Create("Search field:", scroll);
            ModSearchField.Create("Search something...", val => Debug.Log("Search: " + val), scroll);

            ModLabel.Create("Numeric input (0-100):", scroll);
            ModNumericInput.Create(50, 0, 100, val => Debug.Log("Number: " + val), scroll);

            // --- Section: Toggle & Slider ---
            ModHeader.Create("Toggle & Slider", scroll);
            ModToggle.Create("Enable feature", true, val => Debug.Log("Toggle: " + val), scroll);
            ModToggle.Create("Another option", false, val => Debug.Log("Toggle2: " + val), scroll);

            ModLabel.Create("Slider (0 to 100):", scroll);
            ModSlider.Create(0f, 100f, 50f, val => Debug.Log("Slider: " + val), scroll);

            // --- Section: Progress Bar ---
            ModHeader.Create("Progress Bar", scroll);
            var progressBar = ModProgressBar.Create(0.65f, scroll);
            ModLabel.Create("65% complete", scroll);

            // --- Section: Dialogs & Tooltips ---
            ModHeader.Create("Dialogs & Tooltips", scroll);
            var dialogBtnRow = ModPanel.CreateHorizontal(scroll);
            ModButton.Create("Show Message", () => ModDialog.ShowMessage("Test", "This is a simple message!"), dialogBtnRow, 120f);
            ModButton.Create("Show Confirm", () => ModDialog.ShowConfirm("Confirm", "Are you sure?", () => ModDialog.ShowMessage("Result", "Confirmed!"), () => ModDialog.ShowMessage("Result", "Cancelled.")), dialogBtnRow, 140f);
            ModButton.Create("Hover Me", () => {}, dialogBtnRow, 120f).AddTooltip("This is a custom ModTooltip!");

            // --- Section: Advanced Data Views ---
            ModHeader.Create("Advanced Data Views", scroll);
            
            // 1. ModListView Test
            ModLabel.Create("ModListView (1000 items, paged + pooled):", scroll);
            GameObject listContainer = new GameObject("ListContainer");
            listContainer.AddComponent<RectTransform>().SetParent(scroll.transform, false);
            LayoutElement listLe = listContainer.AddComponent<LayoutElement>();
            listLe.minHeight = 250f;
            listLe.preferredHeight = 250f;
            
            var listView = ModListView<string>.Create(
                listContainer,
                24f,
                (rowObj) => 
                {
                    HorizontalLayoutGroup hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
                    hlg.childControlWidth = true;
                    hlg.childForceExpandWidth = true;
                    
                    ModLabel.Create("Placeholder", rowObj);
                },
                (item, rowObj) => 
                {
                    rowObj.GetComponentInChildren<UnityEngine.UI.Text>().text = item;
                },
                (item, query) => item.ToLowerInvariant().Contains(query),
                20 // 20 items per page
            );

            System.Collections.Generic.List<string> dummyData = new System.Collections.Generic.List<string>();
            for(int i = 0; i < 1000; i++) dummyData.Add("List Item " + i);
            listView.SetData(dummyData);

            // 2. ModTable Test
            ModLabel.Create("ModTable (3 columns):", scroll);
            GameObject tableContainer = new GameObject("TableContainer");
            tableContainer.AddComponent<RectTransform>().SetParent(scroll.transform, false);
            LayoutElement tableLe = tableContainer.AddComponent<LayoutElement>();
            tableLe.minHeight = 250f;
            tableLe.preferredHeight = 250f;

            var tableColumns = new System.Collections.Generic.List<ModTableColumn<DummyEmp>>
            {
                new ModTableColumn<DummyEmp>
                {
                    HeaderName = "Name",
                    Width = 0, // Flexible
                    OnBindCell = (emp, cell) => { ModLabel.Create(emp.Name, cell); }
                },
                new ModTableColumn<DummyEmp>
                {
                    HeaderName = "Role",
                    Width = 0, // Flexible (even split)
                    OnBindCell = (emp, cell) => { ModLabel.Create(emp.Role, cell); }
                },
                new ModTableColumn<DummyEmp>
                {
                    HeaderName = "Salary",
                    Width = 0, // Flexible (even split)
                    OnBindCell = (emp, cell) => { ModLabel.Create("$" + emp.Salary, cell); }
                }
            };

            var modTable = ModTable<DummyEmp>.Create(
                tableContainer,
                24f,
                tableColumns,
                (emp, query) => emp.Name.ToLowerInvariant().Contains(query) || emp.Role.ToLowerInvariant().Contains(query),
                10
            );

            var empData = new System.Collections.Generic.List<DummyEmp>
            {
                new DummyEmp { Name = "Alice", Role = "Programmer", Salary = 5000 },
                new DummyEmp { Name = "Bob", Role = "Designer", Salary = 4500 },
                new DummyEmp { Name = "Charlie", Role = "Artist", Salary = 4800 },
                new DummyEmp { Name = "Dan", Role = "Programmer", Salary = 5200 },
                new DummyEmp { Name = "Eve", Role = "Lead", Salary = 8000 }
            };
            modTable.SetData(empData);

            // ============================================
            // PHASE 3: Advanced Widgets Demo
            // ============================================

            // --- Section: Bar Chart ---
            ModHeader.Create("Bar Chart", scroll);
            var barChart = ModBarChart.Create(scroll, 150f);
            barChart.SetData(new BarChartEntry[] {
                new BarChartEntry("Marketing", 0.8f, new Color(0.2f, 0.6f, 0.9f)),
                new BarChartEntry("Development", 0.65f, new Color(0.3f, 0.8f, 0.3f)),
                new BarChartEntry("Sales", 0.4f, new Color(0.9f, 0.5f, 0.1f)),
                new BarChartEntry("Support", 0.25f, new Color(0.7f, 0.3f, 0.7f)),
                new BarChartEntry("HR", 0.15f, new Color(0.5f, 0.5f, 0.5f))
            });

            // --- Section: Pie Chart ---
            ModHeader.Create("Pie Chart", scroll);
            var pieChart = ModPieChart.Create(scroll, 160f);
            pieChart.SetData(new PieSlice[] {
                new PieSlice("Windows", 0.55f, new Color(0.2f, 0.5f, 0.9f)),
                new PieSlice("Mac", 0.25f, new Color(0.6f, 0.6f, 0.6f)),
                new PieSlice("Linux", 0.12f, new Color(0.9f, 0.7f, 0.1f)),
                new PieSlice("Mobile", 0.08f, new Color(0.3f, 0.8f, 0.4f))
            });

            // --- Section: Line Chart ---
            ModHeader.Create("Line Chart", scroll);
            var lineChart = ModLineChart.Create(scroll, 180f);
            lineChart.AddSeries("Revenue", new float[] { 10, 25, 40, 35, 60, 55, 80 }, new Color(0.2f, 0.7f, 0.2f));
            lineChart.AddSeries("Costs", new float[] { 15, 20, 30, 28, 45, 40, 50 }, new Color(0.8f, 0.2f, 0.2f));
            lineChart.Rebuild();

            // --- Section: Accordion ---
            ModHeader.Create("Accordion", scroll);
            var accordion = ModAccordion.Create(scroll, true);
            var sec1 = accordion.AddSection("General Settings", true);
            ModLabel.Create("Volume: 80%", sec1);
            ModLabel.Create("Difficulty: Normal", sec1);
            var sec2 = accordion.AddSection("Graphics");
            ModLabel.Create("Resolution: 1920x1080", sec2);
            ModLabel.Create("Quality: High", sec2);
            var sec3 = accordion.AddSection("Controls");
            ModLabel.Create("Mouse sensitivity: 5.0", sec3);
            ModLabel.Create("Invert Y: No", sec3);

            // --- Section: Context Menu ---
            ModHeader.Create("Context Menu", scroll);
            var contextItems = new ContextMenuItem[] {
                new ContextMenuItem("Copy", () => Notifications.Show("Copied!")),
                new ContextMenuItem("Paste", () => Notifications.Show("Pasted!")),
                ContextMenuItem.CreateSeparator(),
                new ContextMenuItem("Delete", () => Notifications.Show("Deleted!"), new Color(0.8f, 0.2f, 0.2f))
            };
            var ctxBtn = ModButton.Create("Right-click or left-click me", () =>
            {
                ModContextMenu.Show(contextItems);
            }, scroll);
            // Also bind right-click
            ModContextMenu.Bind(ctxBtn, contextItems);

            // --- Section: Split Pane ---
            ModHeader.Create("Split Pane (drag divider)", scroll);
            var splitPane = ModSplitPane.Create(scroll, 0.4f, 120f);
            ModLabel.Create("Left panel content", splitPane.LeftPanel);
            ModButton.Create("Left Button", () => Notifications.Show("Left!"), splitPane.LeftPanel, 100f);
            ModLabel.Create("Right panel content", splitPane.RightPanel);
            ModButton.Create("Right Button", () => Notifications.Show("Right!"), splitPane.RightPanel, 100f);

            // --- Section: Card Layout ---
            ModHeader.Create("Card Layout", scroll);
            var cards = ModCardLayout.Create(scroll, 3, 100f);
            cards.AddCard("Product A", "Rev: $1.2M", () => Notifications.Show("Product A selected"));
            cards.AddCard("Product B", "Rev: $800K", () => Notifications.Show("Product B selected"));
            cards.AddCard("Product C", "Rev: $2.5M", () => Notifications.Show("Product C selected"));

            // --- Section: Notification Badge ---
            ModHeader.Create("Notification Badge", scroll);
            var badgeRow = ModPanel.CreateHorizontal(scroll);
            ModButton.Create("Inbox", () => Notifications.Show("Opening inbox..."), badgeRow, 120f);
            // Attach badge to the button we just created
            var inboxBtn = badgeRow.transform.GetChild(badgeRow.transform.childCount - 1).gameObject;
            var badge = ModNotificationBadge.Create(inboxBtn, 7);
            ModButton.Create("Add +1", () => badge.Increment(), badgeRow, 80f);
            ModButton.Create("Clear", () => badge.SetCount(0), badgeRow, 80f);

            // --- Section: Window Resize ---
            ModHeader.Create("Window Resizing", scroll);
            ModLabel.Create("Drag the small grip icon in the bottom-right corner to resize this window. Notice how all the LayoutGroups automatically reflow the content to fit the new size. The live size is automatically saved to ModSettings.", scroll, true);

            // --- Section: Global Hotkeys ---
            ModHeader.Create("Centralized Hotkeys", scroll);
            ModLabel.Create("ModHotkeyRegistry manages global binds. Try pressing 'H' to test.", scroll);
            var hkLbl = ModLabel.Create("Last key pressed: none", scroll);
            ModHotkeyRegistry.Register("ui_test.hotkey", "Test Hotkey", KeyCode.H, () => {
                hkLbl.text = "Last key pressed: H (at " + System.DateTime.Now.ToLongTimeString() + ")";
            });

            // --- Section: Node / Skill Tree Graph ---
            ModHeader.Create("Node / Skill Tree Graph", scroll);
            var graphContainer = ModPanel.Create(scroll);
            graphContainer.AddComponent<LayoutElement>().preferredHeight = 350f;
            
            var graph = ModNodeGraph.Create(graphContainer, -1f, 350f);
            
            // Define nodes
            graph.AddNode(new NodeGraphNode { Id = "engine", Label = "Game Engine", Color = new Color(0.2f, 0.5f, 0.8f) });
            graph.AddNode(new NodeGraphNode { Id = "render", Label = "Rendering" });
            graph.AddNode(new NodeGraphNode { Id = "phys", Label = "Physics" });
            graph.AddNode(new NodeGraphNode { Id = "audio", Label = "Audio" });
            graph.AddNode(new NodeGraphNode { Id = "shader", Label = "Shaders" });
            graph.AddNode(new NodeGraphNode { Id = "particle", Label = "Particles" });
            
            // Interactivity
            foreach (var node in graph.GetType().GetField("_nodes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(graph) as System.Collections.Generic.List<NodeGraphNode>)
            {
                var n = node; // local copy for closure
                n.OnClick = () => Notifications.Show("Clicked node: " + n.Label);
            }

            // Edges (directional)
            graph.AddEdge("engine", "render");
            graph.AddEdge("engine", "phys");
            graph.AddEdge("engine", "audio");
            graph.AddEdge("render", "shader");
            graph.AddEdge("render", "particle");

            // Build layout and visuals
            graph.AutoLayoutTree();
            graph.Rebuild();

            // === INITIALIZE LIVE REFRESH ===
            // Must be done BEFORE CreateLive() calls so the driver exists in hierarchy
            window.SetRefreshInterval(2f);  // Tick every 2 seconds

            // --- Section: Live Data (auto-refreshing) ---
            ModHeader.Create("Live Data (auto-refreshing every 2s)", scroll);
            ModLabel.Create("These labels update in-place without any flicker:", scroll);

            int tickCounter = 0;
            window.OnRefresh(() => tickCounter++);

            ModLabel.CreateLive(() => "Refresh ticks: " + tickCounter, scroll, true);
            ModLabel.CreateLive(() => "Game time: " + System.DateTime.Now.ToString("HH:mm:ss"), scroll);
            ModLabel.CreateLive(() => "Random value: " + UnityEngine.Random.Range(0, 1000).ToString(), scroll);

            ModLabel.Create("Live progress bar (sine wave):", scroll);
            ModProgressBar.CreateLive(() => {
                return (Mathf.Sin(Time.time * 0.5f) + 1f) / 2f; // 0-1 oscillating
            }, scroll);

            // Force immediate first tick so labels are populated right away
            window.RefreshNow();

            window.Show();
        }

        private class DummyEmp
        {
            public string Name;
            public string Role;
            public int Salary;
        }
    }
}
