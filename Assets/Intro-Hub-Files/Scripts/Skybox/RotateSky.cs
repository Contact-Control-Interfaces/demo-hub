using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSky : MonoBehaviour
{
    public float rotateSpeed = 1000f;
    //public Skybox skybox;

    private void Update()
    {
       
        RenderSettings.skybox.SetFloat("_Rotation", Time.time);
        Debug.Log(RenderSettings.skybox.GetFloat("_Rotation"));
    }
}
