using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace ModFramework.UI.Custom
{
    public static class ModDialog
    {
        public static void ShowMessage(string title, string message, Action onConfirm = null)
        {
            Show(title, message, "OK", onConfirm, null, null);
        }

        public static void ShowConfirm(string title, string message, Action onConfirm, Action onCancel = null)
        {
            Show(title, message, "Yes", onConfirm, "No", onCancel);
        }
        
        public static void ShowList(string title, string[] items, Action<int> onSelect, string cancelText = "Cancel")
        {
            GameTheme.Initialize();

            // 1. Blocker Container
            GameObject canvasObj = new GameObject("ModDialogContainer_" + title);
            RectTransform canvasRect = canvasObj.AddComponent<RectTransform>();
            canvasRect.SetParent(WindowManager.Instance.Canvas.transform, false);
            canvasRect.SetAsLastSibling();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.sizeDelta = Vector2.zero;

            // 2. Blocker Background (Click to dismiss)
            GameObject blockerObj = new GameObject("BlockerBg");
            blockerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform blockerRect = blockerObj.AddComponent<RectTransform>();
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.sizeDelta = Vector2.zero;
            
            Image blockerBg = blockerObj.AddComponent<Image>();
            blockerBg.color = new Color(0, 0, 0, 0.65f);
            blockerBg.raycastTarget = true;

            Action closeDialog = () => {
                UnityEngine.Object.Destroy(canvasObj);
            };

            Button blockerBtn = blockerObj.AddComponent<Button>();
            blockerBtn.onClick.AddListener(new UnityAction(closeDialog));

            // 3. Window Panel
            GameObject winPanel = new GameObject("DialogWindow");
            winPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform winRect = winPanel.AddComponent<RectTransform>();
            winRect.anchorMin = new Vector2(0.5f, 0.5f);
            winRect.anchorMax = new Vector2(0.5f, 0.5f);
            winRect.pivot = new Vector2(0.5f, 0.5f);
            winRect.sizeDelta = new Vector2(400f, 400f);
            winRect.anchoredPosition = Vector2.zero;

            Image winBg = winPanel.AddComponent<Image>();
            winBg.color = GameTheme.WindowBackground;
            if (GameTheme.WindowSprite != null) { winBg.sprite = GameTheme.WindowSprite; winBg.type = Image.Type.Sliced; }

            // Ensure window catches clicks so they don't pass through to the background dismissal
            Image winCatcher = winBg; 
            winCatcher.raycastTarget = true;

            VerticalLayoutGroup winVlg = winPanel.AddComponent<VerticalLayoutGroup>();
            winVlg.padding = new RectOffset(16, 16, 16, 16);
            winVlg.spacing = 8f;
            winVlg.childControlHeight = true;
            winVlg.childForceExpandHeight = false;
            
            // 4. Title Header
            ModHeader.Create(title, winPanel);

            // 5. Message Content - ScrollView
            GameObject content = new GameObject("DialogContent");
            content.AddComponent<RectTransform>().SetParent(winPanel.transform, false);
            LayoutElement contentLe = content.AddComponent<LayoutElement>();
            contentLe.flexibleHeight = 1f;

            GameObject svContent = ModScrollView.Create(250f, content);
            
            for (int i = 0; i < items.Length; i++)
            {
                int idx = i;
                ModButton.Create(items[i], () => {
                    closeDialog();
                    if (onSelect != null) onSelect.Invoke(idx);
                }, svContent);
            }

            // 6. Buttons
            GameObject btnRow = ModPanel.CreateHorizontal(winPanel);
            btnRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            LayoutElement btnRowLe = btnRow.AddComponent<LayoutElement>();
            btnRowLe.minHeight = 40f;
            btnRowLe.preferredHeight = 40f;

            if (!string.IsNullOrEmpty(cancelText))
            {
                ModButton.Create(cancelText, () => 
                {
                    closeDialog();
                }, btnRow, 100f);
            }
        }

        private static void Show(string title, string message, string confirmText, Action onConfirm, string cancelText, Action onCancel)
        {
            GameTheme.Initialize();

            // 1. Blocker Container
            GameObject canvasObj = new GameObject("ModDialogContainer_" + title);
            RectTransform canvasRect = canvasObj.AddComponent<RectTransform>();
            canvasRect.SetParent(WindowManager.Instance.Canvas.transform, false);
            canvasRect.SetAsLastSibling();
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
            blockerBg.color = new Color(0, 0, 0, 0.65f);
            blockerBg.raycastTarget = true;

            Action closeDialog = () => {
                UnityEngine.Object.Destroy(canvasObj);
            };

            Button blockerBtn = blockerObj.AddComponent<Button>();
            blockerBtn.onClick.AddListener(new UnityAction(closeDialog));

            // 3. Window Panel
            GameObject winPanel = new GameObject("DialogWindow");
            winPanel.transform.SetParent(canvasObj.transform, false);
            RectTransform winRect = winPanel.AddComponent<RectTransform>();
            winRect.anchorMin = new Vector2(0.5f, 0.5f);
            winRect.anchorMax = new Vector2(0.5f, 0.5f);
            winRect.pivot = new Vector2(0.5f, 0.5f);
            winRect.sizeDelta = new Vector2(400f, 220f);
            winRect.anchoredPosition = Vector2.zero;

            Image winBg = winPanel.AddComponent<Image>();
            winBg.color = GameTheme.WindowBackground;
            if (GameTheme.WindowSprite != null) { winBg.sprite = GameTheme.WindowSprite; winBg.type = Image.Type.Sliced; }
            winBg.raycastTarget = true;

            VerticalLayoutGroup winVlg = winPanel.AddComponent<VerticalLayoutGroup>();
            winVlg.padding = new RectOffset(16, 16, 16, 16);
            winVlg.spacing = 8f;
            winVlg.childControlHeight = true;
            winVlg.childForceExpandHeight = false;
            
            // 4. Title Header
            ModHeader.Create(title, winPanel);

            // 5. Message Content
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
