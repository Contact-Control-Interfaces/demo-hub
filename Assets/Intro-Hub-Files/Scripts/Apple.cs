using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;
using static UnityEngine.GraphicsBuffer;
using Bloom = UnityEngine.Rendering.Universal.Bloom;

public class Apple : MonoBehaviour
{
    public Volume postVolume;
    public Bloom bloomEffect;

    private bool BloomUp;
    private bool BloomDown;

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "MainCamera")
        {

            var volume = this.GetComponent<Volume>();
            if (volume.profile.TryGet<Bloom>(out bloomEffect))
            {
                Debug.Log("Bloom");
                BloomUp = true;
            }
        }
        

    }

    



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(BloomUp)
        {
            if(bloomEffect.intensity != 1000)
            {
                bloomEffect.intensity.value += 5f;
            }
            else
            {
                BloomDown = true;
                BloomUp = false;
            }
        }
        else if(BloomDown)
        {

            if (bloomEffect.intensity != 0f)
            {   
                bloomEffect.intensity.value -= 5f;
            }
            else
            {
                Debug.Log("Done");
            }
        }
    }
}
