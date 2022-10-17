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

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "MainCamera")
        {

            var volume = this.GetComponent<Volume>();
            if (volume.profile.TryGet<Bloom>(out bloomEffect))
            {
                Debug.Log("Bloom");
                bloomEffect.intensity.value = 1000f;
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
        
    }
}
