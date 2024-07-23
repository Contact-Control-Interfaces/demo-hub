using Leap;
using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrationURP : MonoBehaviour
{
    public float speed;
    public float displacementAmount;
    public float maxDisplacementAmount;
    public MeshRenderer meshRender;

    [ColorUsage(true, true)]
    public Color color;
    [ColorUsage(true, true)]
    public Color _GlowColor;
    [ColorUsage(true, true)]
    public Color _DarkGlow;

    // Start is called before the first frame update
    void Start()
    {
        meshRender = GetComponent<MeshRenderer>();
        meshRender.material.color = color;
        meshRender.material.SetColor("_GlowColor", _GlowColor);
        meshRender.material.SetColor("_DarkGlow", _DarkGlow);
    }

    // Update is called once per frame
    void Update()
    {
        if (displacementAmount < 0.0003)
        {
            displacementAmount = 0f;
        }
        else 
        {   
            displacementAmount = Mathf.Lerp(displacementAmount, 0, Time.deltaTime);
            meshRender.material.SetFloat("_Amount", displacementAmount);
        }
    }

    public void OnTriggerEnter(Collider collider) {
            meshRender.material.SetFloat("_Speed", speed);
            displacementAmount = maxDisplacementAmount;
    }

    public void OnCollisionEnter(Collision collision)
    {
            meshRender.material.SetFloat("_Speed", speed);
            displacementAmount = maxDisplacementAmount;
    }
}
