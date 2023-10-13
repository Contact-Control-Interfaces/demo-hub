using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextMeshProPanelText : PanelText
{
    public TextMeshProUGUI textMesh;

    private void Start()
    {
        if (textMesh == null) {
            Debug.LogError("No PanelText set for center panel!");
            this.enabled = false;
        }
    }

    public override void SetText(string text)
    {
        textMesh?.SetText(text);
    }
}
