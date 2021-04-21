using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DribbleLengthener : MonoBehaviour
{

    public static float MinDribbleWait = 0.20f;

    public float MinWait = 0.20f;

    // Update is called once per frame
    void Update()
    {
        MinDribbleWait = MinWait;
    }
}
