using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Singleton MonoBehaviour that polls Input for registered hotkeys every frame.
/// Skips polling if the user is currently typing in an InputField.
/// </summary>
namespace ModFramework.UI.Custom
{
    public class ModHotkeyPoller : MonoBehaviour
    {
        void Update()
        {
            // Do not fire hotkeys if the user is typing in a text field!
            if (EventSystem.current != null && 
                EventSystem.current.currentSelectedGameObject != null)
            {
                // If the selected object has an InputField (or is one), skip hotkeys
                if (EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null ||
                    EventSystem.current.currentSelectedGameObject.GetComponentInParent<InputField>() != null)
                {
                    return;
                }
            }

            // Let the registry check the inputs
            ModHotkeyRegistry.Poll();
        }
    }
}
