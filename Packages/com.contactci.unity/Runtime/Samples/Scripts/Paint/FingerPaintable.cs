using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Codice.Client.IssueTracker;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Maestro
{
    public class FingerPaintable : MonoBehaviour
    {
        public float desiredSize;

        private AudioSource source;
        private float waitTime = 0.05f;
        private float elapsed = 0.0f;

        private bool ClearDefined = false;

        private float _maxSeparation = 0.001f;
        private float _vertexSeparation = 0.02f;
        private float _minRadius = 0.8f;
        private float _cornerVertices = 16;

        public UnityEvent onClear;
        public RenderTexture canvasTexture;

        private int lineCount = 0;

        private const bool ENABLE_RENDER = false;
        public void SetLineWidth(float diameterCm)
        {
            desiredSize = diameterCm / 5f;
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
            
            InitRender();
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
            if (!(first.separation <= _maxSeparation)) 
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
            if (!(first.separation <= _maxSeparation)) 
                return;
            
            if (source != null && !source.isPlaying)
            {
                source.Play();
            }

            elapsed = 0.0f;
            
            var tr = pt.splotch.GetComponent<TrailRenderer>();
            var vtx = first.point - first.normal * _maxSeparation;

            if (Vector3.Distance(tr.GetPosition(tr.positionCount - 1), vtx) >= tr.minVertexDistance)
            {
                AddPoint(tr, vtx);
            }

            tr.transform.position = vtx;
        }

        private void AddPoint(TrailRenderer tr, Vector3 next)
        {
            if (tr.positionCount < 2)
                return;
            
            //check if angle to new point is beyond our radius
            
            var prev = tr.GetPosition(tr.positionCount - 1);
            var prev2 = tr.GetPosition(tr.positionCount - 2);
            var from = prev - prev2;
            var to = next - prev;
            var pmag = from.magnitude;
            from.Normalize();
            to.Normalize();
            if (Vector3.Dot(from, to) >= _minRadius)
                return;
            
            //compute bezier curve between the latest 3 points
            var pa = prev;
            var pc = next;
            var pb = pa + (pc - pa) / 2 + from * pmag * _minRadius;

            for (int i = 0; i < _cornerVertices; i++)
            {
                var frac = (1 / _cornerVertices) * (i + 1);
                var m1 = Vector3.Lerp(pa, pb, frac);
                var m2 = Vector3.Lerp(pb, pc, frac);
                var npos = Vector3.Lerp(m1, m2, frac);
                tr.AddPosition(npos);
            }

        }

        private void InitSplotch(PaintType pt, ContactPoint first)
        {
            GameObject brushObject = Instantiate(pt.splotch);

            var tr = brushObject.GetComponent<TrailRenderer>();

            tr.transform.parent = this.transform;
            tr.transform.rotation = this.transform.rotation;
            tr.transform.localScale = this.transform.localScale;
            
            tr.startColor = pt.paintColor;
            tr.endColor = pt.paintColor;
            tr.sortingOrder = lineCount++; // stack new lines over old ones
            tr.startWidth = desiredSize;
            tr.endWidth = desiredSize;
            // increase vertex separation with thicker lines. looks better
            tr.minVertexDistance = desiredSize / 25f;
            tr.numCapVertices = 32;
            tr.numCornerVertices = 0;

            brushObject.transform.position = first.point + first.normal * (first.separation - 0.001f);
            brushObject.transform.rotation = Quaternion.LookRotation(first.normal);
            brushObject.transform.parent = this.transform;
            
            // Set scale in world space
            brushObject.transform.localScale = Vector3.one * desiredSize;

            tr.transform.position = brushObject.transform.position;
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

            //round trail ends by looping back to the previous position
            Vector3 rdir;
            if (tr.positionCount > 1)
            {
                var prev = tr.GetPosition(tr.positionCount - 1);
                var prev2 = tr.GetPosition(tr.positionCount - 2);
                rdir = prev2 - prev;
                rdir.Normalize();
            }
            else
            {
                rdir = tr.transform.position + Vector3.one * _minRadius;
                AddPoint(tr, rdir);
                rdir = -rdir.normalized;
            }
            

            var rpos = tr.transform.position + rdir * tr.minVertexDistance;
            AddPoint(tr, rpos);
            tr.transform.position = rpos;

            tr.emitting = false;
            
            if (ENABLE_RENDER)
            {
                RenderMesh(tr, tr.sharedMaterial, Vector3.zero);

                tr.Clear();
            }
        }

        private CommandBuffer _com;
        public bool wantClear;
        private void InitRender()
        {
            canvasTexture = new RenderTexture(8752, 6108, 24);
            canvasTexture.Create();
            _com = new CommandBuffer();
            
            var ren = GetComponent<MeshRenderer>();
            ren.material.mainTexture = canvasTexture;

        }
        private void RenderMesh(TrailRenderer trail, Material material, Vector3 offset)
        {
            var ttr = trail.transform;
            var lookMatrix = Matrix4x4.LookAt(ttr.position + ttr.forward, ttr.position, ttr.up);
            var rotate = Quaternion.LookRotation(new Vector3(-1, 0, 0), new Vector3(0, 0, 1));
            var scaleMatrix = Matrix4x4.TRS(Vector3.back, rotate, Vector3.one);
            var viewMatrix = scaleMatrix * lookMatrix.inverse;
            var projMatrix = Matrix4x4.Perspective(600f, 1f, 1f, 100f);

            var transMatrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one);
            
            var mesh = new Mesh();
            trail.BakeMesh(mesh);
            
            _com.SetRenderTarget(canvasTexture);
            _com.SetViewProjectionMatrices(viewMatrix, projMatrix);
            _com.DrawMesh(mesh, transMatrix, material, 0, 0);
        }

        public void OnWillRenderObject()
        {
            if(!ENABLE_RENDER)
                return;
            
            if (wantClear)
            {
                wantClear = false;
                _com.SetRenderTarget(canvasTexture);
                _com.ClearRenderTarget(true, true, Color.clear);
            }

            Graphics.ExecuteCommandBuffer(_com);
            _com.Clear();
        }

        public void Clear()
        {
            wantClear = true;
            foreach (Transform child in this.transform) {//Clear brushes
                Destroy(child.gameObject);
            }
            CleanHands();
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
