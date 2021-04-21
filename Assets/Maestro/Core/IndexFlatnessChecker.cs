using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class IndexFlatnessChecker : FlatnessChecker
{
    SteamVR_Behaviour_Skeleton parent;
    private bool flat;

    public float total;
    public float threshold = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        parent = this.GetComponent<SteamVR_Behaviour_Skeleton>();
    }

    private float sum(float[] floats)
    {
        float result = 0.0f;

        for (int i = 0; i < floats.Length; i++)
            result += floats[i];

        return result;
    }

    // Update is called once per frame
    void Update()
    {
        total = sum(parent.fingerCurls);
        flat = total <= threshold;
    }

    public override bool isFlat()
    {
        return flat;
    }
}
