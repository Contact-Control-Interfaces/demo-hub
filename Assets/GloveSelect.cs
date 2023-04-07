using Leap;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using Maestro;

public class GloveSelect : MonoBehaviour
{
    public enum DemoMode
    {
        None,
        OneGlove,
        TwoGlove
    }

    public DemoMode mode;
    public Color selectColor;
    public GloveSelect otherButton;

    public void UpdateGloveMode()
    {
        ConfigSettings.demoMode = mode;
        this.gameObject.GetComponent<Image>().color = selectColor;
        otherButton.GetComponent<Image>().color = new Color(255,255, 255);
    }
}
