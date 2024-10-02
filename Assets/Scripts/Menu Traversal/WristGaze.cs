using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WristGaze : MonoBehaviour
{
    public Transform playerHead;
    public GameObject wristMenuObj;
    public WristMenu wristMenu;
    public ObjectFill fillHandler;
    public Material wristBand;

    private float curDistance;
    private float wristThreshold = 0.80f;
    private float triggerDistance = .4f;

    void Start()
    {
        if (wristMenu == null)
        {
             wristMenu = FindAnyObjectByType<WristMenu>();
        }
    }

    public bool CheckWrist(Vector3 A, Vector3 B)
    {
        curDistance = Vector3.Distance(wristMenuObj.transform.position, playerHead.transform.position);
        var lookPercentage = Vector3.Dot(A.normalized, B.normalized);

        ColorAdjust(lookPercentage);

        if(lookPercentage > wristThreshold)
        {
            return true;
        }

        return false;
    }

    public void ColorAdjust(float colorValue)
    {
        float valueClamp = Mathf.Clamp01(colorValue);
        wristBand.SetFloat("_DotProduct", valueClamp);
    }

    private void Update()
    {
        if (!wristMenu.menu.Active && wristMenuObj.activeSelf)
        {
            if (CheckWrist(this.transform.forward, wristMenuObj.transform.up) && curDistance <= triggerDistance)
            {
                fillHandler.Fill(false);
            }

            else
            {
                fillHandler.StopFill();
            }
        }
    }
}