using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonoBehaviour that drives periodic in-place UI refresh.
/// Attach to a window root. Registered callbacks fire every Interval seconds
/// while the GameObject is active. Automatically pauses when hidden.
///
/// Usage (standalone):
///   var driver = myGameObject.AddComponent<ModRefreshDriver>();
///   driver.Interval = 2f;
///   driver.Register(() => label.text = "$" + company.Revenue.ToString("N0"));
///
/// Usage (via ModWindow):
///   window.OnRefresh(() => label.text = GetLiveValue());
///   window.SetRefreshInterval(1f);
/// </summary>
namespace ModFramework.UI.Custom
{
    public class ModRefreshDriver : MonoBehaviour
    {
        /// <summary>
        /// Seconds between refresh ticks. Default 3 seconds.
        /// </summary>
        public float Interval = 3f;

        private float _timer;
        private List<Action> _callbacks = new List<Action>();

        /// <summary>
        /// Register a callback to be called every Interval seconds.
        /// The callback should update UI in-place (set .text, .value, etc.).
        /// </summary>
        public void Register(Action callback)
        {
            if (callback != null && !_callbacks.Contains(callback))
            {
                _callbacks.Add(callback);
            }
        }

        /// <summary>
        /// Remove a previously registered callback.
        /// </summary>
        public void Unregister(Action callback)
        {
            _callbacks.Remove(callback);
        }

        /// <summary>
        /// Force an immediate refresh tick (resets the timer).
        /// Useful after initial setup to populate live widgets immediately.
        /// </summary>
        public void RefreshNow()
        {
            _timer = 0f;
            ExecuteCallbacks();
        }

        /// <summary>
        /// Number of currently registered callbacks.
        /// </summary>
        public int CallbackCount { get { return _callbacks.Count; } }

        private void Update()
        {
            if (_callbacks.Count == 0) return;

            _timer += Time.deltaTime;
            if (_timer >= Interval)
            {
                _timer = 0f;
                ExecuteCallbacks();
            }
        }

        private void ExecuteCallbacks()
        {
            for (int i = _callbacks.Count - 1; i >= 0; i--)
            {
                try
                {
                    _callbacks[i].Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[ModFramework] RefreshDriver callback error: " + e.Message);
                    // Remove broken callbacks to prevent log spam
                    _callbacks.RemoveAt(i);
                }
            }
        }
    }
}
