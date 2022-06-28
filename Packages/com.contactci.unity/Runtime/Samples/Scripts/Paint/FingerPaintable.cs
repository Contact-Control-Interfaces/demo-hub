using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Maestro
{
    public class FingerPaintable : MonoBehaviour
    {
        public static float brushSize = 100f;
        public static int maxDots = 5000;
        public static float dist = 0.1f;
        public static float desiredSize = 0.02f;

        private AudioSource source;
        private float waitTime = 0.05f;
        private float elapsed = 0.0f;

        private bool ClearDefined = false;

        public float maxSeparation = 0.0001f;

        public UnityEvent onClear;
        public RenderTexture canvasTexture;

        private void Start()
        {
            try {
                Input.GetButton("Clear");
                ClearDefined = true;
            } catch (Exception) {
                Debug.LogWarning("Input 'Clear' is not bound! Define it for a shortcut to clear the paint canvas.");
                ClearDefined = false;
            }

            source = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (ClearDefined && Input.GetButton("Clear") && onClear != null) {
                Clear();
                onClear.Invoke();
            }

            elapsed += Time.deltaTime;
            if (elapsed > waitTime && source != null && source.isPlaying) {
                source.Stop();
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            FingerCollider fc = collision.gameObject.GetComponent<FingerCollider>();
            if (fc != null) {

                Transform paintTransform = fc.transform.Find("Paint");
                if (paintTransform != null) {

                    PaintType pt = paintTransform.gameObject.GetComponent<PaintType>();
                    if (pt.paintColor != Color.clear && !pt.erase) {

                        ContactPoint first = collision.contacts[0];
                        if (first.separation <= maxSeparation) {

                            if (source != null && !source.isPlaying) {
                                source.Play();
                            }
                            elapsed = 0.0f;

                            InitSplotch(pt, first);
                        }
                    }
                }
            }
        }

        private void InitSplotch(PaintType pt, ContactPoint first)
        {
            GameObject brushObject = Instantiate(pt.splotch);

            // Set scale in world space
            brushObject.transform.parent = null;
            brushObject.transform.localScale = Vector3.one * desiredSize;

            SpriteRenderer sr = brushObject.GetComponent<SpriteRenderer>();
            sr.color = pt.paintColor;
            sr.sortingOrder = this.transform.childCount;

            brushObject.transform.position = first.point + first.normal * (first.separation - 0.001f);
            brushObject.transform.rotation = Quaternion.LookRotation(first.normal);
            brushObject.transform.parent = this.transform;

            if (brushObject.GetComponent<GetErasedBehavior>() == null) {
                brushObject.AddComponent<GetErasedBehavior>();
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            // NOTHING
        }

        public void Clear()
        {
            foreach (Transform child in this.transform) {//Clear brushes
                Destroy(child.gameObject);
            }
        }

        public static void CleanHands()
        {
            PaintType[] paintBlobs = GameObject.FindObjectsOfType<PaintType>();
            foreach (PaintType pt in paintBlobs) {
                pt.gameObject.SetActive(false);
                Destroy(pt.gameObject);
            }
        }
    }
}
