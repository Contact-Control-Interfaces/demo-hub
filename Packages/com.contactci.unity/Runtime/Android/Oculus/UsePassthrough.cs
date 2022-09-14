using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OVRManager))]
[RequireComponent(typeof(OVRCameraRig))]
[RequireComponent(typeof(OVRPassthroughLayer))]
public class UsePassthrough : MonoBehaviour
{
    public bool enablePassthrough;
    private bool lastEnablePassthrough;
    public bool renderRealHandsOnTop;
    private bool lastRenderRealHandsOnTop = false;

    [Space]
    public Material passthroughMaterial;
    public GameObject[] disableDuringPassthrough;

    private OVRPassthroughLayer passthru;
    private OVRManager manager;

    // we assume the hands all have the same material
    private Material initialMaterial; 
    private OVRHand[] modifiedHands;
    private CameraClearFlags initialFlags;

    void Awake()
    {
        passthru = this.GetComponent<OVRPassthroughLayer>();
        manager = this.GetComponent<OVRManager>();

        if (renderRealHandsOnTop)
            ApplyHandMaterial();

        lastRenderRealHandsOnTop = renderRealHandsOnTop;
    }

    void Update()
    {
        if (renderRealHandsOnTop ^ lastRenderRealHandsOnTop) {
            if (renderRealHandsOnTop)
                ApplyHandMaterial();
            else
                UnApplyHandMaterial();
        }

        if (enablePassthrough ^ lastEnablePassthrough) {
            TogglePassthrough();
        }

        lastRenderRealHandsOnTop = renderRealHandsOnTop;
        lastEnablePassthrough = enablePassthrough;
    }

    public void Toggle()
    {
        enablePassthrough = !enablePassthrough;
    }

    public void ToggleHands()
    {
        renderRealHandsOnTop = !renderRealHandsOnTop;
    }

    public void ToggleEdges()
    {
        passthru.edgeRenderingEnabled = !passthru.edgeRenderingEnabled;
    }

    private void TogglePassthrough()
    {
        manager.isInsightPassthroughEnabled = enablePassthrough;
        passthru.enabled = enablePassthrough;
        
        Transform trackingSpace = this.transform.GetChild(0);
        var camera = trackingSpace?.Find("CenterEyeAnchor")?.GetComponent<Camera>();
        ToggleCamera(camera);

        foreach (GameObject go in disableDuringPassthrough) {
            go.SetActive(!enablePassthrough);
        }
    }

    private void ToggleCamera(Camera cam)
    {
        if (cam == null)
            return;

        if (enablePassthrough) {
            initialFlags = cam.clearFlags;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
        } else {
            cam.clearFlags = initialFlags;
        }
    }

    private void ApplyHandMaterial()
    {
        if (passthroughMaterial == null) {
            Debug.LogError("No material specified for passthrough hands!");
            return;
        }

        // We gotta find the hands
        modifiedHands = this.GetComponentsInChildren<OVRHand>();
        if (modifiedHands != null && modifiedHands.Length > 0) {
            initialMaterial = SetMaterials(modifiedHands, passthroughMaterial);
        }
    }

    private void UnApplyHandMaterial()
    {
        if (initialMaterial != null)
            SetMaterials(modifiedHands, initialMaterial);
    }

    /// <summary>
    /// Sets the Material for some amount of OVRHands, returns the Material overwritten.
    /// </summary>
    /// <param name="hands">OVRHands to modify</param>
    /// <param name="mat">Material to be used for the hand meshes</param>
    /// <returns>The previous Material used for the hands, assuming they're all the same</returns>
    private Material SetMaterials(OVRHand[] hands, Material mat)
    {
        Material result = null;

        foreach (OVRHand hand in hands) {
            var renderer = hand.GetComponentInChildren<SkinnedMeshRenderer>();

            if (result == null) {
                result = renderer.material;
            }
            renderer.material = mat;
        }

        return result;
    }
}
