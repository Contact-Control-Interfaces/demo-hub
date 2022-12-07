using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    public static class Extensions
    {
        public static T GetOrMake<T>(this GameObject obj) where T : Component
        {
            T temp = obj.GetComponent<T>();
            // null coalescing shouldn't be used with Unity types, ?? breaks
            return temp != null ? temp : obj.AddComponent<T>();
        }

        public static T GetOrMake<T>(this GameObject obj, Action<T> initializer) where T : Component
        {
            T temp = obj.GetComponent<T>();

            if (temp == null) {
                temp = obj.AddComponent<T>();
                initializer(temp);
            }

            return temp;
        }
    }
}
