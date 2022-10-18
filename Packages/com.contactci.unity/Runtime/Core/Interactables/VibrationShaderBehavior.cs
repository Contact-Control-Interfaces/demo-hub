using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrationShaderBehavior : MonoBehaviour
{
    [Range(0,1)]
    public float MaxAmplitude;
    [Range(0,10)]
    public float Frequency = -1;
    [Range(0,5)]
    public float Speed = -1;
    [Range(1,100)]
    public float Sharpness = -1;
    [Range(0,8)]
    public int Plurality = -1;
    [Range(0,6.28f)]
    public float PluralityPhase = -1;
    public Color? Color = null;
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
        if (Color != null)
            meshRender.material.SetColor("_Color", Color.Value);
        if (Speed >= 0)
            meshRender.material.SetFloat("_Speed", Speed);
        if (Frequency >= 0)
            meshRender.material.SetFloat("_Frequency", Frequency);
        if (Sharpness >= 0)
            meshRender.material.SetFloat("_Sharpness", Sharpness);
        if (Plurality >= 0)
            meshRender.material.SetInt("_Plurality", Plurality);
        if(PluralityPhase >= 0)
            meshRender.material.SetFloat("_Phase", PluralityPhase);        
    }

    public void onTouch()
    {
        currentAmplitude = MaxAmplitude;
    }

}
