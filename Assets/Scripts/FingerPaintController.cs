using Maestro;
using Maestro.Core.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using static Leap.Finger;

public class FingerPaintController : MonoBehaviour
{
    [SerializeField]
    private Transform[] FingerTips;

    [SerializeField]
    private Transform PaintablePlane;
    [SerializeField]
    private Collider PlaneCollider;

    [SerializeField]
    private float tipRadius = 0.02f;

    [SerializeField]
    private LayerMask layerMask;

    private Dictionary<Transform, PaintType> fingerPaints = new Dictionary<Transform, PaintType>();
    private List<GameObject> paintTrails = new List<GameObject>();
    private CircularBuffer<TrailRenderer> trailRendererBuffer;

    private int lineCount;
    private float _minRadius = 0.9f;
    private float _cornerVertices = 16;
    private float elapsed;
    private float waitTime = 0.05f;
    private int maxLines = 200;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = PaintablePlane.GetComponent<AudioSource>();
        trailRendererBuffer = new CircularBuffer<TrailRenderer>(maxLines);
    }

    private void OnDrawGizmos()
    {
        foreach(Transform tip in FingerTips)
        {
            Gizmos.DrawSphere(tip.transform.position + -tip.right * .001f, tipRadius);
        }
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed > waitTime && audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    private bool DetectPaintCollision(Transform fingerTip)
    {
        return Physics.OverlapSphere(fingerTip.position, tipRadius, layerMask).Length > 0;
    }

    private void FixedUpdate()
    {
        foreach(Transform fingerTip in FingerTips)
        {
            if(DetectPaintCollision(fingerTip))
            {
                PaintType paint = fingerTip.GetComponentInChildren<PaintType>();
                if (!paint) 
                {
                    continue; 
                }
                if (!fingerPaints.ContainsKey(fingerTip))
                {
                    fingerPaints.Add(fingerTip, fingerTip.GetComponentInChildren<PaintType>());
                    elapsed = 0.0f;
                    InitSplotch(fingerTip);
                }
                else
                {
                    if (audioSource != null && !audioSource.isPlaying)
                    {
                        audioSource.Play();
                    }
                    elapsed = 0.0f;
                    ContinueSplotch(fingerTip);
                }
            }
            else if (fingerPaints.ContainsKey(fingerTip))
            {
                EndSplotch(fingerTip);
                fingerPaints.Remove(fingerTip);
            }
        }
    }

    private void InitSplotch(Transform fingerTip)
    {
        var pt = fingerPaints[fingerTip];
        GameObject brushObject = Instantiate(pt.splotchPrefab);
        paintTrails.Add(brushObject);
        var tr = brushObject.GetComponent<TrailRenderer>();

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

        //Create a splotch at the contact point, and move it above the normal of the object
        Vector3 paintPos = PlaneCollider.ClosestPoint(fingerTip.position);
        brushObject.transform.position = paintPos;
        brushObject.transform.rotation = Quaternion.LookRotation(PaintablePlane.transform.up);
        brushObject.transform.parent = PaintablePlane;

        // Set scale in world space
        brushObject.transform.localScale = Vector3.one * pt.size;

        tr.transform.position = brushObject.transform.position;
        tr.emitting = true;

        pt.splotch = brushObject;
    }

    private void ContinueSplotch(Transform fingerTip)
    {
        PaintType pt = fingerPaints[fingerTip];
        if (pt.paintColor == Color.clear)
            return;

        var tr = pt.splotch.GetComponent<TrailRenderer>();
        Vector3 paintPos = PlaneCollider.ClosestPoint(fingerTip.position);
        var vtx = new Vector3(paintPos.x, paintPos.y, paintPos.z);
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

    private void EndSplotch(Transform fingerTip)
    {
        PaintType pt = fingerPaints[fingerTip];
        var tr = pt.splotch.GetComponent<TrailRenderer>();
        if (tr == null)
            return;
        Vector3 paintPos = PlaneCollider.ClosestPoint(fingerTip.position);
        var vtx = new Vector3(paintPos.x, paintPos.y, paintPos.z);
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
            rdir = vtx + Vector3.one * _minRadius;
            AddPoint(tr, rdir);
            rdir = -rdir.normalized;
        }

        var rpos = tr.transform.position + rdir * tr.minVertexDistance;
        AddPoint(tr, rpos);
        tr.transform.position = rpos;
        BufferLine(tr);
        tr.emitting = false;
    }

    public void ClearCanvas()
    {
        foreach (GameObject paintTrail in paintTrails)
        {
            //Clear brushes
            Destroy(paintTrail);
        }
        paintTrails.Clear();
        trailRendererBuffer.Clear();
    }

    public void CleanHands()
    {
        PaintType[] paintBlobs = FindObjectsOfType<PaintType>();
        foreach (PaintType pt in paintBlobs)
        {
            pt.gameObject.SetActive(false);
            Destroy(pt.gameObject);
        }
    }

    public void AdjustLineHeights(float deltaY)
    {
        foreach (TrailRenderer tr in trailRendererBuffer)
        {
            AdjustLineHeight(tr, deltaY);
        }
    }

    private void AdjustLineHeight(TrailRenderer tr, float deltaY)
    {
        if (tr != null)
        {
            for (int i = 0; i < tr.positionCount; i++)
            {
                tr.SetPosition(i, tr.GetPosition(i) + (Vector3.up * deltaY));
            }
        }
    }

    private void BufferLine(TrailRenderer tr)
    {
        if (trailRendererBuffer.PushFront(tr))
            return;

        var last = trailRendererBuffer.PopBack();
        last.Clear();
        trailRendererBuffer.PushFront(tr);
    }


    public void UndoLast()
    {
        if (trailRendererBuffer.Count < 1)
            return;

        var tr = trailRendererBuffer.PopFront();
        tr.Clear();
    }
}
