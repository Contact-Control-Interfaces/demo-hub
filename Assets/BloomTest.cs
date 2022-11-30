using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using Bloom = UnityEngine.Rendering.Universal.Bloom;

public class BloomTest : MonoBehaviour
{

    [Header("Bloom Variables")]
    public Volume postVolume;
    public Bloom bloomEffect;
    
    public float bloomIntensity;

    private bool BloomUp;
    private bool BloomDown;

    public VolumeProfile universalBloom;


    // Update is called once per frame
    void Update()
    {
        TestBloom();
        if (BloomUp)
        {
            if (bloomEffect.intensity.value <= bloomIntensity)
            {
                bloomEffect.intensity.value += 5f;
                Debug.Log(bloomEffect.intensity.value.ToString());
            }
            else
            {
                BloomDown = true;
                BloomUp = false;
            }
        }
        else if (BloomDown)
        {

            if (bloomEffect.intensity.value > 0f)
            {
                bloomEffect.intensity.value -= 10f;
            }
            else
            {
                Debug.Log("BloomDone");
                BloomDown= false;
            }
        }



    }

    private void TestBloom()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            //var volume = this.GetComponent<Volume>();
            //if (volume.profile.TryGet<Bloom>(out bloomEffect))
            if(universalBloom.TryGet<Bloom>(out bloomEffect))
            {
                bloomEffect.intensity.value = 0f;
                Debug.Log("Bloom");
                BloomUp = true;
            }
        }
    }
}
