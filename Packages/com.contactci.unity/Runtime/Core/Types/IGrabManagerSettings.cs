using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Maestro
{
    public abstract class IGrabManagerSettings<T> : MonoBehaviour where T : IGrabManager
    {
        private static IGrabManagerSettings<T> _instance;
        protected static IGrabManagerSettings<T> Instance {
            get {
                if (_instance == null) {
                    _instance = GameObject.FindObjectOfType<IGrabManagerSettings<T>>();
                }
                return _instance;
            }
        }

        public static void ApplyToAll()
        {
            if (Instance != null)
                Instance.ApplySettingsToAll();
        }

        public static void Apply(T applyTo)
        {
            if (Instance != null)
                Instance.ApplySettings(applyTo);
        }

        protected virtual void OnValidate()
        {
            ApplySettingsToAll();
        }

        protected virtual void ApplySettingsToAll()
        {
            // Since IGrabManager isn't a Unity Object, can't use GameObject.Find directly
            var managing = GameObject.FindObjectsOfType<IMaestroHand>()
                .Select(x => x.grabManager)
                .Where(x => x is T)
                .Cast<T>()
                .ToArray();

            if (managing.Length > 0) {
                Debug.Log($"Applying settings to {managing.Length} managers of type {typeof(T)}");

                foreach (T t in managing) {
                    Apply(t);
                }
            }
        }

        protected abstract void ApplySettings(T manager);
    }
}
