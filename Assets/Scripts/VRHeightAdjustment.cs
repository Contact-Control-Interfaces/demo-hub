using Leap.Unity;
using Maestro;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.XR;

public class VRHeightAdjustment : MonoBehaviour
{
    private GameObject rigGameObject;
    private float maxY;

    [Range(0.0f, 100f)]
    [SerializeField] int heightPercentage;

    void Start()
    {
        rigGameObject = Camera.main.gameObject;

        maxY = rigGameObject.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (maxY < rigGameObject.transform.position.y)
        {
            maxY = rigGameObject.transform.position.y;
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
        this.transform.position = tempPos;
    }

    public void SceneHeightOverride()
    {
        maxY = rigGameObject.transform.position.y;
        AdjustSceneHeight();
    }
}