using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class RotateSky : MonoBehaviour
{
    [Header("Rotation Speed")]
    public float rotateSpeed;

    private const string RotationFieldName = "_Rotation";
    private float InitialRotation;

    private void Start()
    {
        float initialRotation = RenderSettings.skybox.GetFloat(RotationFieldName);
    }

    private void Update()
    {
        SetSkyboxRotation(Time.time * rotateSpeed);
    }

    private void OnApplicationQuit()
    {
        SetSkyboxRotation(InitialRotation);
    }

    private void SetSkyboxRotation(float rotation)
    {
        RenderSettings.skybox.SetFloat(RotationFieldName, rotation);
    }
}
