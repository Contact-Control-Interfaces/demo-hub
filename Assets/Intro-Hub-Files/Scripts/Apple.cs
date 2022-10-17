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
    public Material glowOff;
    public GameObject panelDisplay;

    private bool BloomUp;
    private bool BloomDown;
    private bool materialChange;

    private void Start()
    {
        panelDisplay.SetActive(false);
    }

    void Update()
    {
        if (BloomUp)
        {
            if (bloomEffect.intensity != 1000)
            {
                bloomEffect.intensity.value += 5f;
            }
            else
            {
                BloomDown = true;
                BloomUp = false;
            }
        }
        else if (BloomDown)
        {

            if (bloomEffect.intensity != 0f)
            {
                bloomEffect.intensity.value -= 5f;
            }
            else
            {
                Debug.Log("BloomDone");
                if (!materialChange)
                {
                    MaterialChange();
                    DisplayPanel();
                }
                else
                {
                    Debug.Log("All Done");
                }
            }
        }
    }


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




    void MaterialChange()
    {
        materialChange = true;
        this.GetComponent<Renderer>().material = glowOff;
    }

    void DisplayPanel()
    {
        panelDisplay.SetActive(true);
    }
}
