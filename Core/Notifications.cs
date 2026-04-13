using UnityEngine;

/// <summary>
/// Easy in-game notification system
/// </summary>
namespace ModFramework
{
    public static class Notifications
    {
        public static void Show(string message)
        {
            Show(message, "Info", 2f);
        }

        public static void Show(string message, string icon, float duration)
        {
            if (HUD.Instance != null)
            {
                HUD.Instance.AddPopupMessage(
                    message,
                    icon,
                    PopupManager.PopUpAction.None,
                    0,
                    PopupManager.NotificationSound.Neutral,
                    duration
                );
            }
        }

        public static void ShowSuccess(string message)
        {
            Show(message, "Check", 2f);
        }

        public static void ShowWarning(string message)
        {
            if (HUD.Instance != null)
            {
                HUD.Instance.AddPopupMessage(
                    message,
                    "Warning",
                    PopupManager.PopUpAction.None,
                    0,
                    PopupManager.NotificationSound.Warning,
                    3f
                );
            }
        }

        public static void ShowError(string message)
        {
            if (HUD.Instance != null)
            {
                HUD.Instance.AddPopupMessage(
                    message,
                    "Exclamation",
                    PopupManager.PopUpAction.None,
                    0,
                    PopupManager.NotificationSound.Issue,
                    4f
                );
            }
        }
    }
}
