using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DescriptionSetter : MonoBehaviour
{
    private TextMesh parent;

    private void Start()
    {
        parent = this.GetComponent<TextMesh>();
    }
    public void SetText(float value)
    {
        int index = (int)(value * 128);
        parent.text = DRV2605Descriptions.get(index);
    }
}
