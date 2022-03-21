using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrationShaderBehavior : MonoBehaviour
{
    public float MaxAmplitude;
    private float currentAmplitude = 0;
    MeshRenderer meshRender;

    // Start is called before the first frame update
    void Start()
    {
        meshRender = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        currentAmplitude = Mathf.Lerp(currentAmplitude, 0, Time.deltaTime);
        meshRender.material.SetFloat("_Amplitude", currentAmplitude);

    }

    public void onTouch()
    {
        currentAmplitude = MaxAmplitude;
    }

}
