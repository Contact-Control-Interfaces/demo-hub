using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class HeightAdjustment : MonoBehaviour
{
    [SerializeField] GameObject playerRig;

    public void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            ManualAdjustUp();
        }

        if(Input.GetKey(KeyCode.DownArrow))
        {
            ManualAdjustDown();
        }
    }

    public void ManualAdjustUp()
    {
        playerRig.transform.position += new Vector3(0, .01f, 0);
    }

    public void ManualAdjustDown()
    {

        playerRig.transform.position -= new Vector3(0, .01f, 0);
    }
}
