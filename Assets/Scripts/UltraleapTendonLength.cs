using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltraleapTendonLength : MonoBehaviour
{
    private LeapProvider provider;

    private void Awake()
    {
        provider = this.GetComponentInChildren<LeapProvider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
