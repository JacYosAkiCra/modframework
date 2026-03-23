using System;
using UnityEngine;
using UnityEngine.UI;

namespace ModFramework.UI.Custom
{
    /// <summary>
    /// Utility for creating modal dialog boxes (Message, Confirm, etc).
    /// </summary>
    public static class ModDialog
    {
        /// <summary>
        /// Shows a simple OK message box that steals focus.
        /// </summary>
        public static void ShowMessage(string title, string message, Action onConfirm = null)
        {
            Show(title, message, "OK", onConfirm, null, null);
        }

        /// <summary>
        /// Shows a Yes/No confirmation dialog box.
        /// </summary>
        public static void ShowConfirm(string title, string message, Action onConfirm, Action onCancel = null)
        {
            Show(title, message, "Yes", onConfirm, "No", onCancel);
        }

        private static void Show(string title, string message, string confirmText, Action onConfirm, string cancelText, Action onCancel)
        {
            GameTheme.Initialize();

            // 1. Blocker Canvas (Ensures top order)
            GameObject canvasObj = new GameObject("ModDialogCanvas_" + title);
            canvasObj.transform.SetParent(WindowManager.Instance.Canvas.transform, false);
            canvasObj.transform.SetAsLastSibling(); // Top most

            Canvas c = canvasObj.AddComponent<Canvas>();
            c.pixelPerfect = true;
            c.overrideSorting = true;
            c.sortingOrder = 30000;
            canvasObj.AddComponent<GraphicRaycaster>(); // Ensures clicks on blocker work properly

            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.sizeDelta = Vector2.zero;

            // 2. Blocker Background
            GameObject blockerObj = new GameObject("BlockerBg");
            blockerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform blockerRect = blockerObj.AddComponent<RectTransform>();
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.sizeDelta = Vector2.zero;
            Image blockerBg = blockerObj.AddComponent<Image>();
            blockerBg.color = new Color(0, 0, 0, 0.65f); // 65% black dim
            blockerBg.raycastTarget = true; // Blocks all clicks below!

            // 3. Window Panel
            GameObject winPanel = new GameObject("DialogWindow");
            winPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform winRect = winPanel.AddComponent<RectTransform>();
            // Center
            winRect.anchorMin = new Vector2(0.5f, 0.5f);
            winRect.anchorMax = new Vector2(0.5f, 0.5f);
            winRect.pivot = new Vector2(0.5f, 0.5f);
            winRect.sizeDelta = new Vector2(400f, 220f);
            winRect.anchoredPosition = Vector2.zero;

            Image winBg = winPanel.AddComponent<Image>();
            winBg.color = GameTheme.WindowBackground;
            if (GameTheme.WindowSprite != null) { winBg.sprite = GameTheme.WindowSprite; winBg.type = Image.Type.Sliced; }

            VerticalLayoutGroup winVlg = winPanel.AddComponent<VerticalLayoutGroup>();
            winVlg.padding = new RectOffset(16, 16, 16, 16);
            winVlg.spacing = 8f;
            winVlg.childControlHeight = true;
            winVlg.childForceExpandHeight = false;
            
            // 4. Title Header
            ModHeader.Create(title, winPanel);

            // 5. Message Content
            // We use a simple GameObject wrapper so it takes flexible space
            GameObject content = new GameObject("DialogContent");
            content.AddComponent<RectTransform>().SetParent(winPanel.transform, false);
            content.AddComponent<LayoutElement>().flexibleHeight = 1f;

            VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandHeight = true;

            Text msg = ModLabel.Create(message, content);
            msg.alignment = TextAnchor.MiddleCenter;

            // 6. Buttons
            GameObject btnRow = ModPanel.CreateHorizontal(winPanel);
            btnRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            LayoutElement btnRowLe = btnRow.AddComponent<LayoutElement>();
            btnRowLe.minHeight = 40f;
            btnRowLe.preferredHeight = 40f;

            Action closeDialog = () => {
                UnityEngine.Object.Destroy(canvasObj);
            };

            ModButton.Create(confirmText, () => 
            {
                closeDialog();
                if (onConfirm != null) onConfirm.Invoke();
            }, btnRow, 100f);

            if (!string.IsNullOrEmpty(cancelText))
            {
                ModButton.Create(cancelText, () => 
                {
                    closeDialog();
                    if (onCancel != null) onCancel.Invoke();
                }, btnRow, 100f);
            }
        }
    }
}
