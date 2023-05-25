using Leap.Unity;
using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.XR;

public class VRHeightAdjustment : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] GameObject rigTransform;
    [SerializeField] GameObject changeObject;

    //Remove Later
    //Used for Visual Debugging
    [SerializeField] float maxY;
    [SerializeField] float minY = 0;
    [SerializeField] float demoTableY;

    [Range(0.0f, 100f)]
    [SerializeField] int heightPercentage;

    void Start()
    {
        maxY = rigTransform.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(maxY < rigTransform.transform.position.y)
        {
            maxY = rigTransform.transform.position.y;
            AdjustSceneHeight();
        }
    }

    public void AdjustSceneHeight()
    {
        Vector3 tempPos = changeObject.transform.position;
        tempPos.y = maxY * ((float)heightPercentage / 100f);
        Debug.Log(tempPos.y);
        changeObject.transform.position = tempPos;
        demoTableY = tempPos.y;
    }


    public void ManualAdjustUp()
    {
        rigTransform.transform.position += new Vector3(0, .1f, 0);
    }

    public void ManualAdjustDown()
    {
        rigTransform.transform.position -= new Vector3(0, .1f, 0);
    }
}