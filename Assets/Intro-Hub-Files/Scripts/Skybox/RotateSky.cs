using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSky : MonoBehaviour
{
    [Header("Rotation Speed")]
    public float rotateSpeed;
    //public Skybox skybox;

    private void Update()
    {
       
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotateSpeed);
        Debug.Log(RenderSettings.skybox.GetFloat("_Rotation"));
    }
}
