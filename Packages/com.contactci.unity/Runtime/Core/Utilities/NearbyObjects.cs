using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maestro
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class NearbyObjects : MonoBehaviour
    {
        public float radius = 0.2f;
        public bool debug = false;

        private Rigidbody rb;
        private SphereCollider detector;
        private List<MaestroInteractable> objects;

        private Color DebugColor = Color.magenta;
        private static Dictionary<MaestroInteractable, Color> originalColors;

        static NearbyObjects() 
        {
            originalColors = new Dictionary<MaestroInteractable, Color>();
        }

        void Awake()
        {
            rb = this.GetComponent<Rigidbody>();
            detector = this.GetComponent<SphereCollider>();

            if (!debug)
                Destroy(this.GetComponent<Renderer>());

            objects = new List<MaestroInteractable>();

            rb.useGravity = false;
            rb.isKinematic = true;
            detector.isTrigger = true;
        }

        // Update is called once per frame
        void Update()
        {
            detector.transform.localScale = Vector3.one * radius;
        }

        public List<MaestroInteractable> get()
        {
            return objects;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (IsObjectValid(other, out MaestroInteractable mi)) {
                objects.Add(mi);
                if (debug)
                    SetRenderColor(mi, DebugColor);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (IsObjectValid(other, out MaestroInteractable mi)) {
                objects.Remove(mi);
                if (debug)
                    UnsetRenderColor(mi);
            }
        }

        private void SetRenderColor(MaestroInteractable toSet, Color c)
        {
            Renderer r = toSet.GetComponent<Renderer>();
            if (r != null) {
                try {
                    originalColors.Add(toSet, r.material.color);
                } catch (Exception) { /* TODO count occurences */}
                r.material.color = c;
            }
        }

        private void UnsetRenderColor(MaestroInteractable toUnset)
        {
            Renderer r = toUnset.GetComponent<Renderer>();
            if (r != null) {
                if (originalColors.TryGetValue(toUnset, out Color originalColor)) {
                    r.material.color = originalColor;
                    originalColors.Remove(toUnset);
                } else {
                    Debug.LogWarning(string.Format("Couldn't find original color for object []!", toUnset.name));
                }
            }
        }

        private bool IsObjectValid(Collider obj, out MaestroInteractable mi)
        {
            mi = obj.GetComponent<MaestroInteractable>();
            return mi != null && mi.type != InteractionType.Static;
        }
    }
}
