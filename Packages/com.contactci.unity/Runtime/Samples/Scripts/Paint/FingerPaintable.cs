using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Maestro.Core.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace Maestro
{
    public class FingerPaintable : MonoBehaviour
    {
        private AudioSource source;
        private float waitTime = 0.05f;
        private float elapsed = 0.0f;

        private bool ClearDefined = false;

        private float _maxSeparation = 0.001f;
        private float _minRadius = 0.9f;
        private float _cornerVertices = 16;

        public UnityEvent onClear;
        public RenderTexture canvasTexture;

        private int lineCount = 0;

        public int maxLines = 200;
        private CircularBuffer<TrailRenderer> _buffer;

        private CommandBuffer _com;
        public bool wantClear;

        private void Start()
        {
            try {
                Input.GetButton("Clear");
                ClearDefined = true;
            } catch (Exception) {
                Debug.LogWarning("Input 'Clear' is not bound! Define it for a shortcut to clear the paint canvas.");
                ClearDefined = false;
            }

            _buffer = new CircularBuffer<TrailRenderer>(maxLines);

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
            if (pt.paintColor == Color.clear) 
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
            if (pt.paintColor == Color.clear) 
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

            tr.transform.parent = brushObject.transform;
            tr.transform.rotation = this.transform.rotation;
            tr.transform.localScale = this.transform.localScale;
            
            tr.startColor = pt.paintColor;
            Debug.Log(pt.paintColor.ToString());
            tr.endColor = pt.paintColor;
            tr.sortingOrder = lineCount++; // stack new lines over old ones
            tr.startWidth = pt.size / 25f;
            tr.endWidth = pt.size / 25f;
            // increase vertex separation with thicker lines. looks better
            tr.minVertexDistance = pt.size / 200f;
            tr.numCapVertices = 32;
            tr.numCornerVertices = 0;

            brushObject.transform.position = first.point + first.normal * (first.separation - 0.001f);
            brushObject.transform.rotation = Quaternion.LookRotation(first.normal);
            brushObject.transform.parent = this.transform;
            
            // Set scale in world space
            brushObject.transform.localScale = Vector3.one * pt.size;

            tr.transform.position = brushObject.transform.position;
            tr.emitting = true;

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
            BufferLine(tr);
            tr.emitting = false;

            RenderMesh(tr, tr.sharedMaterial, Vector3.zero);
        }
        
        [Conditional("RENDER_PAINT")]
        private void InitRender()
        {
            canvasTexture = new RenderTexture(8752, 6108, 24);
            canvasTexture.Create();
            _com = new CommandBuffer();
            
            var ren = GetComponent<MeshRenderer>();
            ren.material.mainTexture = canvasTexture;
        }
        
        [Conditional("RENDER_PAINT")]
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
            
            trail.Clear();
        }
        
#if RENDER_PAINT
        public void OnWillRenderObject()
        {
            if (wantClear)
            {
                wantClear = false;
                _com.SetRenderTarget(canvasTexture);
                _com.ClearRenderTarget(true, true, Color.clear);
            }

            Graphics.ExecuteCommandBuffer(_com);
            _com.Clear();
        }
#endif

        private void BufferLine(TrailRenderer tr)
        {
            if (_buffer.PushFront(tr)) 
                return;
            
            var last = _buffer.PopBack();
            last.Clear();
            _buffer.PushFront(tr);
        }

        public void Clear()
        {
            wantClear = true;
            foreach (Transform child in this.transform) {//Clear brushes
                Destroy(child.gameObject);
            }
            CleanHands();
            _buffer.Clear();
        }
        
        public static void CleanHands()
        {
            PaintType[] paintBlobs = GameObject.FindObjectsOfType<PaintType>();
            foreach (PaintType pt in paintBlobs) {
                pt.gameObject.SetActive(false);
                Destroy(pt.gameObject);
            }
        }

        //deletes the line at the top of the stack
        //this won't work when rendering to a texture. Investigate later?
        public void UndoLast()
        {
            if (_buffer.Count < 1)
                return;
            
            var tr = _buffer.PopFront();
            tr.Clear();
        }
    }
}
