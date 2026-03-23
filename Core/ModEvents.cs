using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple event system for mod communication
/// </summary>
namespace ModFramework
{
    public static class ModEvents
    {
        private static Dictionary<string, List<Action<object>>> events = new Dictionary<string, List<Action<object>>>();

        public static void Subscribe(string eventName, Action<object> callback)
        {
            if (!events.ContainsKey(eventName))
            {
                events[eventName] = new List<Action<object>>();
            }
            events[eventName].Add(callback);
        }

        public static void Unsubscribe(string eventName, Action<object> callback)
        {
            if (events.ContainsKey(eventName))
            {
                events[eventName].Remove(callback);
            }
        }

        public static void Trigger(string eventName, object data = null)
        {
            if (events.ContainsKey(eventName))
            {
                foreach (var callback in events[eventName])
                {
                    try
                    {
                        callback.Invoke(data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("ModEvents error: " + e.Message);
                    }
                }
            }
        }

        public static void Clear()
        {
            events.Clear();
        }
    }
}
