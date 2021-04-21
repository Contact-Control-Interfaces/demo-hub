using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PaintCanvas : MonoBehaviour {

    public GameObject brush;
    public float brushSize = 1f;
    public int maxDots = 10000;

    private int paintLayer, paintMask;

    public float scale = 10f;
    public float dist = 0.1f;
    public byte effect = 128;

    private AudioSource source;

    private float waitTime = 0.05f;
    private float elapsed = 0.0f;

    public UnityEvent onClear;

    private bool ClearDefined = false;

    private void Start()
    {
        try
        {
            Input.GetButton("Clear");
            ClearDefined = true;
        } catch (Exception)
        {
            Debug.LogWarning("Input 'Clear' is not bound! Define it for a shortcut to clear the paint canvas.");
            ClearDefined = false;
        }

        paintLayer = LayerMask.NameToLayer("Paint");
        if (paintLayer < 0)
            Debug.LogWarning("Layer 'Paint' is not defined! Please define it to enable the paint canvas to function");
        else
            this.gameObject.layer = paintLayer;

        paintMask = 1 << paintLayer;

        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (ClearDefined && Input.GetButton("Clear") && onClear != null) {
            Clear();
            onClear.Invoke();
        }


        elapsed += Time.deltaTime;
        if (elapsed > waitTime && source.isPlaying)
        {
            source.Stop();
        }
    }

    public RenderTexture canvasTexture;

    private void OnCollisionStay(Collision collision)
    {

        FingerTipCollider ftc = collision.gameObject.GetComponent<FingerTipCollider>();
        if (ftc != null /*&& ftc.PaintColor != Color.clear*/)
        {
            if (ftc.PaintColor != Color.clear && ftc.PaintColor != FingerPaint.eraseColor)
            {
                if (!source.isPlaying)
                {
                    source.Play();
                }
                elapsed = 0.0f;



                GameObject brushObject = Instantiate(brush);
                //brushObject.layer = paintLayer;
                brushObject.transform.parent = null;
                SpriteRenderer sr = brushObject.GetComponent<SpriteRenderer>();
                sr.color = ftc.PaintColor;
                sr.sortingOrder = this.transform.childCount;

                //Find canvas plane
                RaycastHit hit;
                Ray temp = new Ray(collision.contacts[0].point - collision.contacts[0].normal * dist, collision.contacts[0].normal);
                Physics.Raycast(temp, out hit, dist * 2, paintMask);

                brushObject.transform.position = hit.point - collision.contacts[0].normal * 0.001f;
                brushObject.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);
                brushObject.transform.parent = this.transform;
                brushObject.transform.localScale = Vector3.one * brushSize;
            }

            if (ftc.vocb != null)
            {
                ftc.vocb.PaintEffect = effect;
            }

            //ftc.pocb.Amplitude = (byte)(80 + ftc.rb.velocity.magnitude * scale);
            if (ftc.pocb != null)
            {
                ftc.pocb.PaintAmp = 255;
                ftc.rend.enabled = false;
            }
        }

        if (this.transform.childCount > maxDots)
        { //If we reach the max brushes available, flatten the texture and clear the brushes
            //brushCursor.SetActive(false);
            //saving = true;
            Invoke("SaveTexture", 0.1f);

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        FingerTipCollider ftc = collision.gameObject.GetComponent<FingerTipCollider>();
        if (ftc != null/*&& ftc.PaintColor != Color.clear*/)
        {
            if (ftc.vocb != null)
                ftc.vocb.PaintEffect = null;

            if (ftc.pocb != null)
            {
                ftc.pocb.Amplitude = null;
                ftc.pocb.PaintAmp = null;
            }
        }
    }

    public void Clear()
    {
        foreach (Transform child in this.transform)
        {//Clear brushes
            Destroy(child.gameObject);
        }
    }

    void SaveTexture()
    {
        System.DateTime date = System.DateTime.Now;
        RenderTexture.active = canvasTexture;
        Texture2D tex = new Texture2D(canvasTexture.width, canvasTexture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, canvasTexture.width, canvasTexture.height), 0, 0, false);
        tex.Apply();
        RenderTexture.active = null;
        this.GetComponent<Renderer>().material.mainTexture = tex; //Put the painted texture as the base


        Clear();
        //StartCoroutine ("SaveTextureToFile"); //Do you want to save the texture? This is your method!
        //Invoke("ShowCursor", 0.1f);
    }

    /*IEnumerator SaveTextureToFile(Texture2D savedTexture)
    {
        //brushCounter = 0;
        string fullPath = System.IO.Directory.GetCurrentDirectory() + "\\UserCanvas\\";
        System.DateTime date = System.DateTime.Now;
        string fileName = "CanvasTexture.png";
        if (!System.IO.Directory.Exists(fullPath))
            System.IO.Directory.CreateDirectory(fullPath);
        var bytes = savedTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(fullPath + fileName, bytes);
        Debug.Log("<color=orange>Saved Successfully!</color>" + fullPath + fileName);
        yield return null;
    }*/
}
