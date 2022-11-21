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

            displacementAmount = Mathf.Lerp(displacementAmount, 0, Time.deltaTime);
            meshRender.material.SetFloat("_Amount", displacementAmount);
            

            if (Input.GetKeyDown(KeyCode.U))
            {
                Debug.Log("Buh");
                meshRender.material.SetFloat("_Speed", speed);
                displacementAmount = maxDisplacementAmount;
                
            }

    }

    public void onTouch()
    {
        //displacementAmount = MaxAmount;
    }
}
