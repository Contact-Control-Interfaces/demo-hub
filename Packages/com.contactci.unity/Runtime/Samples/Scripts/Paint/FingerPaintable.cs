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
            tr.transform.position = vtx;
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
            tr.minVertexDistance = (desiredSize ) * _vertexSeparation;
            //tr.numCornerVertices = 128;
            //tr.numCapVertices = 64;
            
            //tr.transform.localScale = Vector3.one;

            brushObject.transform.position = first.point + first.normal * (first.separation - 0.001f);
            brushObject.transform.rotation = Quaternion.LookRotation(first.normal);
            brushObject.transform.parent = this.transform;
            
            // Set scale in world space
            //brushObject.transform.parent = null;
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

            if (collision.contactCount > 0)
            {
                var first = collision.contacts[0];

                var vtx = first.point - first.normal * _maxSeparation;
                tr.transform.position = vtx;
            }

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
            //var lookMatrix = Matrix4x4.LookAt(Vector3.forward, Vector3.back, Vector3.up);
            var lookMatrix = Matrix4x4.LookAt(ttr.position + ttr.forward, ttr.position, ttr.up);
            //var lookMatrix = Matrix4x4.LookAt(ttr.localPosition + ttr.forward, ttr.localPosition , ttr.up);
            //var lookMatrix = this.transform.localToWorldMatrix;
            var rotate = Quaternion.LookRotation(new Vector3(-1, 0, 0), new Vector3(0, 0, 1));
            var scaleMatrix = Matrix4x4.TRS(Vector3.back, rotate, Vector3.one);
            //var scaleMatrix = ttr.localToWorldMatrix;
            var viewMatrix = scaleMatrix * lookMatrix.inverse;
            var projMatrix = Matrix4x4.Perspective(600f, 1f, 1f, 100f);

            var transMatrix = Matrix4x4.TRS(offset,Quaternion.identity, Vector3.one  );
            
            var mesh = new Mesh();
            trail.BakeMesh(mesh);
            
            //_com.Clear();
            _com.SetRenderTarget(canvasTexture);
            _com.SetViewProjectionMatrices(viewMatrix, projMatrix);
            _com.DrawMesh(mesh, transMatrix, material, 0, 0);
        
        }

        private const string circle = "0.2910,1.1620,-0.7467:0.2884,1.1752,-0.7537:0.2732,1.1810,-0.7567:0.2661,1.1909,-0.7618:0.2656,1.2097,-0.7717:0.2643,1.2291,-0.7818:0.2621,1.2404,-0.7877:0.2579,1.2534,-0.7945:0.2535,1.2653,-0.8007:0.2488,1.2785,-0.8076:0.2432,1.2911,-0.8141:0.2376,1.3019,-0.8198:0.2296,1.3177,-0.8280:0.2229,1.3309,-0.8350:0.2159,1.3480,-0.8439:0.2093,1.3634,-0.8519:0.2017,1.3796,-0.8604:0.1944,1.3949,-0.8683:0.1894,1.4056,-0.8739:0.1834,1.4167,-0.8798:0.1761,1.4290,-0.8862:0.1696,1.4418,-0.8929:0.1642,1.4547,-0.8996:0.1576,1.4695,-0.9073:0.1499,1.4828,-0.9142:0.1417,1.4941,-0.9201:0.1274,1.4911,-0.9186:0.1218,1.4798,-0.9127:0.1176,1.4556,-0.9001:0.1139,1.4371,-0.8904:0.1118,1.4256,-0.8844:0.1081,1.4113,-0.8769:0.1049,1.3962,-0.8690:0.1016,1.3843,-0.8628:0.0894,1.3802,-0.8607:0.0726,1.3880,-0.8648:0.0599,1.3988,-0.8704:0.0504,1.4087,-0.8755:0.0419,1.4191,-0.8810:0.0330,1.4317,-0.8876:0.0242,1.4450,-0.8945:0.0164,1.4559,-0.9002:0.0095,1.4667,-0.9058:0.0026,1.4770,-0.9112:-0.0058,1.4890,-0.9175:-0.0183,1.5013,-0.9239:-0.0305,1.5092,-0.9281:-0.0451,1.5173,-0.9323:-0.0597,1.5246,-0.9361:-0.0718,1.5301,-0.9389:-0.0851,1.5353,-0.9416:-0.1008,1.5394,-0.9438:-0.1160,1.5413,-0.9448:-0.1309,1.5425,-0.9454:-0.1441,1.5428,-0.9456:-0.1587,1.5428,-0.9456:-0.1721,1.5408,-0.9445:-0.1894,1.5369,-0.9425:-0.2039,1.5316,-0.9397:-0.2149,1.5235,-0.9355:-0.2245,1.5093,-0.9281:-0.2300,1.4936,-0.9199:-0.2327,1.4808,-0.9132:-0.2344,1.4659,-0.9054:-0.2362,1.4455,-0.8948:-0.2372,1.4272,-0.8852:-0.2404,1.4043,-0.8732:-0.2418,1.3926,-0.8672:-0.2433,1.3768,-0.8589:-0.2442,1.3589,-0.8496:-0.2442,1.3450,-0.8423:-0.2438,1.3319,-0.8354:-0.2432,1.3166,-0.8275:-0.2426,1.3010,-0.8193:-0.2427,1.2891,-0.8131:-0.2431,1.2709,-0.8036:-0.2440,1.2567,-0.7962:-0.2462,1.2392,-0.7871:-0.2475,1.2248,-0.7795:-0.2486,1.2112,-0.7724:-0.2491,1.1996,-0.7664:-0.2494,1.1860,-0.7593:-0.2497,1.1725,-0.7522:-0.2502,1.1596,-0.7455:-0.2513,1.1478,-0.7394:-0.2518,1.1356,-0.7330:-0.2535,1.1233,-0.7266:-0.2597,1.1101,-0.7197:-0.2692,1.0989,-0.7138:-0.2812,1.0918,-0.710";

        private void DebugCircle()
        {
            var lr = this.gameObject.AddComponent<TrailRenderer>();
            lr.transform.parent = this.transform;
            lr.transform.rotation = this.transform.rotation;
            lr.transform.localScale = this.transform.localScale;
            
            lr.material = Resources.FindObjectsOfTypeAll<Material>().First(mt => mt.name == "BrushMaterial");

            lr.startWidth = 5;
            lr.endWidth = 5;
            lr.startColor = Color.green;
            lr.endColor=Color.green;

            var vtxs = circle.Split(':');
            foreach (string s in vtxs)
            {
                var comps = s.Split(',');

                var vtx = new Vector3(float.Parse(comps[0]), float.Parse(comps[1]), float.Parse(comps[2]));
                lr.AddPosition(vtx);
            }

            //lr.emitting = false;
            RenderMesh(lr, lr.sharedMaterial, Vector3.zero);
            //lr.Clear();
            //Destroy(lr);
        }
        private void DebugLine()
        {
            var lr = this.gameObject.AddComponent<TrailRenderer>();
            lr.transform.parent = this.transform;
            lr.transform.rotation = this.transform.rotation;
            lr.transform.localScale = this.transform.localScale;
            
            lr.material = Resources.FindObjectsOfTypeAll<Material>().First(mt => mt.name == "BrushMaterial");

            lr.startWidth = 10;
            lr.endWidth = 20;
            lr.startColor = Color.white;
            lr.endColor=Color.yellow;
            
            lr.AddPosition(new Vector3(-50,-50,0));
            lr.AddPosition(new Vector3(50,50,0));
            lr.emitting = false;
            
            RenderMesh(lr, lr.sharedMaterial, Vector3.zero);
            
            lr.Clear();
            lr.emitting = true;
            lr.startWidth = 1;
            lr.endWidth = 1;
            lr.startColor = Color.cyan;
            lr.endColor = Color.cyan;

            float radius = 5f;
            
            for (int i = 1; i <= 100; i++)
            {
                float theta = Mathf.Lerp(0f, Mathf.PI * 2, i / 100f);
                float y = radius * Mathf.Sin(theta);
                float x = radius * Mathf.Cos( theta);
                
                lr.AddPosition(new Vector3(x , y, 0));
            }

            RenderMesh(lr, lr.sharedMaterial, Vector3.zero);
            lr.Clear();
            
            lr.startColor = Color.green;
            lr.endColor = Color.red;
            
            lr.AddPosition(new Vector3(0f,0f,0f));
            lr.AddPosition(new Vector3(0f,5f,2.5f));
            lr.AddPosition(new Vector3(5f,5f,5f));
            lr.AddPosition(new Vector3(5f,0f,2.5f));
            lr.AddPosition(new Vector3(0f,0f,0f));
            
            RenderMesh(lr, lr.sharedMaterial, Vector3.zero);
            
            lr.Clear();

            
            Destroy(lr);
        }

        public void OnWillRenderObject()
        {
            if(!ENABLE_RENDER)
                return;
            
            // if (_com == null || canvasTexture == null)
            //     return;
            
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
        }
    }
}
