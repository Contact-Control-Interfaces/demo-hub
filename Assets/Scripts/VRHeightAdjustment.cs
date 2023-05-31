using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class HeightAdjustmentEvent : UnityEvent<float> { }

public class VRHeightAdjustment : MonoBehaviour
{
    private GameObject rigGameObject;
    private float maxY;

    public HeightAdjustmentEvent AdjustmentEvent;

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
        float lastY = this.transform.position.y;
        float newY = maxY * ((float)heightPercentage / 100f);

        Vector3 tempPos = this.transform.position;
        tempPos.y = newY;
        this.transform.position = tempPos;

        AdjustmentEvent?.Invoke(newY - lastY);
    }

    public void SceneHeightOverride()
    {
        maxY = rigGameObject.transform.position.y;
        AdjustSceneHeight();
    }
}