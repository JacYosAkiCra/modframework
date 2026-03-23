using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Right-click context menu positioned at mouse cursor.
/// Auto-dismisses on click outside or Escape.
/// </summary>
namespace ModFramework.UI.Custom
{
    public class ContextMenuItem
    {
        public string Label;
        public Action Callback;
        public Color TextColor;
        public bool IsSeparator;

        public ContextMenuItem(string label, Action callback)
        {
            Label = label;
            Callback = callback;
            TextColor = GameTheme.LabelColor;
            IsSeparator = false;
        }

        public ContextMenuItem(string label, Action callback, Color textColor)
        {
            Label = label;
            Callback = callback;
            TextColor = textColor;
            IsSeparator = false;
        }

        /// <summary>
        /// Create a separator line between menu items.
        /// </summary>
        public static ContextMenuItem CreateSeparator()
        {
            return new ContextMenuItem("", null) { IsSeparator = true };
        }
    }

    public static class ModContextMenu
    {
        private static GameObject _currentMenu;

        /// <summary>
        /// Show a context menu at the current mouse position.
        /// </summary>
        public static void Show(ContextMenuItem[] items)
        {
            Show(new List<ContextMenuItem>(items));
        }

        /// <summary>
        /// Show a context menu at the current mouse position.
        /// </summary>
        public static void Show(List<ContextMenuItem> items)
        {
            // Close any existing menu
            Close();

            if (items == null || items.Count == 0) return;

            // Find the canvas
            Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            // Create blocker (clickable full-screen background to dismiss)
            GameObject blocker = new GameObject("ContextMenuBlocker");
            RectTransform blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.SetParent(canvas.transform, false);
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.sizeDelta = Vector2.zero;
            blockerRect.SetAsLastSibling();

            Image blockerImg = blocker.AddComponent<Image>();
            blockerImg.color = Color.clear; // Invisible but clickable
            blockerImg.raycastTarget = true;

            Button blockerBtn = blocker.AddComponent<Button>();
            blockerBtn.onClick.AddListener(Close);

            _currentMenu = blocker;

            // Create menu panel
            GameObject menu = new GameObject("ContextMenu");
            RectTransform menuRect = menu.AddComponent<RectTransform>();
            menuRect.SetParent(blocker.transform, false);

            Image menuBg = menu.AddComponent<Image>();
            menuBg.color = GameTheme.WindowBackground;
            menuBg.raycastTarget = true;

            VerticalLayoutGroup vlg = menu.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 1f;
            vlg.padding = new RectOffset(2, 2, 4, 4);

            ContentSizeFitter csf = menu.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Position at mouse
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                blockerRect, Input.mousePosition, canvas.worldCamera, out localPoint);
            menuRect.anchoredPosition = localPoint;
            menuRect.pivot = new Vector2(0f, 1f); // Top-left corner at mouse

            // Build menu items
            foreach (var item in items)
            {
                if (item.IsSeparator)
                {
                    // Separator line
                    GameObject sep = new GameObject("Separator");
                    RectTransform sepRect = sep.AddComponent<RectTransform>();
                    sepRect.SetParent(menu.transform, false);
                    Image sepImg = sep.AddComponent<Image>();
                    sepImg.color = GameTheme.Separator;
                    LayoutElement sepLe = sep.AddComponent<LayoutElement>();
                    sepLe.preferredHeight = 1f;
                    sepLe.flexibleWidth = 1f;
                    continue;
                }

                // Menu item button
                GameObject itemObj = new GameObject("MenuItem_" + item.Label);
                RectTransform itemRect = itemObj.AddComponent<RectTransform>();
                itemRect.SetParent(menu.transform, false);

                Image itemBg = itemObj.AddComponent<Image>();
                itemBg.color = Color.clear;

                LayoutElement itemLe = itemObj.AddComponent<LayoutElement>();
                itemLe.preferredHeight = 26f;
                itemLe.minWidth = 140f;

                HorizontalLayoutGroup itemHlg = itemObj.AddComponent<HorizontalLayoutGroup>();
                itemHlg.childControlWidth = true;
                itemHlg.childControlHeight = true;
                itemHlg.childForceExpandWidth = true;
                itemHlg.childForceExpandHeight = true;
                itemHlg.padding = new RectOffset(10, 10, 2, 2);

                // Text
                GameObject textObj = new GameObject("Text");
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.SetParent(itemObj.transform, false);
                Text text = textObj.AddComponent<Text>();
                text.text = item.Label;
                text.font = GameTheme.GameFont;
                text.fontSize = GameTheme.DefaultFontSize;
                text.color = item.TextColor;
                text.alignment = TextAnchor.MiddleLeft;

                // Click handler
                Button btn = itemObj.AddComponent<Button>();
                var cb = item.Callback;
                btn.onClick.AddListener(() =>
                {
                    Close();
                    if (cb != null) cb.Invoke();
                });

                // Hover effect
                HoverHandler hover = itemObj.AddComponent<HoverHandler>();
                hover.Setup(itemBg);
            }

            // Register Escape to close
            ModKeybind.RegisterGlobal("__ContextMenu_Escape", KeyCode.Escape, Close);
        }

        /// <summary>
        /// Close the context menu.
        /// </summary>
        public static void Close()
        {
            ModKeybind.Unregister("__ContextMenu_Escape");
            if (_currentMenu != null)
            {
                UnityEngine.Object.Destroy(_currentMenu);
                _currentMenu = null;
            }
        }

        /// <summary>
        /// Bind a context menu to a UI element so it opens on right-click.
        /// </summary>
        public static void Bind(GameObject target, ContextMenuItem[] items)
        {
            Bind(target, new List<ContextMenuItem>(items));
        }

        /// <summary>
        /// Bind a context menu to a UI element so it opens on right-click.
        /// </summary>
        public static void Bind(GameObject target, List<ContextMenuItem> items)
        {
            var handler = target.GetComponent<RightClickHandler>();
            if (handler == null)
                handler = target.AddComponent<RightClickHandler>();
            handler.Items = items;
        }
    }

    /// <summary>
    /// Detects right-click on a UI element and opens a context menu.
    /// </summary>
    internal class RightClickHandler : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        public List<ContextMenuItem> Items;

        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Right)
            {
                if (Items != null && Items.Count > 0)
                {
                    ModContextMenu.Show(Items);
                }
            }
        }
    }
}
