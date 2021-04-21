using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableLight : MonoBehaviour
{
    public new Light light;

    public void SetLight(bool isOn)
    {
        light.gameObject.SetActive(isOn);
        light.enabled = isOn;
    }
}
