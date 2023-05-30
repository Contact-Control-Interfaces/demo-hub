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

    private float maxY;

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

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            SceneHeightOverride();
        }
    }

    public void AdjustSceneHeight()
    {
        Vector3 tempPos = this.transform.position;
        tempPos.y = maxY * ((float)heightPercentage / 100f);
        Debug.Log(tempPos.y);
        this.transform.position = tempPos;
    }

    public void SceneHeightOverride()
    {
        maxY = rigTransform.transform.position.y;
        AdjustSceneHeight();
    }

}