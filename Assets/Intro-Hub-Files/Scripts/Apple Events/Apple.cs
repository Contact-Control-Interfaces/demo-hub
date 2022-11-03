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
    [Header("Bloom Variables")]
    public Volume postVolume;
    public Bloom bloomEffect;
    [Range(1f, 1000f)]
    public float bloomIntensity;
    [Space(10)]

    [Header("Shaders")]
    public Material glowOff;
    public Material glowOn;
    public Material hologramGlow;
    [Space(10)]

    [Header("Effected Objects")]
    public GameObject countDisplay;
    public GameObject panelDisplay;
    public GameObject tree;
    public GameObject touchTrigger;
    public GameObject dropObject;
    [Header("Dropped Object Reset Point")]
    public Transform dropOrigin;

 

    private bool BloomUp;
    private bool BloomDown;
    private bool materialChange;
    private bool EffectDone;

    private void Start()
    {
        panelDisplay.SetActive(false);
    }

    void Update()
    {
        if (this.GetComponent<Rigidbody>().isKinematic == false && !EffectDone)
        {
            dropObject.GetComponent<Renderer>().material = glowOn;
        }
        if (dropObject.transform.position.y <= 2.2 && !EffectDone)
        {
            ResetApple();
        }

        if (!EffectDone)
        {
            TestBloom();
            if (BloomUp)
            {
                if (bloomEffect.intensity.value <= bloomIntensity)
                {
                    bloomEffect.intensity.value += 5f;
                }
                else
                {
                    TreeDisable();
                    DisplayPanel();
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
                    if (!materialChange)
                    {
                        BloomDown = false;
                        bloomEffect.intensity.value = 100f;
                        MaterialChange();
                        touchTrigger.SetActive(false);
                        countDisplay.SetActive(false);
                        EffectDone = true;
                    }
                }
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "MainCamera")
        {
            touchTrigger.SetActive(false);
            countDisplay.SetActive(false);
            var volume = this.GetComponent<Volume>();
            if (volume.profile.TryGet<Bloom>(out bloomEffect))
            {
                bloomEffect.intensity.value = 0f;
                Debug.Log("Bloom");
                BloomUp = true;
            }
        }
    }

    private void TestBloom()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            
            var volume = this.GetComponent<Volume>();
            if (volume.profile.TryGet<Bloom>(out bloomEffect))
            {
                bloomEffect.intensity.value = 0f;
                Debug.Log("Bloom");
                BloomUp = true;
            }
        }
    }




    void MaterialChange()
    {
        materialChange = true;
        bloomEffect.intensity.value = 5f;
        this.GetComponent<Renderer>().material = glowOff;
    }

    void DisplayPanel()
    {
        panelDisplay.SetActive(true);
    }

    void TreeDisable()
    {
        tree.SetActive(false);
    }

    private void ResetApple()
    {
        touchTrigger.SetActive(true);
        countDisplay.SetActive(true);
        dropObject.GetComponent<Renderer>().material = hologramGlow;
        dropObject.GetComponent<Rigidbody>().isKinematic = true;
        dropObject.transform.position = dropOrigin.position;
    }
}
