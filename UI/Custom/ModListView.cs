using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModFramework.UI.Custom
{
    /// <summary>
    /// A high-performance, generic list view that supports object pooling, 
    /// built-in search filtering, and pagination to handle hundreds of items smoothly.
    /// </summary>
    /// <typeparam name="T">The data type of the items in the list.</typeparam>
    public class ModListView<T>
    {
        public GameObject Root { get; private set; }
        
        private List<T> _allItems = new List<T>();
        private List<T> _filteredItems = new List<T>();
        private List<GameObject> _rowPool = new List<GameObject>();
        
        private Action<GameObject> _onCreateRow;
        private Action<T, GameObject> _onBindRow;
        private Func<T, string, bool> _onFilter;
        
        private GameObject _contentContainer;
        private InputField _searchField;
        private Text _pageLabel;
        private Button _prevBtn;
        private Button _nextBtn;
        
        private int _itemsPerPage;
        private int _currentPage = 0;
        private string _currentSearch = "";

        // Internal lock to prevent rebuild thrashing
        private bool _isDirty = false;

        private float _rowHeight;

        private ModListView() { }

        /// <summary>
        /// Creates a new ModListView.
        /// </summary>
        /// <param name="parent">The parent container.</param>
        /// <param name="rowHeight">The fixed height of each row (used for scrolling setup).</param>
        /// <param name="onCreateRow">Called ONCE per pooled row to build its internal UI hierarchy (e.g. adding labels/buttons).</param>
        /// <param name="onBindRow">Called during rendering to bind a data item T to the pre-built row GameObject.</param>
        /// <param name="onFilter">Optional. If provided, a search bar is shown. Returns true if the item matches the search string.</param>
        /// <param name="itemsPerPage">Maximum items to show at once.</param>
        public static ModListView<T> Create(
            GameObject parent, 
            float rowHeight,
            Action<GameObject> onCreateRow,
            Action<T, GameObject> onBindRow, 
            Func<T, string, bool> onFilter = null,
            int itemsPerPage = 50)
        {
            ModListView<T> list = new ModListView<T>();
            list._onCreateRow = onCreateRow;
            list._onBindRow = onBindRow;
            list._onFilter = onFilter;
            list._itemsPerPage = itemsPerPage;
            list._rowHeight = rowHeight;

            list.BuildUI(parent);
            return list;
        }

        public void SetData(IEnumerable<T> items)
        {
            _allItems = items != null ? items.ToList() : new List<T>();
            _isDirty = true;
            Refresh();
        }

        public void Refresh()
        {
            if (!_isDirty && string.IsNullOrEmpty(_currentSearch)) 
            {
                // Force a bind refresh without changing array
                BindCurrentPage();
                return;
            }

            // Apply filter
            if (_onFilter != null && !string.IsNullOrEmpty(_currentSearch))
            {
                _filteredItems = _allItems.Where(item => _onFilter(item, _currentSearch)).ToList();
            }
            else
            {
                _filteredItems = _allItems.ToList();
            }

            // Enforce bounds
            int maxPage = Mathf.Max(0, (_filteredItems.Count - 1) / _itemsPerPage);
            if (_currentPage > maxPage) _currentPage = maxPage;

            BindCurrentPage();
            _isDirty = false;
        }

        private void BindCurrentPage()
        {
            int startIndex = _currentPage * _itemsPerPage;
            int endIndex = Mathf.Min(startIndex + _itemsPerPage, _filteredItems.Count);
            int itemsToShow = endIndex - startIndex;

            // Ensure our pool has enough rows for this page
            EnsurePoolSize(itemsToShow);

            // Bind data to active rows
            for (int i = 0; i < _rowPool.Count; i++)
            {
                GameObject row = _rowPool[i];
                if (i < itemsToShow)
                {
                    row.SetActive(true);
                    _onBindRow.Invoke(_filteredItems[startIndex + i], row);
                }
                else
                {
                    row.SetActive(false);
                }
            }

            // Update pagination UI
            if (_pageLabel != null)
            {
                int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)_filteredItems.Count / _itemsPerPage));
                _pageLabel.text = "Page " + (_currentPage + 1) + " / " + totalPages + " (" + _filteredItems.Count + " total)";
                _prevBtn.interactable = _currentPage > 0;
                _nextBtn.interactable = _currentPage < totalPages - 1;
            }
            
            // Rebuild layout to clean up spacing
            LayoutRebuilder.ForceRebuildLayoutImmediate(_contentContainer.GetComponent<RectTransform>());
        }

        private void EnsurePoolSize(int size)
        {
            while (_rowPool.Count < size)
            {
                GameObject row = new GameObject("Row_" + _rowPool.Count);
                RectTransform rowRect = row.AddComponent<RectTransform>();
                rowRect.SetParent(_contentContainer.transform, false);
                
                LayoutElement le = row.AddComponent<LayoutElement>();
                le.minHeight = _rowHeight;
                le.preferredHeight = _rowHeight;
                le.flexibleHeight = 0f;
                
                _onCreateRow.Invoke(row);
                
                // Start hidden until bound
                row.SetActive(false);
                _rowPool.Add(row);
            }
        }

        private void BuildUI(GameObject parent)
        {
            Root = ModPanel.Create(parent);
            
            
            // ModPanel.Create already sets childControlHeight=true globally now.
            VerticalLayoutGroup rootLayout = Root.GetComponent<VerticalLayoutGroup>();
            rootLayout.childForceExpandHeight = false;

            
            // 1. Top bar: Search
            if (_onFilter != null)
            {
                GameObject topBar = ModPanel.CreateHorizontal(Root);
                LayoutElement topLe = topBar.GetComponent<LayoutElement>();
                topLe.minHeight = 40f;
                topLe.preferredHeight = 40f;
                topBar.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(0,0,0,4);
                
                _searchField = ModSearchField.Create("Search...", (query) => 
                {
                    _currentSearch = query.ToLowerInvariant();
                    _currentPage = 0; // Reset to page 0 on search
                    _isDirty = true;
                    Refresh();
                }, topBar);
            }

            // 2. Middle: ScrollView
            // ModScrollView itself creates a LayoutElement with flexibleHeight and flexibleWidth = 1
            // so it naturally expands to fill the remaining area in Root.
            _contentContainer = ModScrollView.Create(0, Root);
            
            // Enable child control height so rows stack properly
            VerticalLayoutGroup vlg = _contentContainer.GetComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 2f; // Slight gap between rows

            // 3. Bottom bar: Pagination
            GameObject bottomBar = ModPanel.CreateHorizontal(Root);
            LayoutElement botLe = bottomBar.GetComponent<LayoutElement>();
            botLe.minHeight = 40f;
            botLe.preferredHeight = 40f;
            HorizontalLayoutGroup bottomLayout = bottomBar.GetComponent<HorizontalLayoutGroup>();
            bottomLayout.childAlignment = TextAnchor.MiddleCenter;
            bottomLayout.padding = new RectOffset(0,0,4,0);

            _prevBtn = ModButton.Create("<", () => 
            {
                if (_currentPage > 0) { _currentPage--; Refresh(); }
            }, bottomBar).GetComponent<Button>();

            _pageLabel = ModLabel.Create("Page 1 / 1", bottomBar, true);
            _pageLabel.alignment = TextAnchor.MiddleCenter;
            LayoutElement plLayout = _pageLabel.gameObject.AddComponent<LayoutElement>();
            plLayout.minWidth = 120f;

            _nextBtn = ModButton.Create(">", () => 
            {
                int maxPage = (_filteredItems.Count - 1) / _itemsPerPage;
                if (_currentPage < maxPage) { _currentPage++; Refresh(); }
            }, bottomBar).GetComponent<Button>();
        }
    }
}
