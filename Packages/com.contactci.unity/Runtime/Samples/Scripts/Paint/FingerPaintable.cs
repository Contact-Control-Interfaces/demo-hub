using System;
using System.Collections;
using System.Collections.Generic;
using Codice.Client.IssueTracker;
using UnityEngine;
using UnityEngine.Events;

namespace Maestro
{
    public class FingerPaintable : MonoBehaviour
    {
        public float desiredSize;

        private AudioSource source;
        private float waitTime = 0.05f;
        private float elapsed = 0.0f;

        private bool ClearDefined = false;

        public float maxSeparation = 0.0001f;
        public float vertexSeparation = 0.002f;

        public UnityEvent onClear;
        public RenderTexture canvasTexture;

        private int lineCount = 0;

        public void SetLineWidth(float diameterCm)
        {
            desiredSize = diameterCm * 5f;
        }
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

        public void OnCollisionEnter(Collision collision)
        {
            FingerCollider fc = collision.gameObject.GetComponent<FingerCollider>();
            if (fc == null) 
                return;
            
            Transform paintTransform = fc.parent.transform.Find("Paint");
            if (paintTransform == null) 
                return;
            
            PaintType pt = paintTransform.gameObject.GetComponent<PaintType>();
            if (pt.paintColor == Color.clear || pt.erase) 
                return;
            
            ContactPoint first = collision.contacts[0];
            if (!(first.separation <= maxSeparation)) 
                return;
            
            elapsed = 0.0f;

            InitSplotch(pt, first);
        }

        private void OnCollisionStay(Collision collision)
        {
            FingerCollider fc = collision.gameObject.GetComponent<FingerCollider>();
            if (fc == null) 
                return;
            
            Transform paintTransform = fc.parent.transform.Find("Paint");
            if (paintTransform == null) 
                return;
            
            PaintType pt = paintTransform.gameObject.GetComponent<PaintType>();
            if (pt.paintColor == Color.clear || pt.erase) 
                return;
            
            ContactPoint first = collision.contacts[0];
            if (!(first.separation <= maxSeparation)) 
                return;
            
            if (source != null && !source.isPlaying)
            {
                source.Play();
            }

            elapsed = 0.0f;
            
            var tr = pt.splotch.GetComponent<TrailRenderer>();
            var vtx = first.point - first.normal * maxSeparation;
            tr.transform.position = vtx;
        }

        private void InitSplotch(PaintType pt, ContactPoint first)
        {
            GameObject brushObject = Instantiate(pt.splotch);

            var tr = brushObject.GetComponent<TrailRenderer>();
            tr.startColor = pt.paintColor;
            tr.endColor = pt.paintColor;
            tr.sortingOrder = lineCount++; // stack new lines over old ones
            tr.startWidth = desiredSize;
            tr.endWidth = desiredSize;
            // increase vertex separation with thicker lines. looks better
            tr.minVertexDistance = (desiredSize / 5f) * vertexSeparation;
            
            tr.transform.localScale = Vector3.one;

            brushObject.transform.position = first.point + first.normal * (first.separation - 0.001f);
            brushObject.transform.rotation = Quaternion.LookRotation(first.normal);
            brushObject.transform.parent = this.transform;
            
            // Set scale in world space
            //brushObject.transform.parent = null;
            brushObject.transform.localScale = Vector3.one * desiredSize;

            tr.emitting = true;

            if (brushObject.GetComponent<GetErasedBehavior>() == null) {
                brushObject.AddComponent<GetErasedBehavior>();
            }
            
            pt.splotch = brushObject;
        }

        private void OnCollisionExit(Collision collision)
        {
            FingerCollider fc = collision.gameObject.GetComponent<FingerCollider>();
            if (fc == null) 
                return;
            
            Transform paintTransform = fc.parent.transform.Find("Paint");
            if (paintTransform == null) 
                return;
            
            PaintType pt = paintTransform.gameObject.GetComponent<PaintType>();
            var tr = pt.splotch.GetComponent<TrailRenderer>();
            if (tr == null)
                return;

            var first = collision.contacts[0];
            
            var vtx = first.point - first.normal * maxSeparation;
            tr.transform.position = vtx;
            
            tr.emitting = false;
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
