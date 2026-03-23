using UnityEngine;

/// <summary>
/// Common utility functions
/// </summary>
namespace ModFramework
{
    public static class ModUtils
    {
        public static string FormatCurrency(float amount)
        {
            return "$" + amount.ToString("N0");
        }

        public static string FormatNumber(float number)
        {
            return FormatNumber(number, 0);
        }

        public static string FormatNumber(float number, int decimals)
        {
            return number.ToString("N" + decimals);
        }

        public static string FormatPercent(float value)
        {
            return (value * 100f).ToString("F1") + "%";
        }

        public static string FormatTime(float seconds)
        {
            int hours = (int)(seconds / 3600f);
            int minutes = (int)((seconds % 3600f) / 60f);
            int secs = (int)(seconds % 60f);
            
            if (hours > 0)
                return string.Format("{0}h {1}m {2}s", hours, minutes, secs);
            if (minutes > 0)
                return string.Format("{0}m {1}s", minutes, secs);
            return string.Format("{0}s", secs);
        }

        public static T GetSingleton<T>() where T : MonoBehaviour
        {
            return Object.FindObjectOfType<T>();
        }

        public static bool IsInGame()
        {
            return GameSettings.Instance != null && HUD.Instance != null;
        }

        public static string TruncateString(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;
            return text.Substring(0, maxLength - 3) + "...";
        }
    }
}
