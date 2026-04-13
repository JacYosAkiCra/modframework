using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModFramework.UI.Custom
{
    public class ModTableColumn<T>
    {
        public string HeaderName;
        public float Width; // Use 0 for flexible width, >0 for fixed pixel width
        public Action<T, GameObject> OnBindCell;
        public Func<T, IComparable> SortKey; // Optional sorting key
    }

    /// <summary>
    /// A high-performance table view built on top of ModListView. 
    /// Generates headers automatically and distributes columns according to predefined widths.
    /// </summary>
    public class ModTable<T>
    {
        public ModListView<T> ListView { get; private set; }
        public GameObject Root { get; private set; }

        private List<ModTableColumn<T>> _columns;
        private List<Text> _headerTexts = new List<Text>();
        private int _sortColumnIndex = -1;
        private bool _sortAscending = true;

        private ModTable() { }

        public static ModTable<T> Create(
            GameObject parent,
            float rowHeight,
            List<ModTableColumn<T>> columns,
            Func<T, string, bool> onFilter = null,
            int itemsPerPage = 50)
        {
            ModTable<T> table = new ModTable<T>();
            table._columns = columns;

            // Root panel to hold Header + ListView
            table.Root = ModPanel.Create(parent);
            
            VerticalLayoutGroup rootLayout = table.Root.GetComponent<VerticalLayoutGroup>();
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandHeight = false;
            
            LayoutElement tableRootLe = table.Root.AddComponent<LayoutElement>();
            tableRootLe.flexibleHeight = 1f;
            tableRootLe.flexibleWidth = 1f;

            // Build Headers
            GameObject headerRow = ModPanel.CreateHorizontal(table.Root);
            LayoutElement headerLe = headerRow.GetComponent<LayoutElement>();
            headerLe.minHeight = 40f;
            headerLe.preferredHeight = 40f;
            headerRow.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(4, 4, 4, 4);
            headerRow.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.1f); // Subtle background for headers
            
            for (int i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                int colIndex = i; // capture index for closure
                Text headerText = ModHeader.Create(col.HeaderName, headerRow);
                headerText.alignment = TextAnchor.MiddleLeft;
                table._headerTexts.Add(headerText);
                
                // If sortable, add button behavior
                if (col.SortKey != null)
                {
                    // Convert the header into a clickable button
                    Button headerBtn = headerText.gameObject.AddComponent<Button>();
                    
                    // Create a child object for the hit box to avoid Graphic conflicts
                    GameObject hitBoxObj = new GameObject("HitBox");
                    RectTransform hitBoxRect = hitBoxObj.AddComponent<RectTransform>();
                    hitBoxRect.SetParent(headerText.transform, false);
                    hitBoxRect.anchorMin = Vector2.zero;
                    hitBoxRect.anchorMax = Vector2.one;
                    hitBoxRect.offsetMin = Vector2.zero;
                    hitBoxRect.offsetMax = Vector2.zero;
                    
                    Image hitBox = hitBoxObj.AddComponent<Image>();
                    hitBox.color = Color.clear;
                    headerBtn.targetGraphic = hitBox;
                    
                    ColorBlock cb = headerBtn.colors;
                    cb.normalColor = Color.clear;
                    cb.highlightedColor = new Color(0, 0, 0, 0.1f);
                    cb.pressedColor = new Color(0, 0, 0, 0.2f);
                    cb.colorMultiplier = 1f;
                    headerBtn.colors = cb;
                    
                    headerBtn.onClick.AddListener(() => table.SortByColumn(colIndex));
                }
                
                // ModHeader.Create already adds a LayoutElement - use it, don't add a duplicate!
                LayoutElement le = headerText.gameObject.GetComponent<LayoutElement>();
                if (col.Width > 0)
                {
                    le.minWidth = col.Width;
                    le.preferredWidth = col.Width;
                    le.flexibleWidth = 0;
                }
                else
                {
                    le.preferredWidth = 0; // Force zero base so distribution is purely by flexibleWidth
                    le.flexibleWidth = 1;
                }
            }

            // Build underlying ListView
            table.ListView = ModListView<T>.Create(
                table.Root,
                rowHeight,
                // onCreateRow: create horizontal cells per column
                (rowObj) => 
                {
                    HorizontalLayoutGroup hlg = rowObj.AddComponent<HorizontalLayoutGroup>();
                    hlg.childControlWidth = true;
                    hlg.childControlHeight = true;
                    hlg.childForceExpandWidth = false;
                    hlg.childForceExpandHeight = true;
                    hlg.spacing = GameTheme.SectionSpacing;
                    hlg.padding = new RectOffset(0, 0, 0, 0);
                    
                    LayoutElement rowLe = rowObj.GetComponent<LayoutElement>();
                    if (rowLe == null) rowLe = rowObj.AddComponent<LayoutElement>();
                    rowLe.minHeight = rowHeight;
                    rowLe.preferredHeight = rowHeight;

                    // Create cell containers
                    for (int i = 0; i < table._columns.Count; i++)
                    {
                        var col = table._columns[i];
                        GameObject cell = new GameObject("Cell_" + i);
                        RectTransform cellRect = cell.AddComponent<RectTransform>();
                        cellRect.SetParent(rowObj.transform, false);
                        
                        // Layout group so child labels fill the cell properly
                        HorizontalLayoutGroup cellHlg = cell.AddComponent<HorizontalLayoutGroup>();
                        cellHlg.childAlignment = TextAnchor.MiddleLeft;
                        cellHlg.childControlWidth = true;
                        cellHlg.childControlHeight = true;
                        cellHlg.childForceExpandWidth = true;
                        cellHlg.childForceExpandHeight = true;

                        LayoutElement cellLe = cell.AddComponent<LayoutElement>();
                        if (col.Width > 0)
                        {
                            cellLe.minWidth = col.Width;
                            cellLe.preferredWidth = col.Width;
                            cellLe.flexibleWidth = 0;
                        }
                        else
                        {
                            cellLe.preferredWidth = 0; // Match header: zero base, distribute purely by flexibleWidth
                            cellLe.flexibleWidth = 1;
                        }
                    }
                },
                // onBindRow: dispatch to column binders
                (item, rowObj) => 
                {
                    for (int i = 0; i < table._columns.Count; i++)
                    {
                        if (i < rowObj.transform.childCount)
                        {
                            Transform cellTransform = rowObj.transform.GetChild(i);
                            // Prevent GameObject leak: clear old bind contents before rebinding
                            for (int c = cellTransform.childCount - 1; c >= 0; c--)
                            {
                                GameObject oldChild = cellTransform.GetChild(c).gameObject;
                                oldChild.transform.SetParent(null, false);
                                GameObject.Destroy(oldChild);
                            }
                            table._columns[i].OnBindCell(item, cellTransform.gameObject);
                        }
                    }
                },
                onFilter,
                itemsPerPage
            );

            // Strip the ListView's internal padding - table.Root already provides padding,
            // so the extra WindowPadding from ListView's ModPanel would misalign data vs headers.
            VerticalLayoutGroup listVlg = table.ListView.Root.GetComponent<VerticalLayoutGroup>();
            if (listVlg != null)
                listVlg.padding = new RectOffset(0, 0, 0, 0);

            // Make list view expand
            LayoutElement listLe = table.ListView.Root.GetComponent<LayoutElement>();
            if (listLe == null) listLe = table.ListView.Root.AddComponent<LayoutElement>();
            listLe.flexibleHeight = 1f;

            return table;
        }

        public void SetData(IEnumerable<T> items)
        {
            ListView.SetData(items);
        }

        public void SortByColumn(int columnIndex, bool? ascending = null)
        {
            if (columnIndex < 0 || columnIndex >= _columns.Count) return;
            var col = _columns[columnIndex];
            if (col.SortKey == null) return;

            if (ascending.HasValue)
            {
                _sortAscending = ascending.Value;
            }
            else
            {
                // Toogle if clicking same column, else default to descending first (usually want highest sales/qty first)
                if (_sortColumnIndex == columnIndex)
                    _sortAscending = !_sortAscending;
                else
                    _sortAscending = false; 
            }

            _sortColumnIndex = columnIndex;

            // Update Header UI
            for (int i = 0; i < _columns.Count; i++)
            {
                string baseName = _columns[i].HeaderName;
                if (i == _sortColumnIndex)
                {
                    _headerTexts[i].text = baseName + (_sortAscending ? " ▲" : " ▼");
                }
                else
                {
                    _headerTexts[i].text = baseName;
                }
            }

            // Apply Sort to ListView
            ListView.SetSort((a, b) =>
            {
                var valA = col.SortKey(a);
                var valB = col.SortKey(b);
                if (valA == null && valB == null) return 0;
                if (valA == null) return 1;
                if (valB == null) return -1;

                int result = valA.CompareTo(valB);
                return _sortAscending ? result : -result;
            });
        }
    }
}
